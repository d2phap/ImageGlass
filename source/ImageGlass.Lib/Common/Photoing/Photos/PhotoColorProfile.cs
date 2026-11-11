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

namespace ImageGlass.Common.Photoing;


public partial class PhotoColorProfile(byte[]? data)
{
    public byte[]? ProfileData { get; private set; } = data;



    /// <summary>
    /// Gets Description tag of ICC profile.
    /// </summary>
    public string GetIccDescription()
    {
        if (ProfileData is null) return string.Empty;

        try
        {
            // header is 128 bytes, then a big-endian tag count and 12-byte tag records
            var tagCount = ReadBE32__(ProfileData, 128);

            for (var i = 132; i + 12 <= ProfileData.Length && i < 132 + tagCount * 12; i += 12)
            {
                var tag = System.Text.Encoding.ASCII.GetString(ProfileData, i, 4);
                if (tag != "desc") continue;

                var offset = ReadBE32__(ProfileData, i + 4);
                var size = ReadBE32__(ProfileData, i + 8);
                if (offset < 0 || size < 8 || offset + size > ProfileData.Length) return string.Empty;

                // the element's own type signature decides the layout, not the tag signature:
                // ICC v2 stores "desc" (textDescription), v4 stores "mluc" (multiLocalizedUnicode)
                var elementType = System.Text.Encoding.ASCII.GetString(ProfileData, offset, 4);

                return elementType switch
                {
                    "desc" => ReadTextDescription__(ProfileData, offset, size),
                    "mluc" => ReadMultiLocalizedUnicode__(ProfileData, offset, size),
                    _ => string.Empty,
                };
            }
        }
        catch { }

        return string.Empty;
    }


    /// <summary>
    /// Reads an ICC v2 <c>textDescription</c>: count then a NUL-terminated ASCII string.
    /// </summary>
    private static string ReadTextDescription__(byte[] data, int offset, int size)
    {
        var length = ReadBE32__(data, offset + 8);
        if (length <= 1 || 12 + length > size) return string.Empty;

        // the stored count includes the terminating NUL
        return System.Text.Encoding.ASCII.GetString(data, offset + 12, length - 1);
    }


    /// <summary>
    /// Reads an ICC v4 <c>multiLocalizedUnicode</c>, taking its first (normally English) record.
    /// </summary>
    private static string ReadMultiLocalizedUnicode__(byte[] data, int offset, int size)
    {
        var recordCount = ReadBE32__(data, offset + 8);
        var recordSize = ReadBE32__(data, offset + 12);
        if (recordCount <= 0 || recordSize < 12 || 16 + recordSize > size) return string.Empty;

        // record: 2-byte language, 2-byte country, 4-byte length, 4-byte offset from the element
        var strLength = ReadBE32__(data, offset + 16 + 4);
        var strOffset = ReadBE32__(data, offset + 16 + 8);
        if (strLength <= 0 || strOffset < 0 || strOffset + strLength > size) return string.Empty;

        return System.Text.Encoding.BigEndianUnicode.GetString(data, offset + strOffset, strLength);
    }


    private static int ReadBE32__(byte[] data, int index)
    {
        // Manually convert 4 bytes from big-endian to host order
        return (data[index] << 24)
             | (data[index + 1] << 16)
             | (data[index + 2] << 8)
             | (data[index + 3]);
    }
}
