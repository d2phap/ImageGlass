/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia.Controls;
using ImageGlass.Common;
using System;

namespace ImageGlass.UI.Viewer;

public partial class ViewerControl
{

    /// <summary>
    /// Enables the renderer loop for drawing amination source.
    /// </summary>
    public bool EnableDrawingAnimation { get; set; } = false;


    /// <summary>
    /// Gets the current animating source.
    /// </summary>
    public AnimationSources AnimationSource { get; protected set; } = AnimationSources.None;





    /// <summary>
    /// Prepares logics for frame animation source.
    /// </summary>
    protected virtual void OnPrepareAnimationFrame(TimeSpan ts)
    {
        if (!EnableDrawingAnimation) return;

        CalculateFps();

        if (_lastAnimationFrameTime != TimeSpan.Zero)
        {
            // Calculate delta based on real time passed
            var elapsedSinceUpdate = (ts - _lastAnimationFrameTime).TotalSeconds;

            // How much to zoom this specific frame
            OnDrawAnimationSource(elapsedSinceUpdate);
            InvalidateVisual();
        }
        _lastAnimationFrameTime = ts;


        // loop for drawing animation source
        TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnPrepareAnimationFrame);
    }



    /// <summary>
    /// Starts a built-in animation.
    /// </summary>
    /// <param name="sources">Source of animation</param>
    public void StartDrawingAnimation(AnimationSources sources, int durationMs = int.MaxValue, Action? callbackFn = null)
    {
        if (durationMs <= 0) return;

        AnimationSource = sources;
        EnableDrawingAnimation = true;
        _lastAnimationFrameTime = TimeSpan.Zero;

        TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnPrepareAnimationFrame);


        // set time to stop animation source
        if (durationMs < int.MaxValue)
        {
            BHelper.Debounce(durationMs, OnAnimationStopped,
                new AnimationSourceArgs(sources, callbackFn), true);
        }
    }


    private void OnAnimationStopped(AnimationSourceArgs? e)
    {
        if (e is null) return;
        StopDrawingAnimation(e.Source, e.CallbackFn);
    }



    /// <summary>
    /// Stops a built-in animation.
    /// </summary>
    /// <param name="sources">Source of animation</param>
    public void StopDrawingAnimation(AnimationSources sources, Action? callbackFn = null)
    {
        AnimationSource ^= sources;
        EnableDrawingAnimation = false;
        InvalidateVisual();

        callbackFn?.Invoke();
    }


}
