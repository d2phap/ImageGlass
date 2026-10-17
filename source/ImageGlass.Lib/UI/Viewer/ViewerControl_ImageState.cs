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
using Avalonia;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using SkiaSharp;
using System;
using System.Threading.Tasks;

namespace ImageGlass.UI.Viewer;

public partial class ViewerControl
{
    /// <summary>
    /// Gets the orientation the photo is shown in, composed by <see cref="RotateImage"/> and <see cref="FlipImage"/>.
    /// </summary>
    public ImageOrientation PhotoOrientation { get; private set; } = ImageOrientation.Identity;


    /// <summary>
    /// Checks whether the photo is shown with a viewer edit: a channel filter, inverted colors or an orientation.
    /// </summary>
    private bool HasImageEdits()
    {
        // animated and vector photos take no edits
        if (_animator is not null || IsVectorSource()) return false;

        return !_loadingOptions.Channels.HasFlag(ColorChannels.RGBA)
            || IsColorInverted
            || !PhotoOrientation.IsIdentity;
    }


    /// <summary>
    /// Builds the edited image of a new frame off the UI thread, so its first draw already shows the edits; the caller keeps the frame alive.
    /// </summary>
    private async Task<SKImage?> BuildEditedFrameAsync(SKImage? frame)
    {
        if (frame.IsDisposed() || !HasImageEdits()) return null;

        // snapshot the UI-thread state the pass reads
        var channels = _loadingOptions.Channels;
        var isColorInverted = IsColorInverted;
        var orientation = PhotoOrientation;

        return await Task.Run(() => SkiaCodec.ApplyImageEdits(frame, channels, isColorInverted, orientation));
    }


    /// <summary>
    /// Rebuilds the edited image from the source in the one edit order; the caller holds <see cref="_lock"/>.
    /// </summary>
    private bool RebuildEditedImage()
    {
        var srcImage = _imgSource?.Image;
        if (srcImage.IsDisposed()) return false;

        // nothing to apply shares the source itself
        var edited = SkiaCodec.ApplyImageEdits(srcImage, _loadingOptions.Channels, IsColorInverted, PhotoOrientation);
        SKImageRef.Set(ref _imgRender, edited ?? srcImage, _imgSource);

        _mipmapCache?.Dispose();
        _mipmapCache = null;

        return true;
    }


    /// <summary>
    /// Captures what the viewer shows now, its edits plus the shown image, without copying pixels; call on the UI thread and dispose the result.
    /// </summary>
    public ViewerImageState? CaptureImageState()
    {
        lock (_lock)
        {
            var photo = Photo;
            var lease = (_imgRender ?? _imgSource)?.Acquire();
            if (photo is null || lease is null) return null;

            // an animator owns its frames and evicts them by Dispose, so its frame is copied
            SKImage? ownedFrame = null;
            if (_animator is not null)
            {
                ownedFrame = SkiaCodec.CopyImage(lease.Image);
                lease.Dispose();
                lease = null;

                if (ownedFrame is null) return null;
            }

            // animated and vector photos show no edits
            var showsEdits = _animator is null && !IsVectorSource();
            var toneMapping = Core.IsProEnabled ? Core.HdrToneMappingConfig : new HdrToneMappingOptions();

            return new ViewerImageState(lease, ownedFrame)
            {
                Photo = photo,
                IsVector = IsVectorSource(),
                IsAnimated = _animator is not null,
                FrameIndex = Math.Max(0, photo.FrameIndex),
                Orientation = showsEdits ? PhotoOrientation : ImageOrientation.Identity,
                IsColorInverted = showsEdits && IsColorInverted,
                Channels = showsEdits ? _loadingOptions.Channels : ColorChannels.RGBA,
                SelectionRect = SourceSelection,
                VisibleRect = SrcRect,
                IsPreview = _isPreviewing,
                HdrToneMapping = EnableHdrRendering && photo.Metadata?.IsHdr == true ? toneMapping with { } : null,
                HasExactColorTag = !photo.HasBakedDisplayProfile,
            };
        }
    }


    /// <summary>
    /// Gets the size of the shown image: the edited one, else the source.
    /// </summary>
    private Size GetShownImageSize(Size fallback)
    {
        var img = (_imgRender ?? _imgSource)?.Image;
        return img.IsDisposed() ? fallback : new Size(img.Width, img.Height);
    }
}
