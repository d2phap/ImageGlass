using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Security.Permissions;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Configuration;

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
                supportedExts = GetConfig("SupportedExtensions", supportedExts);

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

                SetConfig("ContextMenuExtensions", extensions);
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
                supportedExts = GetConfig("SupportedExtensions", supportedExts);

                string[] exts = supportedExts.Replace("*", "").Split(new char[] { ';' },
                                                StringSplitOptions.RemoveEmptyEntries);

                foreach (string ext in exts)
                {
                    RemoveContextMenuItem(ext, "Open with ImageGlass");
                }

                SetConfig("ContextMenuExtensions", "");
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



        
        /// <summary>
        /// Lấy thông tin cấu hình. Trả về "" nếu không tìm thấy.
        /// </summary>
        /// <param name="key">Tên cấu hình</param>
        /// <returns></returns>
        public static string GetConfig(string key)
        {
            return GetConfig(key, "");
        }

        /// <summary>
        /// Lấy thông tin cấu hình
        /// </summary>
        /// <param name="key">Tên cấu hình</param>
        /// <param name="defaultValue">Giá trị mặc định nếu không tìm thấy</param>
        /// <returns></returns>
        public static string GetConfig(string key, string defaultValue)
        {
            // Open App.Config of executable
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration
                                                        (Application.StartupPath + "\\ImageGlass.exe");

            //Kiểm tra sự tồn tại của Key
            int index = config.AppSettings.Settings.AllKeys.ToList().IndexOf(key);

            //Nếu tồn tại
            if (index != -1)
            {
                //Thì lấy giá trị
                return config.AppSettings.Settings[key].Value;
            }
            else //Nếu không tồn tại
            {
                //Trả về giá trị mặc định
                return defaultValue;
            }

        }

        /// <summary>
        /// Gán thông tin cấu hình
        /// </summary>
        /// <param name="key">Tên cấu hình</param>
        /// <param name="value">Giá trị cấu hình</param>
        public static void SetConfig(string key, string value)
        {
            // Open App.Config of executable
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration
                                                        (Application.StartupPath + "\\ImageGlass.exe");

            //Kiểm tra sự tồn tại của Key
            int index = config.AppSettings.Settings.AllKeys.ToList().IndexOf(key);

            //Nếu tồn tại
            if (index != -1)
            {
                //Thì cập nhật
                config.AppSettings.Settings[key].Value = value;
            }
            else //Nếu không tồn tại
            {
                //Tạo Key mới
                config.AppSettings.Settings.Add(key, value);
            }

            // Save the changes in App.config file.
            config.Save(ConfigurationSaveMode.Modified);


            // Force a reload of a changed section.
            ConfigurationManager.RefreshSection("appSettings");

        }
        #endregion





    }
}
