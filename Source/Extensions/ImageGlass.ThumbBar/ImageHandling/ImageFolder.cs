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
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;


namespace ImageGlass.ThumbBar
{
    
    /// <summary>
    /// Class representing a folder that contains images.
    /// </summary>
    public class ImageFolder
    {
        
        private List<ImageFile> imageFiles;
        private int imageCount;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="folder">The folder containing images.</param>
        public ImageFolder(string folder)
        {
            imageFiles = new List<ImageFile>();
            CreateImagesList(folder);
            imageCount = imageFiles.Count;

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="imageFilesList">Image list</param>
        public ImageFolder(List<string> imageFilesList)
        {
            imageFiles = new List<ImageFile>();
            CreateImagesList(imageFilesList);
            imageCount = imageFiles.Count;
        }


        /// <summary>
        /// Creates the list of image files.
        /// </summary>
        /// <param name="folder">The folder where the images reside</param>
        private void CreateImagesList(string folder)
        {
            DirectoryInfo di = new DirectoryInfo(folder);
            FileInfo[] filesInformation = di.GetFiles("*", SearchOption.TopDirectoryOnly);

            foreach (FileInfo fi in filesInformation)
                switch (fi.Extension.ToLower())
                {
                    case ".jpg":
                    case ".jpe":
                    case ".jfif":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                    case ".ico":
                    case ".bmp":
                    case ".dib":
                    case ".tif":
                    case ".tiff":
                    case ".exif":
                    case ".wmf":
                    case ".emf":
                        imageFiles.Add(new ImageFile(fi.FullName));
                        break;
                }

            imageFiles.Sort(delegate(ImageFile file1, ImageFile file2)
            {
                return file1.ShortFileName.CompareTo(file2.ShortFileName);
            });
        }

        /// <summary>
        /// Creates the list of image files.
        /// </summary>
        /// <param name="imageFilesList">Image list</param>
        private void CreateImagesList(List<string> imageFilesList)
        {
            foreach (string f in imageFilesList)
            {
                imageFiles.Add(new ImageFile(f));
            }
        }


        /// <summary>
        /// Returns the number of images in the folder.
        /// </summary>
        public int ImagesCount
        {
            get { return imageCount; }
        }


        /// <summary>
        /// Returns the images list of the folder.
        /// </summary>
        public IList<ImageFile> ImagesList
        {
            get
            {
                return imageFiles;
            }
        }

    }

}

