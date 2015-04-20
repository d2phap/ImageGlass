/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2014 DUONG DIEU PHAP
Project homepage: http://imageglass.org

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
using ImageGlass.ThumbBar.ImageHandling;
using System;
using System.Drawing;
using System.IO;


namespace ImageGlass.ThumbBar
{
    
    /// <summary>
    /// Flyweight pattern image file class.
    /// </summary>
    public class ImageFile
        : IDisposable
    {
        
        /// <summary>
        /// The name of the image file.
        /// </summary>
        private string fileName;

        /// <summary>
        /// The image extracted from the file.
        /// </summary>
        private Image anImage;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">The name of the image file</param>
        public ImageFile(string fileName)
        {
            this.fileName = fileName;
        }


        /// <summary>
        /// Gets the image from the file.
        /// </summary>
        public void GetImageFromFile()
        {
            this.anImage = ExifThumbReader.ReadThumb(this.fileName);

            if (this.anImage == null)
            {
                this.anImage = this.FullSizeImage;
            }
        }

        /// <summary>
        /// Gets the full-sized image corresponding to an image file.
        /// </summary>
        public Image FullSizeImage
        {
            get
            {
                try
                {
                    System.IO.FileInfo Fi = new System.IO.FileInfo(this.fileName);
                    double _size = Math.Round((Fi.Length * 1.0) / 1024 / 1024, 2); //in MB

                    if (_size > GlobalData.ThumbnailMaxLoadingSize)
                    {
                        return new Bitmap(GlobalData.TooLargeImageThumbnail);
                    }

                    return new Bitmap(fileName);
                }
                catch
                {
                    return GlobalData.InvalidImage;
                }
            }
        }

        /// <summary>
        /// Gets the thumbnail image corresponding to an image file.
        /// </summary>
        public Image Thumbnail
        {
            get
            {
                Image thumbnail = null;

                //thumbnail = ExifThumbReader.ReadThumb(this.fileName);
                //if (thumbnail != null) return thumbnail;

                System.IO.FileInfo Fi = new System.IO.FileInfo(this.fileName);
                double _size = Math.Round((Fi.Length * 1.0) / 1024 / 1024, 2); //in MB

                if (_size > GlobalData.ThumbnailMaxLoadingSize)
                {
                    thumbnail = GlobalData.TooLargeImageThumbnail;
                }
                else
                {
                    try
                    {
                        if (anImage != null)
                            thumbnail = ImageResizer.CreateThumbnailFromImage(anImage, GlobalData.ThumbnailWidthAndHeight);
                        else
                            thumbnail = ImageResizer.CreateThumbnailFromFile(fileName, GlobalData.ThumbnailWidthAndHeight);
                    }
                    catch
                    {
                        thumbnail = GlobalData.InvalidImageThumbnail;
                    }
                    finally
                    {
                        if ((anImage != null) && (anImage != GlobalData.InvalidImage))
                        {
                            anImage.Dispose();
                            anImage = null;
                        }
                    }
                }

                return thumbnail;
            }
        }

        /// <summary>
        /// Gets the file name (without path) corresponding to the image.
        /// </summary>
        public string ShortFileName
        {
            get { return fileName.Substring(fileName.LastIndexOf(Path.DirectorySeparatorChar) + 1); }
        }


        /// <summary>
        /// Gets the file name (with path) corresponding to the image.
        /// </summary>
        public string LongFileName
        {
            get { return fileName; }
        }


        public void Dispose()
        {
            if ((anImage != null) && (anImage != GlobalData.InvalidImage))
            {
                anImage.Dispose();
                anImage = null;
            }
            GC.SuppressFinalize(this);
        }

        ~ImageFile()
        {
            if ((anImage != null) && (anImage != GlobalData.InvalidImage))
                anImage.Dispose();
        }

    }

}
