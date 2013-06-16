using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Windows.Forms;

namespace igcmd
{
    public static class Setting
    {

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
    }
}
