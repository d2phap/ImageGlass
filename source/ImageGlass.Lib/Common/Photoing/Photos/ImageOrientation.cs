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
using System;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// One of the 8 right-angle orientations: mirrored horizontally if <c>IsMirrored</c>, then turned <c>Rotation</c> degrees clockwise (0, 90, 180, 270).
/// </summary>
public readonly record struct ImageOrientation(int Rotation, bool IsMirrored)
{
    /// <summary>
    /// Gets the orientation that leaves the image unchanged.
    /// </summary>
    public static ImageOrientation Identity { get; } = new(0, false);


    /// <summary>
    /// Gets a value indicating whether the orientation leaves the image unchanged.
    /// </summary>
    public bool IsIdentity => Rotation == 0 && !IsMirrored;


    /// <summary>
    /// Gets a value indicating whether the width and height of the image swap.
    /// </summary>
    public bool SwapsAxes => Rotation is 90 or 270;


    /// <summary>
    /// Gets the flips this orientation applies before its rotation.
    /// </summary>
    public FlipOptions Flips => IsMirrored ? FlipOptions.Horizontal : FlipOptions.None;


    /// <summary>
    /// Returns this orientation followed by a clockwise rotation; the degrees must be a multiple of 90.
    /// </summary>
    public ImageOrientation Rotate(int degrees) => new(Normalize(Rotation + degrees), IsMirrored);


    /// <summary>
    /// Returns this orientation followed by the given flips of the image as it is shown.
    /// </summary>
    public ImageOrientation Flip(FlipOptions options)
    {
        var result = this;

        // mirroring after a rotation by r equals mirroring first, then rotating by -r
        if (options.HasFlag(FlipOptions.Horizontal))
        {
            result = new(Normalize(-result.Rotation), !result.IsMirrored);
        }

        // a vertical flip is a horizontal one followed by a half turn
        if (options.HasFlag(FlipOptions.Vertical))
        {
            result = new(Normalize(180 - result.Rotation), !result.IsMirrored);
        }

        return result;
    }


    /// <summary>
    /// Builds the orientation of flips applied first, then a clockwise rotation; the degrees must be a multiple of 90.
    /// </summary>
    public static ImageOrientation From(FlipOptions flips, double rotation)
    {
        return Identity.Flip(flips).Rotate((int)Math.Round(rotation));
    }


    /// <summary>
    /// Gets the size of an image of the given size after this orientation.
    /// </summary>
    public (int Width, int Height) GetSize(int width, int height)
    {
        return SwapsAxes ? (height, width) : (width, height);
    }


    /// <summary>
    /// Normalizes a right-angle rotation into [0, 360).
    /// </summary>
    private static int Normalize(int degrees)
    {
        var turns = (int)Math.Round(degrees / 90d);
        return (turns % 4 + 4) % 4 * 90;
    }
}
