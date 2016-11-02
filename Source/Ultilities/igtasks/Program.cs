using System;
using System.Windows.Forms;
using igtasks;
using ImageGlass.Library.Image;

namespace adtasks
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static string[] args;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] argv)
        {
            args = argv;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Command
            string topcmd = args[0].ToLower().Trim();


            //Add menu 'Open with ImageGlass'
            #region addext <string igPath> [string ext]
            if (topcmd == "addext")
            {
                //Get executable file path
                string exePath = args[1];

                if (args.Length > 2)
                {
                    //Get extension
                    string ext = args[2];

                    //Apply changes
                    Functions.AddImageGlassToContextMenu(exePath, ext);
                }
                else
                {
                    //Apply changes
                    Functions.AddImageGlassToContextMenu(exePath);
                }

                Application.Exit();
            }
            #endregion

            //Remove menu 'Open with ImageGlass'
            #region removeext
            else if (topcmd == "removeext")
            {
                //Apply changes
                Functions.RemoveImageGlassToContextMenu();

                Application.Exit();
            }
            #endregion

            //Update extension for Menu
            #region updateext <string exePath> <string exts>
            else if (topcmd == "updateext")
            {
                //Remove all
                Functions.RemoveImageGlassToContextMenu();

                //Add new
                if (args.Length > 2)
                {
                    //Get executable file path
                    string exePath = args[1];

                    //Get extension
                    string ext = args[2];

                    //Apply changes
                    Functions.AddImageGlassToContextMenu(exePath, ext);
                }

                Application.Exit();
            }
            #endregion

            //Set desktop wallpaper
            #region setwallpaper <string imgPath> [int style]
            else if (topcmd == "setwallpaper")
            {
                //Get image's path
                string imgPath = args[1];

                if (args.Length > 2)
                {
                    //Get style
                    string style = args[2];

                    //Apply changes
                    if (style == "1")
                    {
                        DesktopWallapaper.Set(new Uri(imgPath), DesktopWallapaper.Style.Stretched);
                    }
                    else if (style == "2")
                    {
                        DesktopWallapaper.Set(new Uri(imgPath), DesktopWallapaper.Style.Tiled);
                    }
                    else //style == "0"
                    {
                        DesktopWallapaper.Set(new Uri(imgPath), DesktopWallapaper.Style.Centered);
                    }
                }
                else
                {
                    //Apply changes
                    DesktopWallapaper.Set(new Uri(imgPath), DesktopWallapaper.Style.Centered);
                }

                Application.Exit();
            }
            #endregion

            //Register file association
            #region regassociations <string appPath> <string exts>
            else if (topcmd == "regassociations")
            {
                //get Executable file
                string appPath = args[1];
                //get Extension
                string exts = args[2];

                Functions.RegisterAssociation(appPath, exts);

                Application.Exit();
            }
            #endregion

            //Install new language packs
            #region iginstalllang
            else if (topcmd == "iginstalllang")
            {
                Functions.InstallLanguagePacks();
                Application.Exit();
            }
            #endregion

            //Create new language packs
            #region ignewlang
            else if (topcmd == "ignewlang")
            {
                Functions.CreateNewLanguagePacks();
                Application.Exit();
            }
            #endregion

            //Edit language packs
            #region igeditlang <string filename>
            else if (topcmd == "igeditlang")
            {
                //get Executable file
                string filename = args[1];

                Functions.EditLanguagePacks(filename);
                Application.Exit();
            }
            #endregion

            //Install new extensions
            #region iginstallext
            else if (topcmd == "iginstallext")
            {
                Functions.InstallExtensions();
                Application.Exit();
            }
            #endregion


        }



    }
}
