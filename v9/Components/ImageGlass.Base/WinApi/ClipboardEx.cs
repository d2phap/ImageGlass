/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 - 2023 DUONG DIEU PHAP
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

using WicNet;

namespace ImageGlass.Base.WinApi;

/// <summary>
/// Support copy and paste transparent bitmap:
/// https://stackoverflow.com/a/46424800/2856887
/// </summary>
public static class ClipboardEx
{

    /// <summary>
    /// Copies the given image to the clipboard as PNG, DIB and standard Bitmap format.
    /// </summary>
    /// <param name="img">Image to put on the clipboard.</param>
    /// <param name="nonAlphaImg">
    /// Optional specifically nontransparent version of the image to put on the clipboard.
    /// </param>
    /// <param name="data">
    /// Clipboard data object to put the image into. Might already contain other stuff.
    /// Leave null to create a new one.
    /// </param>
    public static void SetClipboardImage(WicBitmapSource? img, WicBitmapSource? nonAlphaImg = null, DataObject? data = null)
    {
        var clonedImg = img?.Clone();
        if (clonedImg == null) return;

        // https://stackoverflow.com/a/17762059/2856887
        BHelper.RunAsThread(() => Clipboard.Clear(), ApartmentState.STA)
            .Join();

        data ??= new DataObject();
        nonAlphaImg ??= clonedImg;

        using var pngMemStream = new MemoryStream();
        using var dibMemStream = new MemoryStream();

        // As standard bitmap, without transparency support
        data.SetData(DataFormats.Bitmap, true, BHelper.ToGdiPlusBitmap(nonAlphaImg));

        // As PNG. Gimp will prefer this over the other two.
        clonedImg.Save(pngMemStream, WicCodec.GUID_ContainerFormatPng);
        data.SetData("PNG", false, pngMemStream);

        // The 'copy=true' argument means the MemoryStreams can be safely disposed
        // after the operation.
        BHelper.RunAsThread(() => Clipboard.SetDataObject(data, true), ApartmentState.STA)
            .Join();
    }


    /// <summary>
    /// Retrieves an image from the given clipboard data object,
    /// in the order PNG, DIB, Bitmap, Image object.
    /// </summary>
    /// <param name="retrievedData">The clipboard data.</param>
    /// <returns>The extracted image, or null if no supported image type was found.</returns>
    public static WicBitmapSource? GetClipboardImage()
    {
        var dataObj = Clipboard.GetDataObject();
        if (dataObj == null) return null;

        WicBitmapSource? clipboardImage = null;

        // Order: try PNG, move on to try 32-bit ARGB DIB, then try the normal
        // Bitmap and Image types.
        if (dataObj.GetDataPresent("PNG", false))
        {
            if (dataObj.GetData("PNG", false) is MemoryStream pngStream)
            {
                using var wicBmp = BHelper.ToWicBitmapSource(pngStream);
                clipboardImage = wicBmp?.Clone();
            }
        }

        if (clipboardImage == null && dataObj.GetDataPresent(DataFormats.Bitmap))
        {
            if (dataObj.GetData(DataFormats.Bitmap) is Image img)
            {
                clipboardImage = BHelper.ToWicBitmapSource(img);
            }
        }

        if (clipboardImage == null && dataObj.GetDataPresent(typeof(Image)))
        {
            if (dataObj.GetData(typeof(Image)) is Image img)
            {
                clipboardImage = BHelper.ToWicBitmapSource(img);
            }
        }

        return clipboardImage;
    }


    /// <summary>
    /// Indicates whether the clipboard contains an image.
    /// </summary>
    public static bool ContainsImage()
    {
        var dataObj = Clipboard.GetDataObject();
        if (dataObj == null) return false;

        // Order: try PNG, move on to try 32-bit ARGB DIB, then try the normal
        // Bitmap and Image types.
        var hasPng = dataObj.GetDataPresent("PNG", false);
        var hasBitmap = Clipboard.ContainsImage();
        var hasImage = dataObj.GetDataPresent(typeof(Image));

        return hasPng || hasBitmap || hasImage;
    }

}
