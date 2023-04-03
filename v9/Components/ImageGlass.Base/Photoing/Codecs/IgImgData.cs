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

    public bool IsDisposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
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
            else if (Source is WicBitmapDecoder decoder)
            {
                decoder.Dispose();
            }
            else if (Source is AnimatedImg animatedImg)
            {
                foreach (var frame in animatedImg.Frames)
                {
                    frame.Dispose();
                }
                animatedImg.Dispose();
            }
            Source = null;

            FrameCount = 0;
            HasAlpha = false;
            CanAnimate = false;
        }

        // Free any unmanaged objects here.
        IsDisposed = true;
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
            HasAlpha = data.MultiFrameImage.Any(imgM => imgM.HasAlpha);
            CanAnimate = data.MultiFrameImage.Any(imgM => imgM.GifDisposeMethod != GifDisposeMethod.Undefined);

            if (CanAnimate)
            {
                // fall back to use Magick.NET
                data.MultiFrameImage.Coalesce();
                var frames = data.MultiFrameImage.AsEnumerable().Select(frame =>
                {
                    var duration = frame.AnimationDelay > 0 ? frame.AnimationDelay : 10;
                    duration = duration * 1000 / frame.AnimationTicksPerSecond;

                    return new AnimatedImgFrame(frame.ToBitmap(ImageFormat.Gif), duration);
                });

                Source = new AnimatedImg(frames);
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
