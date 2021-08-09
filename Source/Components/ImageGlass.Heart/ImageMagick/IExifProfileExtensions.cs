// Copyright Dirk Lemstra https://github.com/dlemstra/Magick.NET.
// Licensed under the Apache License, Version 2.0.


using System;
using QuantumType = System.UInt16;

namespace ImageMagick {
    /// <summary>
    /// Extension methods for the <see cref="IExifProfile"/> interface.
    /// </summary>
    public static class IExifProfileExtensions {
        /// <summary>
        /// Returns the thumbnail in the exif profile when available.
        /// </summary>
        /// <param name="self">The exif profile.</param>
        /// <returns>The thumbnail in the exif profile when available.</returns>
        public static IMagickImage<QuantumType>? CreateThumbnail(this IExifProfile self) {
            if (self is null) {
                throw new Exception($"{nameof(self)} is null.");
            }

            var thumbnailLength = self.ThumbnailLength;
            var thumbnailOffset = self.ThumbnailOffset;

            if (thumbnailLength == 0 || thumbnailOffset == 0)
                return null;

            var data = self.GetData();

            if (data == null || data.Length < (thumbnailOffset + thumbnailLength))
                return null;

            var result = new byte[thumbnailLength];
            Array.Copy(data, thumbnailOffset, result, 0, thumbnailLength);
            return new MagickImage(result);
        }
    }
}
