/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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


public class IgImgData : IDisposable
{

    #region IDisposable Disposing

    public bool IsDisposed { get; private set; } = false;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
        {
            // Free any other managed objects here.
            Image?.Dispose();
            Image = null;

            ExifProfile = null;
            ColorProfile = null;
        }

        // Free any unmanaged objects here.
        IsDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~IgImgData()
    {
        Dispose(false);
    }

    #endregion


    public Bitmap? Image { get; set; } = null;
    public IExifProfile? ExifProfile { get; set; } = null;
    public IColorProfile? ColorProfile { get; set; } = null;

}
