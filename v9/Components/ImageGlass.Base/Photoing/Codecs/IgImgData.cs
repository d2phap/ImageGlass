/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using ImageMagick;
using System.Drawing.Imaging;
using WicNet;
using static ImageGlass.WebP.WebPWrapper;

namespace ImageGlass.Base.Photoing.Codecs;


/// <summary>
/// Contains image data and metadata to pass to frontend.
/// </summary>
public class IgImgData : IDisposable
{

    #region IDisposable Disposing

    private bool _isDisposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            // Free any other managed objects here.
            Image?.Dispose();
            Image = null;

            if (Source is Bitmap bmp)
            {
                bmp.Dispose();
            }
            else if (Source is IEnumerable<FrameData> frames)
            {
                foreach (var frame in frames)
                {
                    frame.Bitmap.Dispose();
                    frame.Bitmap = null;
                }
            }
            Source = null;

            Width = 0;
            Height = 0;
            FrameCount = 0;
            HasAlpha = false;
            CanAnimate = false;
        }

        // Free any unmanaged objects here.
        _isDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~IgImgData()
    {
        Dispose(false);
    }

    #endregion


    public WicBitmapSource? Image { get; set; } = null;

    /// <summary>
    /// Represent other type of image source.
    /// </summary>
    public object? Source { get; set; } = null;

    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;

    /// <summary>
    /// Checks if both <see cref="Image"/> and <see cref="Bitmap"/> are null;
    /// </summary>
    public bool IsImageNull => Image == null && Source == null;
    public int FrameCount { get; set; } = 0;
    public bool HasAlpha { get; set; } = false;
    public bool CanAnimate { get; set; } = false;


    public IgImgData() { }


    /// <summary>
    /// Initializes <see cref="IgImgData"/> instance with <see cref="IgMagickReadData"/> value.
    /// </summary>
    public IgImgData(IgMagickReadData data)
    {
        FrameCount = data.FrameCount;

        // multi-frames
        if (data.MultiFrameImage != null)
        {
            Width = data.MultiFrameImage[0]?.Width ?? 0;
            Height = data.MultiFrameImage[0]?.Height ?? 0;
            HasAlpha = data.MultiFrameImage.Any(imgM => imgM.HasAlpha);
            CanAnimate = data.MultiFrameImage.Any(imgM => imgM.GifDisposeMethod != GifDisposeMethod.Undefined);

            if (CanAnimate)
            {
                data.MultiFrameImage.Coalesce();
                Source = data.MultiFrameImage.ToBitmap(ImageFormat.Gif);
            }
            else
            {
                var bytes = data.MultiFrameImage.ToByteArray(MagickFormat.Tiff);
                Source = WicBitmapDecoder.Load(new MemoryStream(bytes));
            }
        }
        // single frame
        else
        {
            Width = data.SingleFrameImage?.Width ?? 0;
            Height = data.SingleFrameImage?.Height ?? 0;
            HasAlpha = data.SingleFrameImage?.HasAlpha ?? false;
            Image = BHelper.ToWicBitmapSource(data.SingleFrameImage?.ToBitmapSourceWithDensity());

            // fall back to GDI+ Bitmap
            if (Image == null)
            {
                Source = data.SingleFrameImage.ToBitmapWithDensity();
            }
        }
    }
}
