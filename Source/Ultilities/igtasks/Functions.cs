/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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
using System.Windows.Forms;
using Microsoft.Win32;
using ImageGlass.Services.Configuration;
using ImageGlass.Library;
using System.IO;
using ImageGlass.Library.FileAssociations;
using System.Diagnostics;

namespace igtasks
{
    public static class Functions
    {
        /// <summary>
        /// Install new language packs
        /// </summary>
        public static void InstallLanguagePacks()
        {
            OpenFileDialog o = new OpenFileDialog
            {
                Filter = "ImageGlass language pack (*.iglang)|*.iglang",
                Multiselect = true
            };

            if (o.ShowDialog() == DialogResult.OK)
            {
                // create directory if not exist
                if (!Directory.Exists(GlobalSetting.StartUpDir(Dir.Languages))) {
                    Directory.CreateDirectory(GlobalSetting.StartUpDir(Dir.Languages));
                }

                foreach (string f in o.FileNames)
                {
                    try
                    {
                        File.Copy(f, GlobalSetting.StartUpDir(Dir.Languages, Path.GetFileName(f)));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }


        /// <summary>
        /// Create new language packs
        /// </summary>
        public static void CreateNewLanguagePacks()
        {
            SaveFileDialog s = new SaveFileDialog
            {
                Filter = "ImageGlass language pack (*.iglang)|*.iglang"
            };

            if (s.ShowDialog() == DialogResult.OK)
            {
                Language l = new Language();
                l.ExportLanguageToXML(s.FileName);

                try
                {
                    Process p = new Process();
                    p.StartInfo.ErrorDialog = true;
                    p.StartInfo.FileName = "notepad.exe";
                    p.StartInfo.Arguments = "\"" + s.FileName + "\"";
                    p.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        /// <summary>
        /// Edit language packs
        /// </summary>
        public static void EditLanguagePacks(string filename)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.ErrorDialog = true;
                p.StartInfo.FileName = "notepad.exe";
                p.StartInfo.Arguments = "\"" + filename + "\"";
                p.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Delete registry association of ImageGlass
        /// </summary>
        /// <param name="exts">Extensions string to delete. Ex: *.png;*.bmp;</param>
        /// <param name="deleteAllKeys">TRUE: delete all keys</param>
        /// <returns>0 = SUCCESS; 1 = ERROR</returns>
        public static int DeleteRegistryAssociations(string exts, bool deleteAllKeys = false)
        {
            RegistryHelper reg = new RegistryHelper
            {
                ShowError = true,
                BaseRegistryKey = Registry.LocalMachine,

                // delete current registry settings
                SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities\FileAssociations"
            };

            if (!reg.DeleteSubKeyTree()) return 1;


            if (deleteAllKeys)
            {
                reg.SubKey = @"SOFTWARE\RegisteredApplications";
                if (!reg.DeleteKey("ImageGlass")) return 1;

                reg.SubKey = @"SOFTWARE\PhapSoftware";
                if (!reg.DeleteSubKeyTree()) return 1;
            }


            var extList = exts.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in extList)
            {
                reg.SubKey = @"SOFTWARE\Classes\ImageGlass.AssocFile" + ext.ToUpper();
                if (!reg.DeleteSubKeyTree()) return 1;
            }

            return 0;
        }


        /// <summary>
        /// Register file associations
        /// </summary>
        /// <param name="extensions">Extension string, ex: *.png;*.svg;</param>
        /// <returns>0 = SUCCESS; 1 = ERROR</returns>
        public static int SetRegistryAssociations(string extensions)
        {
            DeleteRegistryAssociations(extensions);

            RegistryHelper reg = new RegistryHelper
            {
                ShowError = true,
                BaseRegistryKey = Registry.LocalMachine,

                // Register the application to Registry
                SubKey = @"SOFTWARE\RegisteredApplications"
            };

            if (!reg.Write("ImageGlass", @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities"))
            {
                return 1;
            }

            // Register Capabilities info
            reg.SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities";
            if (!reg.Write("ApplicationName", "ImageGlass"))
            {
                return 1;
            }

            if (!reg.Write("ApplicationIcon", $"\"{GlobalSetting.StartUpDir("ImageGlass.exe")}\", 0"))
            {
                return 1;
            }

            if (!reg.Write("ApplicationDescription", "A lightweight, versatile image viewer"))
            {
                return 1;
            }

            // Register File Associations
            var extList = extensions.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var ext in extList)
            {
                var keyname = "ImageGlass.AssocFile" + ext.ToUpper();

                reg.SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities\FileAssociations";
                if (!reg.Write(ext, keyname))
                {
                    return 1;
                }

                // Config the File Associations - Icon
                var iconPath = GlobalSetting.StartUpDir(@"Ext-Icons\" + ext.ToUpper().Substring(1) + ".ico");
                if (!File.Exists(iconPath))
                {
                    iconPath = GlobalSetting.StartUpDir("ImageGlass.exe");
                }

                reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\DefaultIcon";
                if (!reg.Write("", $"\"{iconPath}\", 0"))
                {
                    return 1;
                }

                // Config the File Associations - Friendly App Name
                reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\shell\open";
                if (!reg.Write("FriendlyAppName", "ImageGlass"))
                {
                    return 1;
                }

                // Config the File Associations - Command
                reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\shell\open\command";
                if (!reg.Write("", $"\"{GlobalSetting.StartUpDir("ImageGlass.exe")}\" \"%1\""))
                {
                    return 1;
                }
            }


            // Register Web-to-App linking
            return SetURIScheme();
        }


        /// <summary>
        /// Delete URI Scheme registry
        /// </summary>
        /// <returns></returns>
        public static int DeleteURIScheme()
        {
            string baseKey = $@"SOFTWARE\Classes\{GlobalSetting.URI_SCHEME}";

            RegistryHelper reg = new RegistryHelper
            {
                ShowError = true,
                BaseRegistryKey = Registry.CurrentUser,
                SubKey = baseKey
            };

            if (!reg.DeleteSubKeyTree()) return 1;


            return 0;
        }



        /// <summary>
        /// Register URI Scheme for Web-to-App linking
        /// </summary>
        /// <returns></returns>
        public static int SetURIScheme()
        {
            DeleteURIScheme();


            string baseKey = $@"SOFTWARE\Classes\{GlobalSetting.URI_SCHEME}";
            RegistryHelper reg = new RegistryHelper
            {
                ShowError = true,
                BaseRegistryKey = Registry.CurrentUser,
                SubKey = baseKey
            };

            if (!reg.Write("", "URL: ImageGlass Protocol"))
            {
                return 1;
            }

            if (!reg.Write("URL Protocol", ""))
            {
                return 1;
            }


            // DefaultIcon
            reg.SubKey = $@"{baseKey}\DefaultIcon";
            if (!reg.Write("", $"\"{GlobalSetting.StartUpDir("ImageGlass.exe")}\", 0"))
            {
                return 1;
            }


            // shell\open\command
            reg.SubKey = $@"{baseKey}\shell\open\command";
            if (!reg.Write("", $"\"{GlobalSetting.StartUpDir("ImageGlass.exe")}\" \"%1\""))
            {
                return 1;
            }

            return 0;
        }
    }
}
