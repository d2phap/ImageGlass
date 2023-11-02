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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace ImageGlass.Base.Photoing.Codecs;

public class AnimatedImgFrame : IDisposable
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
            Duration = TimeSpan.Zero;

            Bitmap?.Dispose();
            Bitmap = null;
        }

        // Free any unmanaged objects here.
        IsDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AnimatedImgFrame()
    {
        Dispose(false);
    }

    #endregion


    /// <summary>
    /// Gets frame bitmap.
    /// </summary>
    public IDisposable? Bitmap { get; private set; }


    /// <summary>
    /// Gets frame duration in millisecond.
    /// </summary>
    public TimeSpan Duration { get; private set; }


    /// <summary>
    /// Initialize new instance of <see cref="AnimatedImgFrame"/>.
    /// </summary>
    /// <param name="bmpSrc"></param>
    /// <param name="duration"></param>
    public AnimatedImgFrame(IDisposable? bmpSrc, int duration)
    {
        Bitmap = bmpSrc;
        Duration = TimeSpan.FromMilliseconds(duration);
    }

    /// <summary>
    /// Initialize new instance of <see cref="AnimatedImgFrame"/>.
    /// </summary>
    /// <param name="bmpSrc"></param>
    /// <param name="duration"></param>
    public AnimatedImgFrame(IDisposable? bmpSrc, TimeSpan duration)
    {
        Bitmap = bmpSrc;
        Duration = duration;
    }
}
