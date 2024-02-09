/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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

/// <summary>
/// Initialize new instance of <see cref="AnimatedImg"/>.
/// </summary>
public class AnimatedImg(IEnumerable<AnimatedImgFrame> frames) : IDisposable
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
            FrameCount = 0;
            Frames = [];
        }

        // Free any unmanaged objects here.
        IsDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AnimatedImg()
    {
        Dispose(false);
    }

    #endregion


    /// <summary>
    /// Gets frames list.
    /// </summary>
    public IEnumerable<AnimatedImgFrame> Frames { get; private set; } = frames;

    /// <summary>
    /// Gets frams count.
    /// </summary>
    public int FrameCount { get; private set; } = frames.Count();


    /// <summary>
    /// Gets frame data.
    /// </summary>
    public AnimatedImgFrame? GetFrame(int frameIndex)
    {
        try
        {
            return Frames.ElementAtOrDefault(frameIndex);
        }
        catch { }

        return null;
    }
}

