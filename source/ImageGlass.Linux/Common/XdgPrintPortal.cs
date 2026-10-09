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


    /// <summary>
    /// Hands a PDF to the portal, which shows its print dialog and prints it there; returns once the portal took the file.
    /// </summary>
    public static async Task PrintAsync(string pdfPath, string title, CancellationToken token)
    {
        var address = DBusAddress.Session ?? throw new InvalidOperationException("There is no D-Bus session bus to reach the print portal.");

        using var connection = new DBusConnection(address);
        await connection.ConnectAsync().ConfigureAwait(false);
        token.ThrowIfCancellationRequested();

        // the descriptor is the portal's own copy once sent, so the file can go when this returns
        using var file = File.OpenHandle(pdfPath, FileMode.Open, FileAccess.Read);
        await connection.CallMethodAsync(CreatePrintMessage(connection, title, file)).ConfigureAwait(false);
    }


    /// <summary>
    /// Builds Print(s parent_window, s title, h fd, a{sv} options); without a token in the options, the portal asks the user first.
    /// </summary>
    private static MessageBuffer CreatePrintMessage(DBusConnection connection, string title, SafeHandle file)
    {
        using var writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(PORTAL_DEST, PORTAL_PATH, PRINT_INTERFACE, "Print", "ssha{sv}", MessageFlags.None);
        writer.WriteString(string.Empty);
        writer.WriteString(title);
        writer.WriteHandle(file);

        var options = writer.WriteDictionaryStart();
        writer.WriteDictionaryEntryStart();
        writer.WriteString("modal");
        writer.WriteVariantBool(true);
        writer.WriteDictionaryEnd(options);

        return writer.CreateMessage();
    }
}
