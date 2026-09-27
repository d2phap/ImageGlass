/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using ImageGlass.Common.Types;
using SkiaSharp;
using System;

namespace ImageGlass.UI.Viewer.Transitions;


/// <summary>
/// State of the transition the viewer is playing; guarded by the viewer's lock.
/// </summary>
internal sealed class ViewerTransition(TransitionRequest request, SKPicture fromFrame) : PhDisposable
{
    /// <summary>
    /// Gets the request that started this transition.
    /// </summary>
    public TransitionRequest Request { get; } = request;


    /// <summary>
    /// Gets the frame on screen before the switch, in viewer coordinates; it holds its own refs to the old images.
    /// </summary>
    public SKPicture FromFrame { get; } = fromFrame;

    private bool _isFromFrameTaken;


    /// <summary>
    /// Gets the tick count when the transition began waiting for the new photo.
    /// </summary>
    public long HoldStartTick { get; } = Environment.TickCount64;


    /// <summary>
    /// Gets, sets the animation frame time the effect started at; <c>null</c> while holding the old frame.
    /// </summary>
    public TimeSpan? StartTime { get; set; }


    /// <summary>
    /// Gets, sets the eased effect progress, from 0 to 1.
    /// </summary>
    public float Progress { get; set; }


    /// <summary>
    /// Hands <see cref="FromFrame"/> over to a transition that replaces this one, so disposing this keeps it.
    /// </summary>
    public SKPicture TakeFromFrame()
    {
        _isFromFrameTaken = true;
        return FromFrame;
    }


    protected override void OnDisposing()
    {
        base.OnDisposing();

        if (!_isFromFrameTaken) FromFrame.Dispose();
        Request.Complete();
    }

}
