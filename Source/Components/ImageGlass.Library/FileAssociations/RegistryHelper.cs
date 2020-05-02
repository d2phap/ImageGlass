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

using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace ImageGlass.Library.FileAssociations
{
    /// <summary>
    /// An useful class to read/write/delete/count registry keys
    /// </summary>
    public class RegistryHelper
    {
        private bool showError = false;

        /// <summary>
        /// A property to show or hide error messages
        /// (default = false)
        /// </summary>
        public bool ShowError
        {
            get { return showError; }
            set { showError = value; }
        }

        private string subKey = @"SOFTWARE\PhapSoftware\ImageGlass";

        /// <summary>
        /// A property to set the SubKey value
        /// (default = "SOFTWARE\PhapSoftware\ImageGlass")
        /// </summary>
        public string SubKey
        {
            get { return subKey; }
            set { subKey = value; }
        }

        private RegistryKey baseRegistryKey = Registry.LocalMachine;

        /// <summary>
        /// A property to set the BaseRegistryKey value.
        /// (default = Registry.LocalMachine)
        /// </summary>
        public RegistryKey BaseRegistryKey
        {
            get { return baseRegistryKey; }
            set { baseRegistryKey = value; }
        }

        /// <summary>
        /// To read a registry key.
        /// input: KeyName (string)
        /// output: value (string)
        /// </summary>
        public string Read(string KeyName)
        {
            // Opening the registry key
            RegistryKey rk = baseRegistryKey;
            // Open a subKey as read-only
            RegistryKey sk1 = rk.OpenSubKey(subKey);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
            {
                return null;
            }
            else
            {
                try
                {
                    // If the RegistryKey exists I get its value
                    // or null is returned.
                    return (string)sk1.GetValue(KeyName);
                }
                catch (Exception e)
                {
                    // AAAAAAAAAAARGH, an error!
                    ShowErrorMessage(e, "Reading registry " + KeyName);
                    return null;
                }
            }
        }

        /// <summary>
        /// To write into a registry key.
        /// input: KeyName (string) , Value (object)
        /// output: true or false
        /// </summary>
        public bool Write(string KeyName, object Value)
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                // I have to use CreateSubKey
                // (create or open it if already exits),
                // 'cause OpenSubKey open a subKey as read-only
                RegistryKey sk1 = rk.CreateSubKey(subKey);
                // Save the value
                sk1.SetValue(KeyName, Value);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Writing registry " + KeyName);
                return false;
            }
        }

        /// <summary>
        /// To delete a registry key.
        /// input: KeyName (string)
        /// output: true or false
        /// </summary>
        public bool DeleteKey(string KeyName)
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.CreateSubKey(subKey);
                // If the RegistrySubKey doesn't exists -> (true)
                if (sk1 == null)
                    return true;
                else
                    sk1.DeleteValue(KeyName);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Deleting SubKey " + subKey);
                return false;
            }
        }

        /// <summary>
        /// To delete a sub key and any child.
        /// input: void
        /// output: true or false
        /// </summary>
        public bool DeleteSubKeyTree()
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                // If the RegistryKey exists, I delete it
                if (sk1 != null)
                    rk.DeleteSubKeyTree(subKey);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Deleting SubKey " + subKey);
                return false;
            }
        }

        /// <summary>
        /// Retrieve the count of subkeys at the current key.
        /// input: void
        /// output: number of subkeys
        /// </summary>
        public int SubKeyCount()
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                // If the RegistryKey exists...
                if (sk1 != null)
                    return sk1.SubKeyCount;
                else
                    return 0;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Retrieving subkeys of " + subKey);
                return 0;
            }
        }

        /// <summary>
        /// Retrieve the count of values in the key.
        /// input: void
        /// output: number of keys
        /// </summary>
        public int ValueCount()
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                // If the RegistryKey exists...
                if (sk1 != null)
                    return sk1.ValueCount;
                else
                    return 0;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Retrieving keys of " + subKey);
                return 0;
            }
        }

        /// <summary>
        /// Retrieve all value names in the key
        /// </summary>
        /// <returns></returns>
        public string[] GetValueNames()
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                // If the RegistryKey exists...
                if (sk1 != null)
                    return sk1.GetValueNames();
                else
                    return new string[0];
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Retrieving keys of " + subKey);
                return new string[0];
            }
        }

        private void ShowErrorMessage(Exception e, string Title)
        {
            if (showError == true)
                MessageBox.Show(e.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}