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
using System;

namespace ImageGlass.Common.Photoing;

public class PhotoTransform
{
    private FlipOptions _flipOptions = FlipOptions.None;
    private float _rotation = 0;
    private bool _isColorInverted = false;


    /// <summary>
    /// Occurs when there is a change.
    /// </summary>
    public event TEventHandler<PhotoTransform, EventArgs>? Changed;


    /// <summary>
    /// Gets or sets the flip options, triggers the <see cref="Changed"/> event when changed.
    /// </summary>
    public FlipOptions Flips
    {
        get => _flipOptions;
        set
        {
            if (_flipOptions != value)
            {
                _flipOptions = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }


    /// <summary>
    /// Gets or sets the rotation value, triggers the <see cref="Changed"/> event when changed.
    /// </summary>
    public float Rotation
    {
        get => _rotation;
        set
        {
            if (_rotation != value)
            {
                _rotation = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }


    /// <summary>
    /// Gets or sets a value indicating whether the color is inverted.
    /// Triggers the <see cref="Changed"/> event when changed.
    /// </summary>
    public bool IsColorInverted
    {
        get => _isColorInverted;
        set
        {
            if (_isColorInverted != value)
            {
                _isColorInverted = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }


    /// <summary>
    /// Gets, sets frame index to apply the transformation to.
    /// Use <c>-1</c> to apply to all frames.
    /// Default value is <c>-1</c>.
    /// </summary>
    public int FrameIndex { get; set; } = -1;


    /// <summary>
    /// Gets, sets the flips and rotation as one orientation, triggers the <see cref="Changed"/> event when changed.
    /// </summary>
    public ImageOrientation Orientation
    {
        get => ImageOrientation.From(_flipOptions, _rotation);
        set
        {
            if (Orientation == value) return;

            _flipOptions = value.Flips;
            _rotation = value.Rotation;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }


    /// <summary>
    /// Checks if there are changes.
    /// </summary>
    public bool HasChanges => Flips != FlipOptions.None
        || Rotation != 0
        || IsColorInverted;



    /// <summary>
    /// Clears all pending changes.
    /// </summary>
    public void Clear()
    {
        _flipOptions = FlipOptions.None;
        _rotation = 0;
        _isColorInverted = false;
        FrameIndex = -1;

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
