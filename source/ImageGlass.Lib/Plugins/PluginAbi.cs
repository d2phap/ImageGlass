/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using ImageGlass.SDK.Plugins;

namespace ImageGlass.Plugins;


/// <summary>
/// Layout rules for the versioned plugin ABI tables.
/// </summary>
/// <remarks>
/// The tables grow by appending, and a plugin allocates its own SDK's size, so the declared
/// <c>StructSize</c> is the only safe bound: reading a field past it runs off the plugin's
/// allocation and reads whatever follows as a function pointer.
/// </remarks>
internal static unsafe class PluginAbi
{
    /// <summary>
    /// Offset past <c>FreePixelBuffer</c>: the smallest table carrying the required decode members.
    /// </summary>
    // Derived from real field offsets: counting fields gets it wrong (a leading int pads to 8).
    internal static readonly int MinCodecApiSize = MeasureMinCodecApi();


    private static int MeasureMinCodecApi()
    {
        IGCodecApi probe = default;
        return (int)((byte*)&probe.FreePixelBuffer - (byte*)&probe) + sizeof(nint);
    }


    /// <summary>
    /// Whether <paramref name="field"/> of <paramref name="api"/> is both covered by the
    /// table's declared size and set to a non-null entry point.
    /// </summary>
    /// <param name="field">Address of the member, e.g. <c>&amp;api-&gt;EncodeFrame</c>.</param>
    internal static bool HasEntryPoint(IGCodecApi* api, void* field)
    {
        if (api == null || field == null) return false;

        var fieldEnd = (byte*)field - (byte*)api + sizeof(nint);
        if (fieldEnd > api->StructSize) return false;

        return *(nint*)field != 0;
    }
}
