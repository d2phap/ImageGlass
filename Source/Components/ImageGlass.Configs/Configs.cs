using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGlass.Settings
{
    public static class Configs
    {
        public static ConfigSource Source { get; private set; } = new ConfigSource();


        /// <summary>
        /// Convert the given value to specific type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="value">Value</param>
        /// <returns></returns>
        private static T ConvertType<T>(object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }


        /// <summary>
        /// Gets config item from ConfigSource
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key of the config</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns></returns>
        public static T Get<T>(string key, object defaultValue)
        {
            try
            {
                return ConvertType<T>(Source[key]);
            }
            catch { }

            // return default value
            return ConvertType<T>(defaultValue);
        }


        /// <summary>
        /// Set the given config to ConfigSource
        /// </summary>
        /// <param name="key">Key of the config</param>
        /// <param name="value">Value</param>
        public static void Set(string key, object value)
        {
            if (Source.ContainsKey(key))
            {
                Source[key] = value.ToString();
            }
            else
            {
                Source.Add(key, value.ToString());
            }
        }
    }
}
