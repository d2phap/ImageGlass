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
namespace ImageGlass.Base.Photoing.Codecs;

public class ImgTransform
{
    private float _rotation = 0;
    private FlipOptions _flipOptions = FlipOptions.None;


    /// <summary>
    /// Contains all flips of the image.
    /// </summary>
    public FlipOptions Flips
    {
        get => _flipOptions;
        set
        {
            _flipOptions = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }


    /// <summary>
    /// Contains the rotation in degree of the image.
    /// </summary>
    public float Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }


    /// <summary>
    /// Checks if there are changes.
    /// </summary>
    public bool HasChanges => Flips != FlipOptions.None || Rotation != 0;


    /// <summary>
    /// Occurs when there is a change.
    /// </summary>
    public event EventHandler<EventArgs>? Changed;


    /// <summary>
    /// Clears all pending changes.
    /// </summary>
    public void Clear()
    {
        Flips = FlipOptions.None;
        Rotation = 0;

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
