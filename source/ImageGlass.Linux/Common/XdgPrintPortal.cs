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
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace ImageGlass.Linux.Common;


/// <summary>
/// Prints through the desktop's own print dialog, the XDG print portal, which takes the file as a descriptor that <c>gdbus</c> cannot pass.
/// </summary>
internal static class XdgPrintPortal
{
    private const string PORTAL_DEST = "org.freedesktop.portal.Desktop";
    private const string PORTAL_PATH = "/org/freedesktop/portal/desktop";
    private const string PRINT_INTERFACE = "org.freedesktop.portal.Print";
    private const string REQUEST_INTERFACE = "org.freedesktop.portal.Request";
    private const uint RESPONSE_SUCCESS = 0;


    /// <summary>
    /// Hands a PDF to the portal, which shows its print dialog and prints it there; returns whether the user printed.
    /// </summary>
    public static async Task<bool> PrintAsync(string pdfPath, string title, string parentWindow, CancellationToken token)
    {
        var address = DBusAddress.Session ?? throw new InvalidOperationException("There is no D-Bus session bus to reach the print portal.");

        // the portal closes the requests of a caller that leaves the bus, so the connection stays until the dialog answers
        using var connection = new DBusConnection(address);
        await connection.ConnectAsync().ConfigureAwait(false);
        token.ThrowIfCancellationRequested();

        // the answer is watched before the call, on the path the token gives the request, so it cannot be missed
        var handleToken = $"imageglass{Guid.NewGuid():N}";
        var requestPath = $"{PORTAL_PATH}/request/{connection.UniqueName!.TrimStart(':').Replace('.', '_')}/{handleToken}";
        var response = new TaskCompletionSource<uint>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var watcher = await connection.WatchSignalAsync(null, requestPath, REQUEST_INTERFACE, "Response",
            (message, _) => message.GetBodyReader().ReadUInt32(),
            notification =>
            {
                if (notification.HasValue) response.TrySetResult(notification.Value);
                else if (notification.Exception is { } ex) response.TrySetException(ex);
            },
            ObserverFlags.None, false, null).ConfigureAwait(false);

        // the descriptor is the portal's own copy once sent, so the file can go when this returns
        using (var file = File.OpenHandle(pdfPath, FileMode.Open, FileAccess.Read))
        {
            await connection.CallMethodAsync(CreatePrintMessage(connection, parentWindow, title, file, handleToken)).ConfigureAwait(false);
        }

        try
        {
            // 0 printed, 1 cancelled, 2 ended another way
            return await response.Task.WaitAsync(token).ConfigureAwait(false) == RESPONSE_SUCCESS;
        }
        catch (OperationCanceledException)
        {
            // the Print window went away, so its dialog goes too; a request that already ended refuses, which changes nothing
            try
            {
                await connection.CallMethodAsync(CreateCloseMessage(connection, requestPath)).ConfigureAwait(false);
            }
            catch (DBusExceptionBase) { }

            throw;
        }
    }


    /// <summary>
    /// Builds Print(s parent_window, s title, h fd, a{sv} options); without a token from PreparePrint, the portal asks the user first.
    /// </summary>
    private static MessageBuffer CreatePrintMessage(DBusConnection connection, string parentWindow, string title, SafeHandle file, string handleToken)
    {
        using var writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(PORTAL_DEST, PORTAL_PATH, PRINT_INTERFACE, "Print", "ssha{sv}", MessageFlags.None);
        writer.WriteString(parentWindow);
        writer.WriteString(title);
        writer.WriteHandle(file);

        var options = writer.WriteDictionaryStart();
        writer.WriteDictionaryEntryStart();
        writer.WriteString("modal");
        writer.WriteVariantBool(true);
        writer.WriteDictionaryEntryStart();
        writer.WriteString("handle_token");
        writer.WriteVariantString(handleToken);
        writer.WriteDictionaryEnd(options);

        return writer.CreateMessage();
    }


    private static MessageBuffer CreateCloseMessage(DBusConnection connection, string requestPath)
    {
        using var writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(PORTAL_DEST, requestPath, REQUEST_INTERFACE, "Close", null, MessageFlags.None);

        return writer.CreateMessage();
    }
}
