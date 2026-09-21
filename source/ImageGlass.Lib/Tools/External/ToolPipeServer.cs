/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.SDK.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ImageGlass.Tools;


/// <summary>
/// Host-side handler for a single tool's named pipe connection.
/// Receives requests from the tool and dispatches them to host services.
/// Sends events to the tool.
/// </summary>
internal sealed class ToolPipeServer : IDisposable
{
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly Channel<string> _outboundMessages = Channel.CreateUnbounded<string>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });
    private readonly CancellationTokenSource _writeCts = new();
    private readonly Task _writerTask;
    private readonly string _toolId;

    // Pixel buffer management
    private readonly Dictionary<string, (MemoryMappedFile Mmf, string Path)> _activeBuffers = [];

    /// <summary>
    /// Gets the event subscriptions currently requested by the tool.
    /// </summary>
    internal ToolEventSubscriptions Subscriptions { get; private set; } = new();


    /// <summary>
    /// Creates a host-side pipe handler around one connected tool stream.
    /// </summary>
    public ToolPipeServer(NamedPipeServerStream pipeServer, string toolId)
    {
        _toolId = toolId;
        _reader = new StreamReader(pipeServer, leaveOpen: true);
        _writer = new StreamWriter(pipeServer, leaveOpen: true) { AutoFlush = true };
        _writerTask = Task.Run(WriteLoopAsync);
    }


    /// <summary>
    /// Runs the message loop, dispatching tool requests until cancellation or pipe close.
    /// </summary>
    public async Task RunMessageLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // Read one newline-delimited message from the pipe.
                var line = await _reader.ReadLineAsync(ct);
                if (line is null) break;

                // Ignore malformed JSON payloads so one bad message does not kill the session.
                ToolMessage? msg;
                try
                {
                    msg = JsonSerializer.Deserialize(line, ToolJsonContext.Default.ToolMessage);
                }
                catch
                {
                    continue;
                }
                if (msg is null) continue;

                await HandleToolMessageAsync(msg);
            }
        }
        catch (OperationCanceledException) { }
        catch (IOException) { }
    }


    /// <summary>
    /// Routes one decoded tool message to the appropriate handler.
    /// </summary>
    private async Task HandleToolMessageAsync(ToolMessage msg)
    {
        switch (msg.Type)
        {
            case MessageTypes.READ_PIXEL:
                await HandleReadPixelAsync(msg);
                break;

            case MessageTypes.GET_PIXEL_BUFFER:
                await HandleGetPixelBufferAsync(msg);
                break;

            case MessageTypes.RELEASE_PIXEL_BUFFER:
                HandleReleasePixelBuffer(msg);
                break;

            case MessageTypes.RUN_API:
                await HandleRunApiAsync(msg);
                break;

            case MessageTypes.GET_PHOTO_METADATA:
                await HandleGetPhotoMetadataAsync(msg);
                break;

            case MessageTypes.GET_PHOTO_LIST:
                await HandleGetPhotoListAsync(msg);
                break;

            case MessageTypes.GET_SOURCE_SIZE:
                await HandleGetSourceSizeAsync(msg);
                break;

            case MessageTypes.GET_SELECTION:
                await HandleGetSelectionAsync(msg);
                break;

            case MessageTypes.SET_SELECTION:
                await HandleSetSelectionAsync(msg);
                break;

            case MessageTypes.ENABLE_SELECTION:
                await HandleEnableSelectionAsync(msg);
                break;

            case MessageTypes.SUBSCRIBE_EVENTS:
                HandleSubscribeEvents(msg);
                break;

            case MessageTypes.GET_THEME_INFO:
                HandleGetThemeInfo(msg);
                break;
        }
    }


    #region Message Handlers


    /// <summary>
    /// Reads one rendered pixel from the active viewer.
    /// </summary>
    private async Task HandleReadPixelAsync(ToolMessage msg)
    {
        var req = DeserializePayload<ReadPixelRequest>(msg);
        if (req is null) { SendResponse(msg.RequestId, null); return; }

        var color = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (AppAPIProvider.GetViewer() is not { } viewer) return default;
            return viewer.GetColorAt(req.X, req.Y);
        });

        SendResponse(msg.RequestId, new ReadPixelResponse
        {
            R = color.R,
            G = color.G,
            B = color.B,
            A = color.A,
        });
    }


    /// <summary>
    /// Exports the current rendered bitmap to a temporary memory-mapped file for the tool.
    /// </summary>
    private async Task HandleGetPixelBufferAsync(ToolMessage msg)
    {
        var req = DeserializePayload<GetPixelBufferRequest>(msg);
        var selectionOnly = req?.SelectionOnly ?? false;

        // Capture the current bitmap on the UI thread.
        var bitmap = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (AppAPIProvider.GetViewer() is not { } viewer) return null;
            return viewer.GetRenderedBitmap(selectionOnly);
        });

        if (bitmap is null)
        {
            SendResponse(msg.RequestId, null);
            return;
        }

        try
        {
            // Write the bitmap to a scoped, exclusively-created temp file the tool maps read-only;
            // deleted on release/Dispose.
            var pixelsDir = BHelper.ConfigDir(Dir.Temporary);
            var tempPath = Path.Combine(pixelsDir, $"ig_pixels_{Guid.NewGuid():N}.bin");
            var byteCount = bitmap.ByteCount;

            using (var fs = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                unsafe
                {
                    var span = new ReadOnlySpan<byte>((void*)bitmap.GetPixels(), byteCount);
                    fs.Write(span);
                }
            }

            var mmf = MemoryMappedFile.CreateFromFile(
                tempPath, FileMode.Open, null,
                byteCount, MemoryMappedFileAccess.Read);
            var toolPath = BHelper.GetRealPlatformPath(tempPath);
            _activeBuffers[toolPath] = (mmf, tempPath);

            // Return the mapping metadata the tool needs to open and interpret the buffer.
            SendResponse(msg.RequestId, new GetPixelBufferResponse
            {
                MmfPath = toolPath,
                Width = bitmap.Width,
                Height = bitmap.Height,
                Stride = bitmap.RowBytes,
                ColorType = bitmap.ColorType.ToString(),
            });
        }
        finally
        {
            bitmap.Dispose();
        }
    }


    /// <summary>
    /// Releases a previously exported pixel buffer and deletes its backing temp file.
    /// </summary>
    private void HandleReleasePixelBuffer(ToolMessage msg)
    {
        var req = DeserializePayload<ReleasePixelBufferRequest>(msg);
        if (req?.MmfPath is not null && _activeBuffers.TryGetValue(req.MmfPath, out var entry))
        {
            entry.Mmf.Dispose();
            _activeBuffers.Remove(req.MmfPath);
            try { File.Delete(entry.Path); } catch { }
        }
    }


    /// <summary>
    /// Executes a host API command requested by the tool.
    /// </summary>
    private async Task HandleRunApiAsync(ToolMessage msg)
    {
        var req = DeserializePayload<RunApiRequest>(msg);
        if (req is null)
        {
            SendResponse(msg.RequestId, new RunApiResponse { Success = false, Error = "Invalid request" });
            return;
        }

        // Security: a tool may only invoke non-destructive host APIs over IPC.
        if (!Core.API.IsApiAllowedForTool(req.ApiName))
        {
            SendResponse(msg.RequestId, new RunApiResponse { Success = false, Error = $"API '{req.ApiName}' is not permitted for tools." });
            return;
        }

        try
        {
            // Marshal the request to the UI thread because host APIs may touch UI state.
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await (Core.API.RunApiAsync(req.ApiName, req.Argument) ?? Task.CompletedTask);
            });
            SendResponse(msg.RequestId, new RunApiResponse { Success = true });
        }
        catch (Exception ex)
        {
            SendResponse(msg.RequestId, new RunApiResponse { Success = false, Error = ex.Message });
        }
    }


    /// <summary>
    /// Returns the current photo metadata in the tool SDK shape.
    /// </summary>
    private async Task HandleGetPhotoMetadataAsync(ToolMessage msg)
    {
        // Snapshot the current photo and project only the fields exposed to tools.
        var meta = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var photo = Core.Photos?.Get(Core.Photos.CurrentIndex);
            if (photo is null) return null;

            var m = photo.Metadata;
            return new ToolPhotoMetadata
            {
                FilePath = photo.FilePath,
                FileName = m?.FileName ?? string.Empty,
                FileExtension = m?.FileExtension ?? string.Empty,
                FolderPath = m?.FolderPath ?? string.Empty,
                FolderName = m?.FolderName ?? string.Empty,
                FileSizeInBytes = m?.FileSizeInBytes ?? 0,
                FileCreationTimeUtc = m?.FileCreationTimeUtc ?? default,
                FileLastWriteTimeUtc = m?.FileLastWriteTimeUtc ?? default,

                OriginalWidth = (int)(m?.OriginalWidth ?? 0),
                OriginalHeight = (int)(m?.OriginalHeight ?? 0),
                Width = (int)(m?.Width ?? 0),
                Height = (int)(m?.Height ?? 0),
                Format = m?.FileExtension,
                FrameCount = (int)(m?.FrameCount ?? 1),
                CanAnimate = m?.CanAnimate ?? false,
                HasAlpha = m?.HasAlpha ?? false,

                ColorSpace = m?.ColorSpace.ToString(),
                ColorProfileName = m?.ColorProfileName,

                ExifRatingPercent = m?.ExifRatingPercent ?? 0,
                ExifDateTimeOriginal = m?.ExifDateTimeOriginal,
                ExifImageDescription = m?.ExifImageDescription,
                ExifModel = m?.ExifModel,
                ExifArtist = m?.ExifArtist,
                ExifCopyright = m?.ExifCopyright,
                ExifSoftware = m?.ExifSoftware,
                ExifExposureTime = m?.ExifExposureTime,
                ExifFNumber = m?.ExifFNumber,
                ExifISOSpeed = m?.ExifISOSpeed,
                ExifFocalLength = m?.ExifFocalLength,
            };
        });

        SendResponse(msg.RequestId, meta);
    }


    /// <summary>
    /// Returns the current photo list and active index.
    /// </summary>
    private async Task HandleGetPhotoListAsync(ToolMessage msg)
    {
        var result = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var pm = Core.Photos;
            if (pm is null) return new ToolPhotoList();

            var photos = pm.Items.Select(p => new ToolPhotoListItem
            {
                FilePath = p.FilePath,
                Width = (int?)p.Metadata?.OriginalWidth,
                Height = (int?)p.Metadata?.OriginalHeight,
                Format = p.Metadata?.FileExtension,
            }).ToArray();

            return new ToolPhotoList
            {
                Photos = photos,
                CurrentIndex = pm.CurrentIndex,
            };
        });

        SendResponse(msg.RequestId, result);
    }


    /// <summary>
    /// Returns the size of the current viewer source bitmap.
    /// </summary>
    private async Task HandleGetSourceSizeAsync(ToolMessage msg)
    {
        var size = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (AppAPIProvider.GetViewer() is not { } viewer) return (0, 0);
            return ((int)viewer.BitmapSize.Width, (int)viewer.BitmapSize.Height);
        });

        SendResponse(msg.RequestId, new SourceSizeResponse { Width = size.Item1, Height = size.Item2 });
    }


    /// <summary>
    /// Returns the current source-space selection rectangle.
    /// </summary>
    private async Task HandleGetSelectionAsync(ToolMessage msg)
    {
        var sel = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (AppAPIProvider.GetViewer() is not { } viewer
                || viewer.SourceSelection == default) return (SetSelectionRequest?)null;

            var s = viewer.SourceSelection;
            return new SetSelectionRequest
            {
                X = (float)s.X,
                Y = (float)s.Y,
                Width = (float)s.Width,
                Height = (float)s.Height,
            };
        });

        SendResponse(msg.RequestId, sel);
    }


    /// <summary>
    /// Updates the viewer selection rectangle from a tool request.
    /// </summary>
    private async Task HandleSetSelectionAsync(ToolMessage msg)
    {
        var req = DeserializePayload<SetSelectionRequest>(msg);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (AppAPIProvider.GetViewer() is not { } viewer) return;

            if (req?.X is null)
            {
                viewer.SourceSelection = default;
            }
            else
            {
                viewer.SourceSelection = new Avalonia.Rect(
                    req.X.Value, req.Y ?? 0, req.Width ?? 0, req.Height ?? 0);
            }
        });

        SendResponse(msg.RequestId, null);
    }


    /// <summary>
    /// Enables or disables the viewer selection overlay.
    /// </summary>
    private async Task HandleEnableSelectionAsync(ToolMessage msg)
    {
        var req = DeserializePayload<EnableSelectionRequest>(msg);
        if (req is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (AppAPIProvider.GetViewer() is { } viewer)
            {
                viewer.EnableSelection = req.Enable;
            }
        });

        SendResponse(msg.RequestId, null);
    }


    /// <summary>
    /// Replaces the current event subscription set with the tool-requested subscriptions.
    /// </summary>
    private void HandleSubscribeEvents(ToolMessage msg)
    {
        var subs = DeserializePayload<ToolEventSubscriptions>(msg);
        if (subs is not null)
        {
            Subscriptions = subs;
        }
        SendResponse(msg.RequestId, null);
    }


    /// <summary>
    /// Returns the current theme information the tool needs for initial UI sync.
    /// </summary>
    private void HandleGetThemeInfo(ToolMessage msg)
    {
        var info = new ThemeInfo
        {
            IsDarkMode = Core.Theme.Settings.IsDarkMode,
            AccentColor = Core.AccentColor.ToString(),
            BackgroundColor = Core.Config.BackgroundColor,
        };
        SendResponse(msg.RequestId, info);
    }

    #endregion


    #region Sending

    /// <summary>
    /// Serializes outbound messages onto the pipe in send order.
    /// </summary>
    private async Task WriteLoopAsync()
    {
        try
        {
            while (await _outboundMessages.Reader.WaitToReadAsync(_writeCts.Token))
            {
                // Drain the channel batch currently available before waiting again.
                while (_outboundMessages.Reader.TryRead(out var message))
                {
                    await _writer.WriteLineAsync(message);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
    }


    /// <summary>
    /// Queues one outbound message for the dedicated writer loop.
    /// </summary>
    private void QueueMessage(ToolMessage msg)
    {
        var json = JsonSerializer.Serialize(msg, ToolJsonContext.Default.ToolMessage);
        _ = _outboundMessages.Writer.TryWrite(json);
    }

    /// <summary>
    /// Sends an event (no requestId) to the tool.
    /// </summary>
    public void SendEvent(string type, object? payload = null)
    {
        var jsonPayload = payload is null
            ? null
            : (JsonElement?)JsonSerializer.SerializeToElement(payload, payload.GetType(), ToolJsonContext.Default);

        var msg = new ToolMessage { Type = type, Payload = jsonPayload };
        QueueMessage(msg);
    }


    /// <summary>
    /// Sends a response with a matching requestId.
    /// </summary>
    private void SendResponse(int? requestId, object? payload)
    {
        var jsonPayload = payload is null
            ? null
            : (JsonElement?)JsonSerializer.SerializeToElement(payload, payload.GetType(), ToolJsonContext.Default);

        var msg = new ToolMessage { Type = "RESPONSE", RequestId = requestId, Payload = jsonPayload };
        QueueMessage(msg);
    }

    #endregion


    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Serializer uses source-generated PluginJsonContext options")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "Serializer uses source-generated PluginJsonContext options")]
    /// <summary>
    /// Deserializes the message payload into the requested tool contract type.
    /// </summary>
    private static T? DeserializePayload<T>(ToolMessage msg) where T : class
    {
        if (msg.Payload is null) return null;
        return msg.Payload.Value.Deserialize<T>(ToolJsonContext.Default.Options);
    }


    /// <summary>
    /// Disposes the pipe handler, writer loop, and any active exported pixel buffers.
    /// </summary>
    public void Dispose()
    {
        // Clean up active pixel buffers
        foreach (var entry in _activeBuffers.Values)
        {
            try
            {
                entry.Mmf.Dispose();
                File.Delete(entry.Path);
            }
            catch { }
        }
        _activeBuffers.Clear();

        try { _writeCts.Cancel(); } catch { }
        _outboundMessages.Writer.TryComplete();
        try { _writerTask.GetAwaiter().GetResult(); } catch { }
        try { _writer.Dispose(); } catch { }
        try { _reader.Dispose(); } catch { }
        try { _writeCts.Dispose(); } catch { }
    }
}
