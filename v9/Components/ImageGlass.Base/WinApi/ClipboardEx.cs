/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
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

using ImageGlass.Base.Photoing.Codecs;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using WicNet;

namespace ImageGlass.Base.WinApi;

/// <summary>
/// Support copy and paste transparent bitmap:
/// https://stackoverflow.com/a/46424800/2856887
/// </summary>
public class ClipboardEx
{

    /// <summary>
    /// Copies the given image to the clipboard as PNG, DIB and standard Bitmap format.
    /// </summary>
    /// <param name="image">Image to put on the clipboard.</param>
    /// <param name="imageNoTr">
    /// Optional specifically nontransparent version of the image to put on the clipboard.
    /// </param>
    /// <param name="data">
    /// Clipboard data object to put the image into.
    /// Might already contain other stuff.
    /// Leave null to create a new one.
    /// </param>
    public static void SetClipboardImage(WicBitmapSource? image, WicBitmapSource? imageNoTr = null, DataObject? data = null)
    {
        var clonedImg = image?.Clone();
        if (clonedImg == null) return;

        // https://stackoverflow.com/a/17762059/2856887
        Helpers.RunAsThread(() => Clipboard.Clear(), ApartmentState.STA)
            .Join();

        data ??= new DataObject();
        imageNoTr ??= clonedImg;

        using var pngMemStream = new MemoryStream();
        using var dibMemStream = new MemoryStream();

        // As standard bitmap, without transparency support
        data.SetData(DataFormats.Bitmap, true, PhotoCodec.BitmapSourceToGdiPlusBitmap(imageNoTr));

        // As PNG. Gimp will prefer this over the other two.
        clonedImg.Save(pngMemStream, WicEncoder.GUID_ContainerFormatPng);
        data.SetData("PNG", false, pngMemStream);

        // As DIB. This is (wrongly) accepted as ARGB by many applications.
        var dibData = ConvertToDib(clonedImg);
        dibMemStream.Write(dibData, 0, dibData.Length);
        data.SetData(DataFormats.Dib, false, dibMemStream);

        // The 'copy=true' argument means the MemoryStreams can be safely disposed
        // after the operation.
        Helpers.RunAsThread(() => Clipboard.SetDataObject(data, true), ApartmentState.STA)
            .Join();
    }


    /// <summary>
    /// Retrieves an image from the given clipboard data object,
    /// in the order PNG, DIB, Bitmap, Image object.
    /// </summary>
    /// <param name="retrievedData">The clipboard data.</param>
    /// <returns>The extracted image, or null if no supported image type was found.</returns>
    public static WicBitmapSource? GetClipboardImage(IDataObject retrievedData)
    {
        WicBitmapSource? clipboardImage = null;

        // Order: try PNG, move on to try 32-bit ARGB DIB, then try the normal
        // Bitmap and Image types.
        if (retrievedData.GetDataPresent("PNG", false))
        {
            if (retrievedData.GetData("PNG", false) is MemoryStream png_stream)
            {
                using var bm = WicBitmapSource.Load(png_stream);
                bm.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);
                clipboardImage = bm.Clone();
            }
        }

        if (clipboardImage == null && retrievedData.GetDataPresent(DataFormats.Dib, false))
        {
            if (retrievedData.GetData(DataFormats.Dib, false) is MemoryStream dib)
            {
                clipboardImage = ImageFromClipboardDib(dib.ToArray());
            }
        }

        if (clipboardImage == null && retrievedData.GetDataPresent(DataFormats.Bitmap))
        {
            if (retrievedData.GetData(DataFormats.Bitmap) is Image img)
            {
                var bmp = new Bitmap(img);
                clipboardImage = WicBitmapSource.FromHBitmap(bmp.GetHbitmap());
            }
        }

        if (clipboardImage == null && retrievedData.GetDataPresent(typeof(Image)))
        {
            if (retrievedData.GetData(typeof(Image)) is Image img)
            {
                var bmp = new Bitmap(img);
                clipboardImage = WicBitmapSource.FromHBitmap(bmp.GetHbitmap());
            }
        }

