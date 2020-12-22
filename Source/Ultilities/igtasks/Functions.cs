/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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
using ImageGlass.Base;
using ImageGlass.Library;
using ImageGlass.Library.FileAssociations;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace igtasks {
    public static class Functions {
        /// <summary>
        /// Install new language packs
        /// </summary>
        public static void InstallLanguagePacks() {
            var o = new OpenFileDialog {
                Filter = "ImageGlass language pack (*.iglang)|*.iglang",
                Multiselect = true
            };

            if (o.ShowDialog() == DialogResult.OK) {
                // create directory if not exist
                if (!Directory.Exists(App.StartUpDir(Dir.Languages))) {
                    Directory.CreateDirectory(App.StartUpDir(Dir.Languages));
                }

                foreach (var f in o.FileNames) {
                    try {
                        File.Copy(f, App.StartUpDir(Dir.Languages, Path.GetFileName(f)));
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Create new language packs
        /// </summary>
        public static void CreateNewLanguagePacks() {
            var s = new SaveFileDialog {
                Filter = "ImageGlass language pack (*.iglang)|*.iglang"
            };

            if (s.ShowDialog() == DialogResult.OK) {
                var l = new Language();
                l.ExportLanguageToXML(s.FileName);

                try {
                    var p = new Process();
                    p.StartInfo.ErrorDialog = true;
                    p.StartInfo.FileName = "notepad.exe";
                    p.StartInfo.Arguments = "\"" + s.FileName + "\"";
                    p.Start();
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Edit language packs
        /// </summary>
        public static void EditLanguagePacks(string filename) {
            try {
                var p = new Process();
                p.StartInfo.ErrorDialog = true;
                p.StartInfo.FileName = "notepad.exe";
                p.StartInfo.Arguments = "\"" + filename + "\"";
                p.Start();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Delete registry association of ImageGlass
        /// </summary>
        /// <param name="exts">Extensions string to delete. Ex: *.png;*.bmp;</param>
        /// <param name="deleteAllKeys">TRUE: delete all keys</param>
        /// <returns>0 = SUCCESS; 1 = ERROR</returns>
        public static int DeleteRegistryAssociations(string exts, bool deleteAllKeys = false) {
            var reg = new RegistryHelper {
                ShowError = true,
                BaseRegistryKey = Registry.LocalMachine,

                // delete current registry settings
                SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities\FileAssociations",
            };

            if (!reg.DeleteSubKeyTree()) return 1;

            if (deleteAllKeys) {
                reg.SubKey = @"SOFTWARE\RegisteredApplications";
                if (!reg.DeleteKey("ImageGlass")) return 1;

                reg.SubKey = @"SOFTWARE\PhapSoftware";
                if (!reg.DeleteSubKeyTree()) return 1;
            }

            var extList = exts.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in extList) {
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
        public static int SetRegistryAssociations(string extensions) {
            DeleteRegistryAssociations(extensions);

            var reg = new RegistryHelper {
                ShowError = true,
                BaseRegistryKey = Registry.LocalMachine,

                // Register the application to Registry
                SubKey = @"SOFTWARE\RegisteredApplications"
            };

            if (!reg.Write("ImageGlass", @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities")) {
                return 1;
            }

            // Register Capabilities info
            reg.SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities";
            if (!reg.Write("ApplicationName", "ImageGlass")) {
                return 1;
            }

            if (!reg.Write("ApplicationIcon", $"\"{App.IGExePath}\", 0")) {
                return 1;
            }

            if (!reg.Write("ApplicationDescription", "A lightweight, versatile image viewer")) {
                return 1;
            }

            // Register File Associations
            var extList = extensions.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var ext in extList) {
                var keyname = "ImageGlass.AssocFile" + ext.ToUpper();

                reg.SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities\FileAssociations";
                if (!reg.Write(ext, keyname)) {
                    return 1;
                }

                // File type description: ImageGlass JPG File
                reg.SubKey = @"SOFTWARE\Classes\" + keyname;
                if (!reg.Write("", $"ImageGlass {ext.Substring(1).ToUpper()} File")) {
                    return 1;
                }

                // File type icon
                var iconPath = App.StartUpDir(@"Ext-Icons\" + ext.ToUpper().Substring(1) + ".ico");
                if (!File.Exists(iconPath)) {
                    iconPath = App.IGExePath;
                }

                reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\DefaultIcon";
                if (!reg.Write("", $"\"{iconPath}\", 0")) {
                    return 1;
                }

                // Friendly App Name
                reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\shell\open";
                if (!reg.Write("FriendlyAppName", "ImageGlass")) {
                    return 1;
                }

                // Execute command
                reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\shell\open\command";
                if (!reg.Write("", $"\"{App.IGExePath}\" \"%1\"")) {
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
        public static int DeleteURIScheme() {
            var baseKey = $@"SOFTWARE\Classes\{Constants.URI_SCHEME}";

            var reg = new RegistryHelper {
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
        public static int SetURIScheme() {
            DeleteURIScheme();

            var baseKey = $@"SOFTWARE\Classes\{Constants.URI_SCHEME}";
            var reg = new RegistryHelper {
                ShowError = true,
                BaseRegistryKey = Registry.CurrentUser,
                SubKey = baseKey
            };

            if (!reg.Write("", "URL: ImageGlass Protocol")) {
                return 1;
            }

            if (!reg.Write("URL Protocol", "")) {
                return 1;
            }

            // DefaultIcon
            reg.SubKey = $@"{baseKey}\DefaultIcon";
            if (!reg.Write("", $"\"{App.IGExePath}\", 0")) {
                return 1;
            }

            // shell\open\command
            reg.SubKey = $@"{baseKey}\shell\open\command";
            if (!reg.Write("", $"\"{App.IGExePath}\" \"%1\"")) {
                return 1;
            }

            return 0;
        }
    }
}
