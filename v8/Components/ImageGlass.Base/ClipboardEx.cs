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
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace ImageGlass.Base {

    /// <summary>
    /// Support copy and paste transparent bitmap:
    /// https://stackoverflow.com/a/46424800/2856887
    /// </summary>
    public class ClipboardEx {

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
        public static void SetClipboardImage(Bitmap image, Bitmap? imageNoTr = null, DataObject? data = null) {
            Clipboard.Clear();

            if (data == null)
                data = new DataObject();

            if (imageNoTr == null)
                imageNoTr = image;

            using var pngMemStream = new MemoryStream();
            using var dibMemStream = new MemoryStream();

            // As standard bitmap, without transparency support
            data.SetData(DataFormats.Bitmap, true, imageNoTr);

            // As PNG. Gimp will prefer this over the other two.
            image.Save(pngMemStream, ImageFormat.Png);
            data.SetData("PNG", false, pngMemStream);

            // As DIB. This is (wrongly) accepted as ARGB by many applications.
            var dibData = ConvertToDib(image);
            dibMemStream.Write(dibData, 0, dibData.Length);
            data.SetData(DataFormats.Dib, false, dibMemStream);

            // The 'copy=true' argument means the MemoryStreams can be safely disposed
            // after the operation.
            Clipboard.SetDataObject(data, true);
        }


        /// <summary>
        /// Retrieves an image from the given clipboard data object,
        /// in the order PNG, DIB, Bitmap, Image object.
        /// </summary>
        /// <param name="retrievedData">The clipboard data.</param>
        /// <returns>The extracted image, or null if no supported image type was found.</returns>
        public static Bitmap GetClipboardImage(DataObject retrievedData) {
            Bitmap clipboardImage = null;

            // Order: try PNG, move on to try 32-bit ARGB DIB, then try the normal
            // Bitmap and Image types.
            if (retrievedData.GetDataPresent("PNG", false)) {
                if (retrievedData.GetData("PNG", false) is MemoryStream png_stream)
                    using (var bm = new Bitmap(png_stream))
                        clipboardImage = CloneImage(bm);
            }

            if (clipboardImage == null && retrievedData.GetDataPresent(DataFormats.Dib, false)) {
                if (retrievedData.GetData(DataFormats.Dib, false) is MemoryStream dib) {
                    clipboardImage = ImageFromClipboardDib(dib.ToArray());
                }
            }

            if (clipboardImage == null && retrievedData.GetDataPresent(DataFormats.Bitmap)) {
                clipboardImage = new Bitmap(retrievedData.GetData(DataFormats.Bitmap) as Image);
            }

            if (clipboardImage == null && retrievedData.GetDataPresent(typeof(Image))) {
                clipboardImage = new Bitmap(retrievedData.GetData(typeof(Image)) as Image);
            }

            return clipboardImage;
        }


        private static Bitmap ImageFromClipboardDib(byte[] dibBytes) {
            if (dibBytes == null || dibBytes.Length < 4)
                return null;

            try {
                var headerSize = (int)ReadIntFromByteArray(dibBytes, 0, 4, true);

                // Only supporting 40-byte DIB from clipboard
                if (headerSize != 40)  return null;

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
                switch (bitCount) {
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

                if (compression == 3) {
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
                        && blueMask == 0x0000FF) {
                        // Stride is always a multiple of 4; no need to take it into account for 32bpp.
                        for (var pix = 3; pix < image.Length; pix += 4) {
                            // 0 can mean transparent, but can also mean the alpha isn't filled in,
                            // so only check for non-zero alpha, which would indicate there is
                            // actual data in the alpha bytes.
                            if (image[pix] == 0)
                                continue;

                            fmt = PixelFormat.Format32bppPArgb;
                            break;
                        }
                    }
                    else {
                        // Could be supported with a system that parses the colour masks,
                        // but I don't think the clipboard ever uses these anyway.
                        return null;
                    }
                }

                var bitmap = BuildImage(image, width, height, stride, fmt, null, null);

                // This is bmp; reverse image lines.
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);

                return bitmap;
            }
            catch {
                return null;
            }
        }


        /// <summary>
        /// Clones an image object to free it from any backing resources.
        /// Code taken from http://stackoverflow.com/a/3661892/ with some extra fixes.
        /// </summary>
        /// <param name="sourceImage">The image to clone</param>
        /// <returns>The cloned image</returns>
        private static Bitmap CloneImage(Bitmap sourceImage) {
            var rect = NewMethod(sourceImage);
            var targetImage = new Bitmap(rect.Width, rect.Height, sourceImage.PixelFormat);
            targetImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

            var sourceData = sourceImage.LockBits(rect, ImageLockMode.ReadOnly, sourceImage.PixelFormat);
            var targetData = targetImage.LockBits(rect, ImageLockMode.WriteOnly, targetImage.PixelFormat);
            var actualDataWidth = ((Image.GetPixelFormatSize(sourceImage.PixelFormat) * rect.Width) + 7) / 8;
            var h = sourceImage.Height;
            var origStride = sourceData.Stride;
            var isFlipped = origStride < 0;
            origStride = Math.Abs(origStride); // Fix for negative stride in BMP format.
            var targetStride = targetData.Stride;
            var imageData = new byte[actualDataWidth];
            var sourcePos = sourceData.Scan0;
            var destPos = targetData.Scan0;

            // Copy line by line, skipping by stride but copying actual data width
            for (var y = 0; y < h; y++) {
                Marshal.Copy(sourcePos, imageData, 0, actualDataWidth);
                Marshal.Copy(imageData, 0, destPos, actualDataWidth);

                sourcePos = new IntPtr(sourcePos.ToInt64() + origStride);
                destPos = new IntPtr(destPos.ToInt64() + targetStride);
            }

            targetImage.UnlockBits(targetData);
            sourceImage.UnlockBits(sourceData);

            // Fix for negative stride on BMP format.
            if (isFlipped)
                targetImage.RotateFlip(RotateFlipType.Rotate180FlipX);

            // For indexed images, restore the palette. This is not linking to a referenced
            // object in the original image; the getter of Palette creates a new object when called.
            if ((sourceImage.PixelFormat & PixelFormat.Indexed) != 0)
                targetImage.Palette = sourceImage.Palette;

            // Restore DPI settings
            targetImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

            return targetImage;
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
        private static Bitmap BuildImage(byte[] sourceData, int width, int height, int stride, PixelFormat pixelFormat, Color[] palette, Color? defaultColor) {
            var newImage = new Bitmap(width, height, pixelFormat);
            var targetData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, newImage.PixelFormat);
            var newDataWidth = ((Image.GetPixelFormatSize(pixelFormat) * width) + 7) / 8;

            // Compensate for possible negative stride on BMP format.
            var isFlipped = stride < 0;
            stride = Math.Abs(stride);

            // Cache these to avoid unnecessary getter calls.
            var targetStride = targetData.Stride;
            var scan0 = targetData.Scan0.ToInt64();

            for (var y = 0; y < height; y++) {
                Marshal.Copy(sourceData, y * stride, new IntPtr(scan0 + y * targetStride), newDataWidth);
            }

            newImage.UnlockBits(targetData);

            // Fix negative stride on BMP format.
            if (isFlipped) {
                newImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            }

            // For indexed images, set the palette.
            if ((pixelFormat & PixelFormat.Indexed) != 0 && palette != null) {
                var pal = newImage.Palette;

                for (var i = 0; i < pal.Entries.Length; i++) {
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


        private static Rectangle NewMethod(Bitmap sourceImage) {
            return new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
        }


        /// <summary>
        /// Gets the raw bytes from an image.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="stride">Stride of the retrieved image data.</param>
        /// <returns>The raw bytes of the image</returns>
        private static byte[] GetImageData(Bitmap sourceImage, out int stride) {
            var sourceData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadOnly, sourceImage.PixelFormat);

            stride = sourceData.Stride;
            var data = new byte[stride * sourceImage.Height];

            Marshal.Copy(sourceData.Scan0, data, 0, data.Length);
            sourceImage.UnlockBits(sourceData);

            return data;
        }


        /// <summary>
        /// Converts the image to Device Independent Bitmap format of type BITFIELDS.
        /// This is (wrongly) accepted by many applications as containing transparency,
        /// so I'm abusing it for that.
        /// </summary>
        /// <param name="image">Image to convert to DIB</param>
        /// <returns>The image converted to DIB, in bytes.</returns>
        private static byte[] ConvertToDib(Image image) {
            byte[] bm32bData;
            var width = image.Width;
            var height = image.Height;

            // Ensure image is 32bppARGB by painting it on a new 32bppARGB image.
            using var bm32b = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            using var gr = Graphics.FromImage(bm32b);
            gr.DrawImage(image, new Rectangle(0, 0, bm32b.Width, bm32b.Height));

            // Bitmap format has its lines reversed.
            bm32b.RotateFlip(RotateFlipType.Rotate180FlipX);
            bm32bData = GetImageData(bm32b, out var stride);

            // BITMAPINFOHEADER struct for DIB.
            var hdrSize = 0x28;
            var fullImage = new byte[hdrSize + 12 + bm32bData.Length];
            //Int32 biSize;
            WriteIntToByteArray(fullImage, 0x00, 4, true, (uint)hdrSize);

            //Int32 biWidth;
            WriteIntToByteArray(fullImage, 0x04, 4, true, (uint)width);

            //Int32 biHeight;
            WriteIntToByteArray(fullImage, 0x08, 4, true, (uint)height);

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


        private static void WriteIntToByteArray(byte[] data, int startIndex, int bytes, bool littleEndian, uint value) {
            var lastByte = bytes - 1;

            if (data.Length < startIndex + bytes) {
                throw new ArgumentOutOfRangeException("startIndex", "Data array is too small to write a " + bytes + "-byte value at offset " + startIndex + ".");
            }

            for (var index = 0; index < bytes; index++) {
                var offs = startIndex + (littleEndian ? index : lastByte - index);
                data[offs] = (byte)(value >> (8 * index) & 0xFF);
            }
        }


        private static uint ReadIntFromByteArray(byte[] data, int startIndex, int bytes, bool littleEndian) {
            var lastByte = bytes - 1;

            if (data.Length < startIndex + bytes) {
                throw new ArgumentOutOfRangeException("startIndex", "Data array is too small to read a " + bytes + "-byte value at offset " + startIndex + ".");
            }

            uint value = 0;
            for (var index = 0; index < bytes; index++) {
                var offs = startIndex + (littleEndian ? index : lastByte - index);
                value += (uint)(data[offs] << (8 * index));
            }

            return value;
        }

    }
}
