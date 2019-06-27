using ImageMagick;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImageGlass.Heart
{
    public class Img: IDisposable
    {

        #region PUBLIC PROPERTIES

        /// <summary>
        /// Gets the error details
        /// </summary>
        public Exception Error { get; private set; } = null;


        /// <summary>
        /// Gets the value indicates that image loading is done
        /// </summary>
        public bool IsDone { get; private set; } = false;


        /// <summary>
        /// Gets, sets filename of Img
        /// </summary>
        public string Filename { get; set; } = string.Empty;


        /// <summary>
        /// Gets, sets Bitmap data
        /// </summary>
        public Bitmap Image { get; set; } = null;


        /// <summary>
        /// Gets, sets number if image frames
        /// </summary>
        public int FrameCount { get; private set; } = 0;

        #endregion



        /// <summary>
        /// The Img class contain image data
        /// </summary>
        /// <param name="filename">Image filename</param>
        public Img(string filename)
        {
            this.Filename = filename;
        }


        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Release all resources of Img
        /// </summary>
        public void Dispose()
        {
            this.IsDone = false;
            this.Error = null;

            if (this.Image != null)
            {
                this.Image.Dispose();
            }
        }


        /// <summary>
        /// Load the image
        /// </summary>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        public async Task LoadAsync(Size size = new Size(), string colorProfileName = "", bool isApplyColorProfileForAll = false, Channels channel = Channels.Default)
        {
            // reset done status
            this.IsDone = false;

            // reset error
            this.Error = null;


            try
            {
                // load image data
                this.Image = await Photo.LoadAsync(
                    filename: this.Filename,
                    size,
                    colorProfileName,
                    isApplyColorProfileForAll,
                    channel: channel
                );

                // Get frame count
                FrameDimension dim = new FrameDimension(this.Image.FrameDimensionsList[0]);
                this.FrameCount = this.Image.GetFrameCount(dim);
            }
            catch (Exception ex)
            {
                // save the error
                this.Error = ex;
            }


            // done loading
            this.IsDone = true;
        }


        /// <summary>
        /// Get thumbnail
        /// </summary>
        /// <param name="size">A custom size of thumbnail</param>
        /// <param name="useEmbeddedThumbnail">Return the embedded thumbnail if required size was not found.</param>
        /// <returns></returns>
        public async Task<Bitmap> GetThumbnailAsync(Size size, bool useEmbeddedThumbnail = true)
        {
            return await Photo.GetThumbnailAsync(this.Filename, size, useEmbeddedThumbnail);
        }

        #endregion

    }
}
