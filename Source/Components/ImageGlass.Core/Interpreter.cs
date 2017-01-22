using System.Drawing;
using System.IO;
using System.Linq;
using System.Drawing.IconLib;
using ImageMagick;

namespace ImageGlass.Core
{
    public class Interpreter
    {
        public static Bitmap Load(string path)
        {
            path = path.ToLower();
            Bitmap bmp = null;

            if (path.EndsWith(".gif"))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    bmp = new Bitmap(path, true);                    
                }
            }
            else if (path.EndsWith(".ico"))
            {
                bmp = ReadIconFile(path);
            }
            else
            {
                using (var magicImg = new MagickImage(path))
                {
                    //Get Exif information
                    var profile = magicImg.GetExifProfile();
                    if (profile != null)
                    {
                        //Get Orieantation Flag
                        var orientationFlag = int.Parse(profile.GetValue(ExifTag.Orientation).Value.ToString());

                        var orientationDegree = GetOrientationDegree(orientationFlag);
                        if (orientationDegree != 0)
                        {
                            //Rotate image accordingly
                            magicImg.Rotate(orientationDegree);
                        }
                    }
                    
                    bmp = magicImg.ToBitmap();
                }
            }

            return bmp;
        }

        /// <summary>
        /// Read icon *.ICO file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap ReadIconFile(string path)
        {
            MultiIcon mIcon = new MultiIcon();
            mIcon.Load(path);

            //Try to get the largest image of it
            SingleIcon sIcon = mIcon[0];
            IconImage iImage = sIcon.OrderByDescending(ico => ico.Size.Width).ToList()[0];

            //Convert to bitmap
            return iImage.Icon.ToBitmap();
        }

        /// <summary>
        /// Returns Exif rotation in degrees. Returns 0 if the metadata 
        /// does not exist or could not be read. A negative value means
        /// the image needs to be mirrored about the vertical axis.
        /// </summary>
        /// <param name="orientationFlag">Orientation Flag</param>
        /// <returns></returns>
        public static double GetOrientationDegree(int orientationFlag)
        {
            if (orientationFlag == 1)
                return 0;
            else if (orientationFlag == 2)
                return -360;
            else if (orientationFlag == 3)
                return 180;
            else if (orientationFlag == 4)
                return -180;
            else if (orientationFlag == 5)
                return -90;
            else if (orientationFlag == 6)
                return 90;
            else if (orientationFlag == 7)
                return -270;
            else if (orientationFlag == 8)
                return 270;

            return 0;
        }


    }
}
