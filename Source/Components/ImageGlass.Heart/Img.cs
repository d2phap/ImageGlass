using System;
using System.Drawing;
using System.Threading.Tasks;
using ImageMagick;

namespace ImageGlass.Heart
{
    public class Img: IDisposable
    {

        #region PRIVATE PROPERTIES

        private Exception _Error = null;
        private bool _IsDone = false;

        #endregion



        #region PUBLIC PROPERTIES

        /// <summary>
        /// Gets the error details
        /// </summary>
        public Exception Error => this._Error;


        /// <summary>
        /// Gets the value indicates that image loading is done
        /// </summary>
        public bool IsDone => this._IsDone;


        /// <summary>
        /// Gets, sets filename of Img
        /// </summary>
        public string Filename { get; set; } = string.Empty;


        /// <summary>
        /// Gets, sets MagickImageCollection data
        /// </summary>
        public MagickImageCollection ImgCollection { get; set; } = new MagickImageCollection();


        #endregion



        /// <summary>
        /// The Img class contain ImageMagick data
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
            this._IsDone = false;
            this._Error = null;

            this.ImgCollection.Dispose();
        }


        /// <summary>
        /// Load the image
        /// </summary>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        public async Task LoadAsync(Size size = new Size(), string colorProfileName = "", bool isApplyColorProfileForAll = false)
        {
            // reset done status
            this._IsDone = false;

            // reset error
            this._Error = null;


            try
            {
                // load image data
                this.ImgCollection = await Photo.LoadAsync(
                    filename: this.Filename,
                    size,
                    colorProfileName,
                    isApplyColorProfileForAll
                );
            }
            catch (Exception ex)
            {
                // save the error
                this._Error = ex;
            }


            // done loading
            this._IsDone = true;
        }


        /// <summary>
        /// Get thumbnail
        /// </summary>
        /// <param name="size">A custom size of thumbnail</param>
        /// <param name="useEmbeddedThumbnail">Return the embedded thumbnail if required size was not found.</param>
        /// <returns></returns>
        public async Task<IMagickImage> GetThumbnailAsync(Size size, bool useEmbeddedThumbnail = true)
        {
            return await Photo.GetThumbnailAsync(this.Filename, size, useEmbeddedThumbnail);
        }

        #endregion

    }
}
