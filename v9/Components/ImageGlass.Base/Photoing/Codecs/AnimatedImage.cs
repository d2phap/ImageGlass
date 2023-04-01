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

public class AnimatedImage : IDisposable
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
            FramesCount = 0;
            Frames = Enumerable.Empty<ImageFrameData>();
        }

        // Free any unmanaged objects here.
        IsDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AnimatedImage()
    {
        Dispose(false);
    }

    #endregion


    /// <summary>
    /// Gets frames list.
    /// </summary>
    public IEnumerable<ImageFrameData> Frames { get; private set; }

    /// <summary>
    /// Gets frams count.
    /// </summary>
    public int FramesCount { get; private set; } = 0;


    /// <summary>
    /// Initialize new instance of <see cref="AnimatedImage"/>.
    /// </summary>
    public AnimatedImage(IEnumerable<ImageFrameData> frames)
    {
        Frames = frames;
        FramesCount = frames.Count();
    }


    /// <summary>
    /// Gets frame data.
    /// </summary>
    public ImageFrameData? GetFrame(int frameIndex)
    {
        try
        {
            return Frames.ElementAtOrDefault(frameIndex);
        }
        catch { }

        return null;
    }
}

