using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Security.Permissions;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Configuration;
using ImageGlass.Services.Configuration;
using ImageGlass.Library.FileAssociations;
using System.IO;

namespace igtasks
{
    public static class Functions
    {
        /// <summary>
        /// Thêm menu 'Open with ImageGlass' vào menu ngữ cảnh
        /// </summary>
        /// <param name="igPath">Đường dẫn của tập tin ImageGlass.exe</param>        
        public static void AddImageGlassToContextMenu(string igPath)
        {
            try
            {
                string supportedExts = "*.jpg;*.jpe;*.jfif;*.jpeg;*.png;" +
                                                         "*.gif;*.ico;*.bmp;*.dib;*.tif;*.tiff;" +
                                                         "*.exif;*.wmf;*.emf;";
                supportedExts = GlobalSetting.GetConfig("SupportedExtensions", supportedExts);

                AddImageGlassToContextMenu(igPath, supportedExts);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Thêm menu 'Open with ImageGlass' vào menu ngữ cảnh
        /// </summary>
        /// <param name="igPath">Đường dẫn của tập tin ImageGlass.exe</param>
        /// <param name="extensions">Thành phần mở rộng, ví dụ: .png;.jpg</param>
        public static void AddImageGlassToContextMenu(string igPath, string extensions)
        {
            try
            {
                string[] exts = extensions.Replace("*", "").Split(new char[] { ';' },
                                                StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string ext in exts)
                {
                    AddContextMenuItem(ext, "Open with ImageGlass", "", igPath + " %1", igPath, "0");
                }

                GlobalSetting.SetConfig("ContextMenuExtensions", extensions);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        /// <summary>
        /// Xoá tất cả menu 'Open with ImageGlass'
        /// </summary>
        public static void RemoveImageGlassToContextMenu()
        {
            try
            {
                string supportedExts = "*.jpg;*.jpe;*.jfif;*.jpeg;*.png;" +
                                        "*.gif;*.ico;*.bmp;*.dib;*.tif;*.tiff;" +
                                        "*.exif;*.wmf;*.emf;";
                supportedExts = GlobalSetting.GetConfig("SupportedExtensions", supportedExts);

                string[] exts = supportedExts.Replace("*", "").Split(new char[] { ';' },
                                                StringSplitOptions.RemoveEmptyEntries);

                foreach (string ext in exts)
                {
                    RemoveContextMenuItem(ext, "Open with ImageGlass");
                }

                GlobalSetting.SetConfig("ContextMenuExtensions", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Xoá menu 'Open with ImageGlass'
        /// </summary>
        /// <param name="extensions">Thành phần mở rộng, ví dụ: .png;.jpg;.bmp</param>
        public static void RemoveImageGlassToContextMenu(string extensions)
        {
            try
            {
                string[] exts = extensions.Replace("*", "").Split(new char[] { ';' },
                                                StringSplitOptions.RemoveEmptyEntries);

                foreach (string ext in exts)
                {
                    RemoveContextMenuItem(ext, "Open with ImageGlass");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Register file association
        /// </summary>
        /// <param name="ext">Extension, for ex: .png</param>
        /// <param name="programID">Program ID, for ex: ImageGlass.PNG</param>
        /// <param name="description">Extension description</param>
        /// <param name="icon">.ICO path</param>
        /// <param name="igPath">Executable file</param>
        /// <param name="appName">Application name</param>
        public static void RegisterAssociation(string ext, string programID,
            string description, string icon, string igPath, string appName)
        {
            // Initializes a new FileAssociator to associate the .ABC file extension.
            FileAssociator assoc = new FileAssociator(ext);

            // Creates a new file association for the .ABC file extension. Data is overwritten if it already exists.
            assoc.Create(programID,
                description,
                new ProgramIcon(icon),
                new ExecApplication(igPath),
                new OpenWithList(new string[] { appName }));

            MessageBox.Show("dsgsdg");
        }


        /// <summary>
        /// Install new extensions
        /// </summary>
        public static void InstallExtensions()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "ImageGlass extensions (*.dll)|*.dll";
            o.Multiselect = true;

            if (o.ShowDialog() == DialogResult.OK)
            {
                foreach (string f in o.FileNames)
                {
                    try
                    {
                        File.Copy(f, GlobalSetting.StartUpDir + "Plugins\\" + Path.GetFileName(f));
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
                        File.Copy(f, GlobalSetting.StartUpDir + "Languages\\" + Path.GetFileName(f));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }


        #region General functions
        /// <summary>
        /// Thêm menu vào menu ngữ cảnh
        /// </summary>
        /// <param name="extension">Tên thành phần mở rộng, ví dụ .png</param>
        /// <param name="menuName">Tên của menu mới</param>
        /// <param name="menuDescription">Miêu tả của menu mới</param>
        /// <param name="exePath">Đường dẫn ứng dụng + command</param>
        /// <param name="iconFile">Đường dẫn tập tin icon, có thể là thư viện icon DLL, EXE, ...</param>
        /// <param name="iconIndex">Chỉ số icon sẽ hiển thị, mặc định là 0</param>
        /// <returns></returns>
        public static bool AddContextMenuItem(string extension, string menuName,
                                            string menuDescription, string exePath,
                                            string iconFile, string iconIndex)
        {
            bool ret = false;

            //Open HKEY_CLASS_ROOT\[extension]
            RegistryKey rkey = Registry.ClassesRoot.OpenSubKey(extension);

            if (rkey != null)
            {
                //Get extension string
                string extstring = rkey.GetValue("").ToString();
                rkey.Close();

                if (extstring != null)
                {
                    if (extstring.Length > 0)
                    {
                        //Open HKEY_CLASS_ROOT\[extstring]
                        rkey = Registry.ClassesRoot.OpenSubKey(extstring, true);

                        if (rkey != null)
                        {
                            //Create HKEY_CLASS_ROOT\[extstring]\shell\[menuName]\command\
                            string strkey = "shell\\" + menuName + "\\command";
                            RegistryKey subky = rkey.CreateSubKey(strkey);

                            if (subky != null)
                            {
                                //Set exePath value for (default)
                                subky.SetValue("", exePath);
                                subky.Close();


                                //Open HKEY_CLASS_ROOT\[extstring]\shell\[menuName]
                                subky = rkey.OpenSubKey("shell\\" + menuName, true);

                                if (subky != null)
                                {
                                    //Set menuDescription for (default)
                                    subky.SetValue("", menuDescription);

                                    if (iconFile != "" && iconIndex != "")
                                    {
                                        //Set iconFile + ", " + iconIndex for Icon 
                                        subky.SetValue("Icon", iconFile + ", " + iconIndex);
                                    }
                                    subky.Close();
                                }

                                ret = true;
                            }

                            rkey.Close();
                        }
                    }
                }
            }

            return ret;
        }


        /// <summary>
        /// Xoá menu tuỳ chọn
        /// </summary>
        /// <param name="extension">Phần mở rộng</param>
        /// <param name="menuName">Tên menu</param>
        /// <returns></returns>
        public static bool RemoveContextMenuItem(string extension, string menuName)
        {
            bool ret = false;

            //Open HKEY_CLASS_ROOT\[extension]
            RegistryKey rkey = Registry.ClassesRoot.OpenSubKey(extension);

            if (rkey != null)
            {
                //Get extension string
                string extstring = rkey.GetValue("").ToString();
                rkey.Close();

                if (extstring != null)
                {
                    if (extstring.Length > 0)
                    {
                        //Open HKEY_CLASS_ROOT\[extstring]
                        rkey = Registry.ClassesRoot.OpenSubKey(extstring, true);

                        if (rkey != null)
                        {
                            //Open HKEY_CLASS_ROOT\[extstring]\shell\
                            RegistryKey subky = rkey.OpenSubKey("shell\\", true);

                            //Delete sub ket tree
                            subky.DeleteSubKeyTree(menuName, false);

                            //Close registry key HKEY_CLASS_ROOT\[extstring]\shell\
                            subky.Close();

                            ret = true;
                        }

                        //Close registry key HKEY_CLASS_ROOT\[extstring]
                        rkey.Close();
                    }
                }
            }

            return ret;
        }


        #endregion


    }
}
