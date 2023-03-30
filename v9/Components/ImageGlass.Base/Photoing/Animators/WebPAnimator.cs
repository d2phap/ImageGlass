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
using static ImageGlass.WebP.WebPWrapper;

namespace ImageGlass.Base.Photoing.Animators;

public class WebPAnimator : IDisposable
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
            _frames = Enumerable.Empty<FrameData>();

            _framesCount = 0;
            _frameIndex = 0;
            _enable = false;
        }

        // Free any unmanaged objects here.
        IsDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~WebPAnimator()
    {
        Dispose(false);
    }

    #endregion


    private IEnumerable<FrameData> _frames;
    private int _framesCount = 0;
    private int _frameIndex = 0;
    private bool _enable = false;

    public event EventHandler<FrameChangedEventArgs> FrameChanged;


    public WebPAnimator(IEnumerable<FrameData> frames)
    {
        _frames = frames;
        _framesCount = frames.Count();
    }


    /// <summary>
    /// Starts animating the image frames.
    /// </summary>
    public void Animate()
    {
        _enable = true;

        var _thHeartBeat = new Thread(OnThreadHeartBeatTicked);
        _thHeartBeat.IsBackground = true;
        _thHeartBeat.Name = "heartbeat - WebPAnimator";
        _thHeartBeat.Start();
    }


    /// <summary>
    /// Stops image animation.
    /// </summary>
    public void StopAnimate()
    {
        _enable = false;
    }


    /// <summary>
    /// Gets image frame data.
    /// </summary>
    public FrameData? GetFrame(int frameIndex)
    {
        try
        {
            return _frames.ElementAt(frameIndex);
        }
        catch { }

        return null;
    }


    private void OnThreadHeartBeatTicked()
    {
        var initFrame = GetFrame(_frameIndex);
        Thread.Sleep(initFrame.Duration);

        while (_enable)
        {
            _frameIndex++;
            if (_frameIndex >= _framesCount)
            {
                _frameIndex = 0;
            }

            var frame = GetFrame(_frameIndex);
            Thread.Sleep(frame.Duration);

            FrameChanged?.Invoke(this, new FrameChangedEventArgs()
            {
                FrameIndex = _frameIndex,
                Bitmap = frame.Bitmap,
            });

            Thread.Sleep(frame.Duration);
        }
    }


}


public class FrameChangedEventArgs : EventArgs
{
    /// <summary>
    /// The current frame index.
    /// </summary>
    public int FrameIndex { get; init; }

    /// <summary>
    /// The current frame bitmap.
    /// </summary>
    public Bitmap? Bitmap { get; init; }
}
