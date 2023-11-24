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

namespace ImageGlass.Base.Photoing.Codecs;


/// <summary>
/// Contains Magick.NET data after the image file loaded.
/// </summary>
public class IgMagickReadData : IDisposable
{

    #region IDisposable Disposing

    private bool _isDisposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            // Free any other managed objects here.
            MultiFrameImage?.Dispose();
            SingleFrameImage?.Dispose();

            ExifProfile = null;
            ColorProfile = null;
            Extension = string.Empty;
        }

        // Free any unmanaged objects here.
        _isDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~IgMagickReadData()
    {
        Dispose(false);
    }

    #endregion


    public int FrameCount { get; set; } = 0;
    public string Extension { get; set; } = string.Empty;

    public MagickImageCollection? MultiFrameImage { get; set; } = null;
    public MagickImage? SingleFrameImage { get; set; } = null;

    public IColorProfile? ColorProfile { get; set; } = null;
    public IExifProfile? ExifProfile { get; set; } = null;

}
