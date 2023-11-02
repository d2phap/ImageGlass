/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace ImageGlass.Library.FileAssociations {
    /// <summary>
    /// An useful class to read/write/delete/count registry keys
    /// </summary>
    public class RegistryHelper {
        /// <summary>
        /// A property to show or hide error messages
        /// (default = false)
        /// </summary>
        public bool ShowError { get; set; } = false;

        /// <summary>
        /// A property to set the SubKey value
        /// (default = "SOFTWARE\PhapSoftware\ImageGlass")
        /// </summary>
        public string SubKey { get; set; } = @"SOFTWARE\PhapSoftware\ImageGlass";

        /// <summary>
        /// A property to set the BaseRegistryKey value.
        /// (default = Registry.LocalMachine)
        /// </summary>
        public RegistryKey BaseRegistryKey { get; set; } = Registry.LocalMachine;

        /// <summary>
        /// To read a registry key.
        /// input: KeyName (string)
        /// output: value (string)
        /// </summary>
        public string Read(string KeyName) {
            // Opening the registry key
            using (var rk = BaseRegistryKey) {
                // Open a subKey as read-only
                var sk1 = rk.OpenSubKey(SubKey);
                // If the RegistrySubKey doesn't exist -> (null)
                if (sk1 == null) {
                    return null;
                }
                else {
                    try {
                        // If the RegistryKey exists I get its value
                        // or null is returned.
                        return (string)sk1.GetValue(KeyName);
                    }
                    catch (Exception e) {
                        // AAAAAAAAAAARGH, an error!
                        ShowErrorMessage(e, "Reading registry " + KeyName);
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// To write into a registry key.
        /// input: KeyName (string) , Value (object)
        /// output: true or false
        /// </summary>
        public bool Write(string KeyName, object Value) {
            try {
                // Setting
                var rk = BaseRegistryKey;
                // I have to use CreateSubKey 
                // (create or open it if already exits), 
                // 'cause OpenSubKey open a subKey as read-only
                var sk1 = rk.CreateSubKey(SubKey);
                // Save the value
                sk1.SetValue(KeyName, Value);

                return true;
            }
            catch (Exception e) {
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
        public bool DeleteKey(string KeyName) {
            try {
                // Setting
                var rk = BaseRegistryKey;
                var sk1 = rk.CreateSubKey(SubKey);
                // If the RegistrySubKey doesn't exists -> (true)
                if (sk1 == null) {
                    return true;
                }
                else {
                    sk1.DeleteValue(KeyName);
                }

                return true;
            }
            catch (Exception e) {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Deleting SubKey " + SubKey);
                return false;
            }
        }

        /// <summary>
        /// To delete a sub key and any child.
        /// input: void
        /// output: true or false
        /// </summary>
        public bool DeleteSubKeyTree() {
            try {
                // Setting
                var rk = BaseRegistryKey;
                var sk1 = rk.OpenSubKey(SubKey);
                // If the RegistryKey exists, I delete it
                if (sk1 != null) {
                    rk.DeleteSubKeyTree(SubKey);
                }

                return true;
            }
            catch (Exception e) {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Deleting SubKey " + SubKey);
                return false;
            }
        }

        /// <summary>
        /// Retrieve the count of subkeys at the current key.
        /// input: void
        /// output: number of subkeys
        /// </summary>
        public int SubKeyCount() {
            try {
                // Setting
                var rk = BaseRegistryKey;
                var sk1 = rk.OpenSubKey(SubKey);
                // If the RegistryKey exists...
                if (sk1 != null) {
                    return sk1.SubKeyCount;
                }
                else {
                    return 0;
                }
            }
            catch (Exception e) {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Retrieving subkeys of " + SubKey);
                return 0;
            }
        }

        /// <summary>
        /// Retrieve the count of values in the key.
        /// input: void
        /// output: number of keys
        /// </summary>
        public int ValueCount() {
            try {
                // Setting
                var rk = BaseRegistryKey;
                var sk1 = rk.OpenSubKey(SubKey);
                // If the RegistryKey exists...
                if (sk1 != null) {
                    return sk1.ValueCount;
                }
                else {
                    return 0;
                }
            }
            catch (Exception e) {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Retrieving keys of " + SubKey);
                return 0;
            }
        }

        /// <summary>
        /// Retrieve all value names in the key
        /// </summary>
        /// <returns></returns>
        public string[] GetValueNames() {
            try {
                // Setting
                var rk = BaseRegistryKey;
                var sk1 = rk.OpenSubKey(SubKey);
                // If the RegistryKey exists...
                if (sk1 != null) {
                    return sk1.GetValueNames();
                }
                else {
                    return Array.Empty<string>();
                }
            }
            catch (Exception e) {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Retrieving keys of " + SubKey);
                return Array.Empty<string>();
            }
        }

        private void ShowErrorMessage(Exception e, string Title) {
            if (ShowError) {
                MessageBox.Show(e.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
