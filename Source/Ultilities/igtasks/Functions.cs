/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
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
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using ImageGlass.Services.Configuration;
using ImageGlass.Library;
using System.IO;
using ImageGlass.Library.FileAssociations;
using System.Diagnostics;
using System.Threading.Tasks;

namespace igtasks
{
    public static class Functions
    {
        /// <summary>
        /// Install new extensions
        /// </summary>
        public static void InstallExtensions()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "ImageGlass plugins (*.dll)|*.dll";
            o.Multiselect = true;

            if (o.ShowDialog() == DialogResult.OK)
            {
                foreach (string f in o.FileNames)
                {
                    try
                    {
                        File.Copy(f, Path.Combine(GlobalSetting.StartUpDir, "Plugins", Path.GetFileName(f)));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    
                }
            }
        }

        /// <summary>
        /// Install new language packs
        /// </summary>
        public static void InstallLanguagePacks()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "ImageGlass language pack (*.iglang)|*.iglang";
            o.Multiselect = true;

            if (o.ShowDialog() == DialogResult.OK)
            {
                foreach (string f in o.FileNames)
                {
                    try
                    {
                        File.Copy(f, Path.Combine(GlobalSetting.StartUpDir, "Languages", Path.GetFileName(f)));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }

        /// <summary>
        /// Create new language packs
        /// </summary>
        public static void CreateNewLanguagePacks()
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "ImageGlass language pack (*.iglang)|*.iglang";

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
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Delete registry association of ImageGlass
        /// </summary>
        /// <param name="exts">Extensions string to delete. Ex: *.png;*.bmp;</param>
        /// <param name="deleteAllKeys">TRUE: delete all keys</param>
        public static void DeleteRegistryAssociations(string exts, bool deleteAllKeys = false)
        {
            RegistryHelper reg = new RegistryHelper();
            reg.ShowError = true;
            reg.BaseRegistryKey = Registry.LocalMachine;

            // delete current registry settings
            reg.SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities\FileAssociations";
            if (!reg.DeleteSubKeyTree()) return;


            if (deleteAllKeys)
            {
                reg.SubKey = @"SOFTWARE\RegisteredApplications";
                if (!reg.DeleteKey("ImageGlass")) return;

                reg.SubKey = @"SOFTWARE\PhapSoftware";
                if (!reg.DeleteSubKeyTree()) return;
            }


            var extList = exts.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in extList)
            {
                reg.SubKey = @"SOFTWARE\Classes\ImageGlass.AssocFile" + ext.ToUpper();
                if (!reg.DeleteSubKeyTree()) return;
            }
        }

        /// <summary>
        /// Register file associations
        /// </summary>
        /// <param name="extensions">Extension string, ex: *.png;*.svg;</param>
        public static void SetRegistryAssociations(string extensions)
        {
            DeleteRegistryAssociations(extensions);

            RegistryHelper reg = new RegistryHelper();
            reg.ShowError = true;
            reg.BaseRegistryKey = Registry.LocalMachine;

            // Register the application to Registry
            reg.SubKey = @"SOFTWARE\RegisteredApplications";
            if (!reg.Write("ImageGlass", @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities")) return;

            // Register Capabilities info
            reg.SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities";
            if (!reg.Write("ApplicationName", "ImageGlass")) return;
            if (!reg.Write("ApplicationIcon", $"\"{Path.Combine(GlobalSetting.StartUpDir, "ImageGlass.exe")}\", 0")) return;
            if (!reg.Write("ApplicationDescription", "A lightweight, versatile image viewer")) return;

            // Register File Associations
            var extList = extensions.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var ext in extList)
            {
                var keyname = "ImageGlass.AssocFile" + ext.ToUpper();

                reg.SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities\FileAssociations";
                if (!reg.Write(ext, keyname)) return;

                // Config the File Associations - Icon
                var iconPath = Path.Combine(GlobalSetting.StartUpDir, @"Ext-Icons\" + ext.ToUpper().Substring(1) + ".ico");
                if (!File.Exists(iconPath))
                {
                    iconPath = Path.Combine(GlobalSetting.StartUpDir, "ImageGlass.exe");
                }

                reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\DefaultIcon";
                if (!reg.Write("", $"\"{iconPath}\", 0")) return;

                // Config the File Associations - Friendly App Name
                reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\shell\open";
                if (!reg.Write("FriendlyAppName", "ImageGlass")) return;

                // Config the File Associations - Command
                reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\shell\open\command";
                if (!reg.Write("", $"\"{Path.Combine(GlobalSetting.StartUpDir, "ImageGlass.exe")}\" \"%1\"")) return;
            }

            

            
        }

    }
}