        return clipboardImage;
    }


    private static WicBitmapSource? ImageFromClipboardDib(byte[] dibBytes)
    {
        if (dibBytes == null || dibBytes.Length < 4)
            return null;

        try
        {
            var headerSize = (int)ReadIntFromByteArray(dibBytes, 0, 4, true);

            // Only supporting 40-byte DIB from clipboard
            if (headerSize != 40) return null;

            var header = new byte[40];
            Array.Copy(dibBytes, header, 40);

            var imageIndex = headerSize;
            var width = (int)ReadIntFromByteArray(header, 0x04, 4, true);
            var height = (int)ReadIntFromByteArray(header, 0x08, 4, true);
            var planes = (short)ReadIntFromByteArray(header, 0x0C, 2, true);
            var bitCount = (short)ReadIntFromByteArray(header, 0x0E, 2, true);

            // Compression: 0 = RGB; 3 = BITFIELDS.
            var compression = (int)ReadIntFromByteArray(header, 0x10, 4, true);

            // Not dealing with non-standard formats.
            if (planes != 1 || (compression != 0 && compression != 3))
                return null;

            PixelFormat fmt;
            switch (bitCount)
            {
                case 32:
                    fmt = PixelFormat.Format32bppRgb;
                    break;
                case 24:
                    fmt = PixelFormat.Format24bppRgb;
                    break;
                case 16:
                    fmt = PixelFormat.Format16bppRgb555;
                    break;
                default:
                    return null;
            }

            if (compression == 3)
                imageIndex += 12;

            if (dibBytes.Length < imageIndex)
                return null;

            var image = new byte[dibBytes.Length - imageIndex];
            Array.Copy(dibBytes, imageIndex, image, 0, image.Length);

            // Classic stride: fit within blocks of 4 bytes.
            var stride = (((((bitCount * width) + 7) / 8) + 3) / 4) * 4;

            if (compression == 3)
            {
                var redMask = ReadIntFromByteArray(dibBytes, headerSize + 0, 4, true);
                var greenMask = ReadIntFromByteArray(dibBytes, headerSize + 4, 4, true);
                var blueMask = ReadIntFromByteArray(dibBytes, headerSize + 8, 4, true);

                // Fix for the undocumented use of 32bppARGB disguised as BITFIELDS.
                // Despite lacking an alpha bit field, the alpha bytes are still filled in,
                // without any header indication of alpha usage.
                // Pure 32-bit RGB: check if a switch to ARGB can be made by checking
                // for non-zero alpha. Admitted, this may give a mess if the alpha bits simply aren't cleared, but why the hell wouldn't it use 24bpp then?
                if (bitCount == 32
                    && redMask == 0xFF0000
                    && greenMask == 0x00FF00
                    && blueMask == 0x0000FF)
                {
                    // Stride is always a multiple of 4; no need to take it into account for 32bpp.
                    for (var pix = 3; pix < image.Length; pix += 4)
                    {
                        // 0 can mean transparent, but can also mean the alpha isn't filled in,
                        // so only check for non-zero alpha, which would indicate there is
                        // actual data in the alpha bytes.
                        if (image[pix] == 0)
                            continue;

                        fmt = PixelFormat.Format32bppPArgb;
                        break;
                    }
                }
                else
                {
                    // Could be supported with a system that parses the colour masks,
                    // but I don't think the clipboard ever uses these anyway.
                    return null;
                }
            }

            var bitmap = BuildImage(image, width, height, stride, fmt, null, null);

            // This is bmp; reverse image lines.
            bitmap?.RotateFlip(RotateFlipType.Rotate180FlipX);

            if (bitmap != null)
            {
                return WicBitmapSource.FromHBitmap(bitmap.GetHbitmap());
            }
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Creates a bitmap based on data, width, height, stride and pixel format.
    /// </summary>
    /// <param name="sourceData">Byte array of raw source data</param>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="stride">Scanline length inside the data</param>
    /// <param name="pixelFormat">Pixel format</param>
    /// <param name="palette">Color palette</param>
    /// <param name="defaultColor">
    /// Default color to fill in on the palette if the given colors don't fully fill it.
    /// </param>
    /// <returns>The new image</returns>
    private static Bitmap? BuildImage(byte[] sourceData, int width, int height, int stride, PixelFormat pixelFormat, Color[]? palette, Color? defaultColor)
    {
        var newImage = new Bitmap(width, height, pixelFormat);
        var targetData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, newImage.PixelFormat);
        var newDataWidth = ((Image.GetPixelFormatSize(pixelFormat) * width) + 7) / 8;

        // Compensate for possible negative stride on BMP format.
        var isFlipped = stride < 0;
        stride = Math.Abs(stride);

        // Cache these to avoid unnecessary getter calls.
        var targetStride = targetData.Stride;
        var scan0 = targetData.Scan0.ToInt64();

        for (var y = 0; y < height; y++)
        {
            Marshal.Copy(sourceData, y * stride, new IntPtr(scan0 + y * targetStride), newDataWidth);
        }

        newImage.UnlockBits(targetData);

        // Fix negative stride on BMP format.
        if (isFlipped)
        {
            newImage.RotateFlip(RotateFlipType.Rotate180FlipX);
        }

        // For indexed images, set the palette.
        if ((pixelFormat & PixelFormat.Indexed) != 0 && palette != null)
        {
            var pal = newImage.Palette;

            for (var i = 0; i < pal.Entries.Length; i++)
            {
                if (i < palette.Length)
                    pal.Entries[i] = palette[i];
                else if (defaultColor.HasValue)
                    pal.Entries[i] = defaultColor.Value;
                else
                    break;
            }

            newImage.Palette = pal;
        }

        return newImage;
    }



    /// <summary>
    /// Converts the image to Device Independent Bitmap format of type BITFIELDS.
    /// This is (wrongly) accepted by many applications as containing transparency,
    /// so I'm abusing it for that.
    /// </summary>
    /// <param name="image">Image to convert to DIB</param>
    /// <returns>The image converted to DIB, in bytes.</returns>
    private static byte[] ConvertToDib(WicBitmapSource image)
    {
        // Ensure image is 32bppARGB by painting it on a new 32bppARGB image.
        image.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);

        // Bitmap format has its lines reversed.
        //image.Rotate(DirectN.WICBitmapTransformOptions.WICBitmapTransformRotate180);
        var bm32bData = image.CopyPixels();

        // BITMAPINFOHEADER struct for DIB.
        var hdrSize = 0x28;
        var fullImage = new byte[hdrSize + 12 + bm32bData.Length];
        //Int32 biSize;
        WriteIntToByteArray(fullImage, 0x00, 4, true, (uint)hdrSize);

        //Int32 biWidth;
        WriteIntToByteArray(fullImage, 0x04, 4, true, (uint)image.Width);

        //Int32 biHeight;
        WriteIntToByteArray(fullImage, 0x08, 4, true, (uint)image.Height);

        //Int16 biPlanes;
        WriteIntToByteArray(fullImage, 0x0C, 2, true, 1);

        //Int16 biBitCount;
        WriteIntToByteArray(fullImage, 0x0E, 2, true, 32);

        //BITMAPCOMPRESSION biCompression = BITMAPCOMPRESSION.BITFIELDS;
        WriteIntToByteArray(fullImage, 0x10, 4, true, 3);

        //Int32 biSizeImage;
        WriteIntToByteArray(fullImage, 0x14, 4, true, (uint)bm32bData.Length);

        // These are all 0. Since .net clears new arrays, don't bother writing them.
        //Int32 biXPelsPerMeter = 0;
        //Int32 biYPelsPerMeter = 0;
        //Int32 biClrUsed = 0;
        //Int32 biClrImportant = 0;

        // The aforementioned "BITFIELDS": colour masks applied to the Int32 pixel value
        // to get the R, G and B values.
        WriteIntToByteArray(fullImage, hdrSize + 0, 4, true, 0x00FF0000);
        WriteIntToByteArray(fullImage, hdrSize + 4, 4, true, 0x0000FF00);
        WriteIntToByteArray(fullImage, hdrSize + 8, 4, true, 0x000000FF);

        Array.Copy(bm32bData, 0, fullImage, hdrSize + 12, bm32bData.Length);

        return fullImage;
    }


    private static void WriteIntToByteArray(byte[] data, int startIndex, int bytes, bool littleEndian, uint value)
    {
        var lastByte = bytes - 1;

        if (data.Length < startIndex + bytes)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), "Data array is too small to write a " + bytes + "-byte value at offset " + startIndex + ".");
        }

        for (var index = 0; index < bytes; index++)
        {
            var offs = startIndex + (littleEndian ? index : lastByte - index);
            data[offs] = (byte)(value >> (8 * index) & 0xFF);
        }
    }


    private static uint ReadIntFromByteArray(byte[] data, int startIndex, int bytes, bool littleEndian)
    {
        var lastByte = bytes - 1;

        if (data.Length < startIndex + bytes)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), "Data array is too small to read a " + bytes + "-byte value at offset " + startIndex + ".");
        }

        uint value = 0;
        for (var index = 0; index < bytes; index++)
        {
            var offs = startIndex + (littleEndian ? index : lastByte - index);
            value += (uint)(data[offs] << (8 * index));
        }

        return value;
    }

}
