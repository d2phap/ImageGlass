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

            Bitmap?.Dispose();
            Bitmap = null;

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
    public Bitmap? Bitmap { get; set; } = null;

    /// <summary>
    /// Checks if both <see cref="Image"/> and <see cref="Bitmap"/> are null;
    /// </summary>
    public bool IsImageNull => Image == null && Bitmap == null;
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

        if (data.MultiFrameImage != null)
        {
            CanAnimate = data.MultiFrameImage.Any(imgM => imgM.GifDisposeMethod != GifDisposeMethod.Undefined);
            HasAlpha = data.MultiFrameImage.Any(imgM => imgM.HasAlpha);

            if (CanAnimate)
            {
                data.MultiFrameImage.Coalesce();
                Bitmap = data.MultiFrameImage.ToBitmap(ImageFormat.Gif);
            }
            else
            {
                Bitmap = data.MultiFrameImage.ToBitmap(ImageFormat.Tiff);
                Image = BHelper.ToWicBitmapSource(Bitmap);

                if (Image != null)
                {
                    Bitmap.Dispose();
                    Bitmap = null;
                }
            }
        }
        else
        {
            HasAlpha = data.SingleFrameImage?.HasAlpha ?? false;
            Image = BHelper.ToWicBitmapSource(data.SingleFrameImage?.ToBitmapSourceWithDensity());

            // fall back to GDI+ Bitmap
            if (Image == null)
            {
                Bitmap = data.SingleFrameImage.ToBitmapWithDensity();
            }
        }
    }
}
