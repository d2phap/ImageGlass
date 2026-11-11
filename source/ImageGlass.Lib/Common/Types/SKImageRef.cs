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
using ImageGlass.Common.Extensions;
using SkiaSharp;
using System;
using System.Threading;

namespace ImageGlass.Common.Types;

public sealed class SKImageRef
{
    private SKImage? _image;
    private int _refCount;
    private int _disposeRequested;

    /// <summary>
    /// Gets the referenced image instance.
    /// </summary>
    public SKImage? Image => _image;


    /// <summary>
    /// Initializes a new instance of <see cref="SKImageRef"/> with an optional image.
    /// </summary>
    /// <param name="image">The image to reference.</param>
    public SKImageRef(SKImage? image)
    {
        // Store the image and initialize the owner reference count.
        _image = image;
        _refCount = image is null ? 0 : 1;
    }


    /// <summary>
    /// Adds an owner reference to keep the image alive.
    /// </summary>
    public void KeepAlive()
    {
        // No-op if the image is already cleared.
        if (_image is null) return;
        Interlocked.Increment(ref _refCount);
    }


    /// <summary>
    /// Acquires a lease for the image and increments the reference count.
    /// </summary>
    /// <returns>An <see cref="ImageLease"/> that must be disposed, or <see langword="null"/>.</returns>
    public ImageLease? Acquire()
    {
        var image = _image;

        // Reject disposed or missing images.
        if (image is null || image.IsDisposed()) return null;

        // Track an active lease.
        Interlocked.Increment(ref _refCount);
        return new ImageLease(this, image);
    }


    /// <summary>
    /// Requests disposal once the reference count reaches zero.
    /// </summary>
    public void RequestDispose()
    {
        // Nothing to dispose if the image was cleared.
        if (_image is null) return;

        // Mark disposal and release the current owner reference.
        Interlocked.Exchange(ref _disposeRequested, 1);
        Release();
    }


    /// <summary>
    /// Releases a reference and disposes the image when eligible.
    /// </summary>
    private void Release()
    {
        // Dispose only when all references are released and disposal was requested.
        if (Interlocked.Decrement(ref _refCount) == 0 && _disposeRequested == 1)
        {
            var image = Interlocked.Exchange(ref _image, null);
            image?.Dispose();
        }
    }




    public sealed class ImageLease : IDisposable
    {
        private SKImageRef? _owner;
        public SKImage Image { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="ImageLease"/>.
        /// </summary>
        /// <param name="owner">The owning <see cref="SKImageRef"/>.</param>
        /// <param name="image">The leased image.</param>
        public ImageLease(SKImageRef owner, SKImage image)
        {
            _owner = owner;
            Image = image;
        }


        /// <summary>
        /// Releases the lease and decrements the reference count.
        /// </summary>
        public void Dispose()
        {
            // Swap the owner to prevent double-release.
            var owner = Interlocked.Exchange(ref _owner, null);
            owner?.Release();
        }
    }



    /// <summary>
    /// Assigns a new image reference, optionally sharing an existing reference.
    /// </summary>
    /// <param name="srcImg">The field to update.</param>
    /// <param name="newImg">The new image.</param>
    /// <param name="shareFrom">An existing <see cref="SKImageRef"/> to share if it matches.</param>
    public static void Set(ref SKImageRef? srcImg, SKImage? newImg, SKImageRef? shareFrom = null)
    {
        // Avoid unnecessary work when the same image is already assigned.
        if (ReferenceEquals(srcImg?.Image, newImg)) return;

        // Dispose the existing reference if needed.
        srcImg?.RequestDispose();

        // Clear the field when no image is provided.
        if (newImg is null)
        {
            srcImg = null;
            return;
        }

        // Reuse a matching shared reference.
        if (shareFrom is not null && ReferenceEquals(shareFrom.Image, newImg))
        {
            shareFrom.KeepAlive();
            srcImg = shareFrom;
            return;
        }

        // Create a new reference for the image.
        srcImg = new SKImageRef(newImg);
    }

}



