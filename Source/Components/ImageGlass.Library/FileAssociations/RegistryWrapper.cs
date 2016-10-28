/*
* Copyright (c) 2006, Brendan Grant (grantb@dahat.com)
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * All original and modified versions of this source code must include the
*       above copyright notice, this list of conditions and the following
*       disclaimer.
*     * This code may not be used with or within any modules or code that is 
*       licensed in any way that that compels or requires users or modifiers
*       to release their source code or changes as a requirement for
*       the use, modification or distribution of binary, object or source code
*       based on the licensed source code. (ex: Cannot be used with GPL code.)
*     * The name of Brendan Grant may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY BRENDAN GRANT ``AS IS'' AND ANY EXPRESS OR
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
* OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
* EVENT SHALL BRENDAN GRANT BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
* SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
* PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; 
* OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
* WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
* OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
* ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using Microsoft.Win32;

namespace ImageGlass.Library.FileAssociations
{
    /// <summary>
    /// Provides a streamlined interface for reading and writing to the registry.
    /// </summary>
    class RegistryWrapper
    {
        /// <summary>
        /// Reads specified value from the registry.
        /// </summary>
        /// <param name="path">Full registry key (minus root) that contains value.</param>
        /// <param name="valueName">Name of the value within key that will be read.</param>
        /// <returns>Read value.</returns>
        public object Read(string path, string valueName)
        {
            RegistryKey key = Registry.ClassesRoot;
            string[] parts = path.Split('\\');

            if (parts == null || parts.Length == 0)
            {
                return null;
            }

            for (int x = 0; x < parts.Length; x++)
            {
                key = key.OpenSubKey(parts[x]);

                if (key == null)
                    return null;

                if (x == parts.Length - 1)
                {
                    return key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                }

            }

            return null;
        }

        /// <summary>
        /// Writes specified value to the registry.
        /// </summary>
        /// <param name="path">Full registry key (minus root) that will contain the value.</param>
        /// <param name="valueName">Name of the value within key that will be written.</param>
        /// <param name="value">Value to be written</param>
        public void Write(string path, string valueName, object value)
        {
            RegistryKey key = Registry.ClassesRoot;
            RegistryKey lastKey = key;
            string[] parts = path.Split('\\');

            if (parts == null || parts.Length == 0)
            {
                return;
            }

            for (int x = 0; x < parts.Length; x++)
            {
                key = key.OpenSubKey(parts[x], true);

                if (key == null)
                {
                    key = lastKey.CreateSubKey(parts[x]);
                }

                if (x == parts.Length - 1)
                {
                    if (value is string)
                    {
                        key.SetValue(valueName, value.ToString());
                    }
                    else if (value is uint || value.GetType().IsEnum)
                    {
                        object o = key.GetValue(valueName, null);

                        if (o == null)
                        {
                            key.SetValue(valueName, value, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryValueKind kind = key.GetValueKind(valueName);

                            if (kind == RegistryValueKind.DWord)
                            {
                                key.SetValue(valueName, value, RegistryValueKind.DWord);
                            }
                            else if (kind == RegistryValueKind.Binary)
                            {
                                uint num = (uint)value;

                                byte[] b = new byte[4];
                                b[0] = (byte)((num & 0x000000FF) >> 0);
                                b[1] = (byte)((num & 0x0000FF00) >> 1);
                                b[2] = (byte)((num & 0x00FF0000) >> 2);
                                b[3] = (byte)((num & 0xFF000000) >> 3);


                                b[0] = (byte)((num & 0x000000FF) >> 0);
                                b[1] = (byte)((num & 0x0000FF00) >> 8);
                                b[2] = (byte)((num & 0x00FF0000) >> 16);
                                b[3] = (byte)((num & 0xFF000000) >> 24);

                                key.SetValue(valueName, b, RegistryValueKind.Binary);
                            }
                            else if (kind == RegistryValueKind.String)
                            {
                                key.SetValue(valueName, "x" + ((uint)value).ToString("X8"));
                            }

                        }

                    }
                    else if (value is Guid)
                    {
                        key.SetValue(valueName, ((Guid)value).ToString("B"));
                    }

                }

                lastKey = key;
            }

            if (key != null)
                key.Close();
        }

        /// <summary>
        /// Deletes specified value;
        /// </summary>
        /// <param name="path">Full registry key (minus root) that contains the value to be deleted.</param>
        /// <param name="valueName">Name of value to be deleted</param>
        public void Delete(string path, string valueName)
        {
            RegistryKey key = Registry.ClassesRoot;
            string[] parts = path.Split('\\');

            if (parts == null || parts.Length == 0)
            {
                return;
            }

            for (int x = 0; x < parts.Length; x++)
            {
                key = key.OpenSubKey(parts[x], true);

                if (key == null)
                    return;

                if (x == parts.Length - 1)
                {
                    key.DeleteValue(valueName, false);
                }
            }
        }

    }
}