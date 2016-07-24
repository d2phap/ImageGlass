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
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace ImageGlass.Library.FileAssociations
{
    /// <summary>
    /// Specifies values that control some aspects of the Shell's handling of the file types linked to a ProgID.
    /// </summary>
    [Flags]
    public enum EditFlags : uint
    {
        /// <summary>
        /// No flags exist
        /// </summary>
        None = 0x0000000,
        /// <summary>
        /// Exclude the file class.
        /// </summary>
        Exclude = 0x00000001,
        /// <summary>
        /// Show file classes, such as folders, that aren't associated with a file name extension.
        /// </summary>
        Show = 0x00000002,
        /// <summary>
        /// The file class has a file name extension.
        /// </summary>
        HasExtension = 0x00000004,
        /// <summary>
        /// The registry entries associated with this file class cannot be edited. New entries cannot be added and existing entries cannot be modified or deleted.
        /// </summary>
        NoEdit = 0x00000008,
        /// <summary>
        /// The registry entries associated with this file class cannot be deleted.
        /// </summary>
        NoRemove = 0x00000010,
        /// <summary>
        /// No new verbs can be added to the file class.
        /// </summary>
        NoNewVerb = 0x00000020,
        /// <summary>
        /// Canonical verbs such as open and print cannot be modified or deleted.
        /// </summary>
        NoEditVerb = 0x00000040,
        /// <summary>
        /// Canonical verbs such as open and print cannot be deleted.
        /// </summary>
        NoRemoveVerb = 0x00000080,
        /// <summary>
        /// The description of the file class cannot be modified or deleted.
        /// </summary>
        NoEditDesc = 0x00000100,
        /// <summary>
        /// The icon assigned to the file class cannot be modified or deleted.
        /// </summary>
        NoEditIcon = 0x00000200,
        /// <summary>
        /// The default verb cannot be modified.
        /// </summary>
        NoEditDflt = 0x00000400,
        /// <summary>
        /// The commands associated with verbs cannot be modified.
        /// </summary>
        NoEditVerbCmd = 0x00000800,
        /// <summary>
        /// Verbs cannot be modified or deleted.
        /// </summary>
        NoEditVerbExe = 0x00001000,
        /// <summary>
        /// The Dynamic Data Exchange (DDE)-related entries cannot be modified or deleted.
        /// </summary>
        NoDDE = 0x00002000,
        /// <summary>
        /// The content-type and default-extension entries cannot be modified or deleted.
        /// </summary>
        NoEditMIME = 0x00008000,
        /// <summary>
        /// The file class's open verb can be safely invoked for downloaded files. Note that this flag may create a security risk, because downloaded files could contain malicious content. To reduce this risk, consider methods to scan downloaded files before opening.
        /// </summary>
        OpenIsSafe = 0x00010000,
        /// <summary>
        /// Do not allow the Never ask me check box to be enabled. The user can override this attribute through the File Type dialog box. This flag also affects ShellExecute, download dialogs, and any application making use of the AssocIsDangerous function.
        /// </summary>
        AlwaysUnsafe = 0x00020000,
        /// <summary>
        /// Always show the file class's file name extension, even if the user has selected the Hide Extensions option.
        /// </summary>
        AlwaysShowExtension = 0x00040000,
        /// <summary>
        /// Don't add members of this file class to the Recent Documents folder.
        /// </summary>
        NoRecentDocuments = 0x00100000
    }

    /// <summary>
    /// Provides instance of ProgramaticID that can be referenced by multiple different extensions.
    /// </summary>
    public class ProgramAssociationInfo
    {
        private RegistryWrapper registryWrapper = new RegistryWrapper();

        /// <summary>
        /// Actual name of Programmatic Identifier
        /// </summary>
        protected string progId;

        /// <summary>
        /// Gets or sets a value that determines if the file's extension will always be displayed.
        /// </summary>
        public bool AlwaysShowExtension
        {
            get { return GetAlwaysShowExt(); }
            set { SetAlwaysShowExt(value); }
        }


        /// <summary>
        /// Gets or sets a value that determines what the friendly name of the file is.
        /// </summary>
        public string Description
        {
            get { return GetDescription(); }
            set { SetDescription(value); }
        }

        /// <summary>
        /// Gets or sets a value that determines numerous shell options for extension as well as limitations on how extension properties can be edited by programs that honor <see cref="EditFlags"/>
        /// </summary>
        public EditFlags EditFlags
        {
            get { return GetEditFlags(); }
            set { SetEditFlags(value); }
        }

        /// <summary>
        /// Gets or sets a value that determines the default icon for the file type.
        /// </summary>
        public ProgramIcon DefaultIcon
        {
            get { return GetDefaultIcon(); }
            set { SetDefaultIcon(value); }
        }

        /// <summary>
        /// Gets or sets an array of <see cref="ProgramVerb"/> that define the verbs supported by this ProgID
        /// </summary>
        public ProgramVerb[] Verbs
        {
            get { return GetVerbs(); }
            set { SetVerbs(value); }
        }

        /// <summary>
        /// Gets a value that is the name of the Programatic Identifier
        /// </summary>
        public string ProgID
        {
            get { return progId; }
        }

        /// <summary>
        /// Gets a value that determines of a registry key exists with this Programatic Identifier
        /// </summary>
        public bool Exists
        {
            get
            {
                RegistryKey root = Registry.ClassesRoot;
                try
                {
                    if (progId == string.Empty)
                        return false;

                    RegistryKey key = root.OpenSubKey(progId);

                    if (key == null)
                        return false;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                    return false;
                }

                return true;
            }
        }

        #region Public Functions - Creators

        /// <summary>
        /// Creates program id within registry.
        /// </summary>
        public void Create()
        {
            if (Exists)
                return;

            RegistryKey root = Registry.ClassesRoot;

            root.CreateSubKey(progId);
        }

        /// <summary>
        /// Creates actual Programmatic Identifier key in registry that is used by other extensions.
        /// </summary>
        /// <param name="verb">Single <see cref="ProgramVerb"/> that contains supported verb.</param>
        /// <returns><see cref="ProgramAssociationInfo"/> instance referring to specified extension.</returns>
        public ProgramAssociationInfo Create(ProgramVerb verb)
        {
            return Create(string.Empty, EditFlags.None, new ProgramVerb[] { verb });
        }

        /// <summary>
        /// Creates actual Programmatic Identifier key in registry that is used by other extensions.
        /// </summary>
        /// <param name="verbs">Array of <see cref="ProgramVerb"/> that contains supported verbs.</param>
        /// <returns><see cref="ProgramAssociationInfo"/> instance referring to specified extension.</returns>
        public ProgramAssociationInfo Create(ProgramVerb[] verbs)
        {
            return Create(string.Empty, EditFlags.None, verbs);
        }

        /// <summary>
        /// Creates actual Programmatic Identifier key in registry that is used by other extensions.
        /// </summary>
        /// <param name="description">Friendly description of file type.</param>
        /// <param name="verb">Single <see cref="ProgramVerb"/> that contains supported verbs.</param>
        /// <returns><see cref="ProgramAssociationInfo"/> instance referring to specified extension.</returns>
        public ProgramAssociationInfo Create(string description, ProgramVerb verb)
        {
            return Create(description, EditFlags.None, new ProgramVerb[] { verb });
        }

        /// <summary>
        /// Creates actual Programmatic Identifier key in registry that is used by other extensions.
        /// </summary>
        /// <param name="description">Friendly description of file type.</param>
        /// <param name="verbs">Array of <see cref="ProgramVerb"/> that contains supported verbs.</param>
        /// <returns><see cref="ProgramAssociationInfo"/> instance referring to specified extension.</returns>
        public ProgramAssociationInfo Create(string description, ProgramVerb[] verbs)
        {
            return Create(description, EditFlags.None, verbs);
        }

        /// <summary>
        /// Creates actual Programmatic Identifier key in registry that is used by other extensions.
        /// </summary>
        /// <param name="description">Friendly description of file type.</param>
        /// <param name="editFlags"><see cref="EditFlags"/> for program file type.</param>
        /// <param name="verb">Single <see cref="ProgramVerb"/> that contains supported verbs.</param>
        /// <returns><see cref="ProgramAssociationInfo"/> instance referring to specified extension.</returns>
        public ProgramAssociationInfo Create(string description, EditFlags editFlags, ProgramVerb verb)
        {
            return Create(description, editFlags, new ProgramVerb[] { verb });
        }

        /// <summary>
        /// Creates actual Programmatic Identifier key in registry that is used by other extensions.
        /// </summary>
        /// <param name="description">Friendly description of file type.</param>
        /// <param name="editFlags"><see cref="EditFlags"/> for program file type.</param>
        /// <param name="verbs">Array of <see cref="ProgramVerb"/> that contains supported verbs.</param>
        /// <returns><see cref="ProgramAssociationInfo"/> instance referring to specified extension.</returns>
        public ProgramAssociationInfo Create(string description, EditFlags editFlags, ProgramVerb[] verbs)
        {
            if (Exists)
            {
                Delete();
            }

            Create();

            if (description != string.Empty)
                Description = description;

            if (editFlags != EditFlags.None)
                EditFlags = editFlags;

            Verbs = verbs;

            return this;
        }
        #endregion

        #region Private Functions - Property backend

        /// <summary>
        /// Gets a value that determines if the file's extension will always be displayed.
        /// </summary>
        /// <returns>Value that specifies if the file's extension is always displayed.</returns>
        protected bool GetAlwaysShowExt()
        {
            if (!Exists)
                throw new Exception("Extension does not exist");

            RegistryKey root = Registry.ClassesRoot;

            RegistryKey key = root.OpenSubKey(progId);

            object o = key.GetValue("AlwaysShowExt", "ThisValueShouldNotExist");
            if (o.ToString() == "ThisValueShouldNotExist")
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets a value that determines if the file's extension will always be shown.
        /// </summary>
        /// <param name="value">Value that specifies if the file's extension should be always displayed.</param>
        protected void SetAlwaysShowExt(bool value)
        {
            if (!Exists)
                throw new Exception("Extension does not exist");

            if (value)
            {
                registryWrapper.Write(progId, "AlwaysShowExt", string.Empty);
            }
            else
            {
                registryWrapper.Delete(progId, "AlwaysShowExt");
            }

            ShellNotification.NotifyOfChange();
        }

        /// <summary>
        /// Gets a value that determines what the friendly name of the file is.
        /// </summary>
        /// <returns>Friendly description of file type.</returns>
        protected string GetDescription()
        {
            if (!Exists)
                throw new Exception("Extension does not exist");

            object val = registryWrapper.Read(progId, string.Empty);

            if (val == null)
                return string.Empty;

            return val.ToString();
        }

        /// <summary>
        /// Sets a value that determines what the friendly name of the file is.
        /// </summary>
        /// <param name="description">Friendly description of file type.</param>
        protected void SetDescription(string description)
        {
            if (!Exists)
                throw new Exception("Extension does not exist");

            registryWrapper.Write(progId, string.Empty, description);

            ShellNotification.NotifyOfChange();
        }


        /// <summary>
        /// Gets a value that determines numerous shell options for extension as well as limitations on how extension properties can be edited by programs that honor <see cref="EditFlags"/>
        /// </summary>
        /// <returns><see cref="EditFlags"/> for program file type.</returns>
        protected EditFlags GetEditFlags()
        {
            if (!Exists)
                throw new Exception("Extension does not exist");

            object val = registryWrapper.Read(progId, "EditFlags");

            if (val == null)
            {
                return EditFlags.None;
            }
            if (val is byte[])
            {
                int num;
                if (TryGetInt(val as byte[], out num))
                {
                    val = num;
                }
                else
                {
                    return EditFlags.None;
                }
            }

            try
            {
                return (EditFlags)Convert.ToUInt32(val);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return EditFlags.None;
        }

        /// <summary>
        /// Sets a value that determines numerous shell options for extension as well as limitations on how extension properties can be edited by programs that honor <see cref="EditFlags"/>
        /// </summary>
        /// <param name="flags"><see cref="EditFlags"/> for program file type.</param>
        protected void SetEditFlags(EditFlags flags)
        {
            if (!Exists)
                throw new Exception("Extension does not exist");

            //registryWrapper.Write(info.progId, "EditFlags", (uint)flags);
            registryWrapper.Write(progId, "EditFlags", flags);

            ShellNotification.NotifyOfChange();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected ProgramIcon GetDefaultIcon()
        {
            if (!Exists)
                throw new Exception("Extension does not exist");

            object val = registryWrapper.Read(progId + "\\DefaultIcon", "");

            if (val == null)
                return ProgramIcon.None;

            return ProgramIcon.Parse(val.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icon"></param>
        protected void SetDefaultIcon(ProgramIcon icon)
        {
            if (!Exists)
                throw new Exception("Extension does not exist");

            if (icon != ProgramIcon.None)
            {
                registryWrapper.Write(progId, "DefaultIcon", icon.ToString());

                ShellNotification.NotifyOfChange();
            }
        }

        /// <summary>
        /// Gets an array of <see cref="ProgramVerb"/> that define the verbs supported by this ProgID.
        /// </summary>
        /// <returns>Array of <see cref="ProgramVerb"/> that contains supported verbs.</returns>
        protected ProgramVerb[] GetVerbs()
        {
            if (!Exists)
                throw new Exception("Extension does not exist");

            RegistryKey root = Registry.ClassesRoot;

            RegistryKey key = root.OpenSubKey(progId);
            List<ProgramVerb> verbs = new List<ProgramVerb>();

            key = key.OpenSubKey("shell", false);
            if (key != null)
            {
                string[] keyNames = key.GetSubKeyNames();

                foreach (string s in keyNames)
                {
                    RegistryKey verb = key.OpenSubKey(s);
                    if (verb == null)
                        continue;

                    verb = verb.OpenSubKey("command");
                    if (verb == null)
                        continue;

                    string command = (string)verb.GetValue("", "", RegistryValueOptions.DoNotExpandEnvironmentNames);

                    verbs.Add(new ProgramVerb(s, command));

                }

                key.Close();
            }

            root.Close();

            return verbs.ToArray();
        }

        /// <summary>
        /// Sets an array of <see cref="ProgramVerb"/> that define the verbs supported by this ProgID
        /// </summary>
        /// <param name="verbs">Array of <see cref="ProgramVerb"/> that contains verbs to be set.</param>
        protected void SetVerbs(ProgramVerb[] verbs)
        {
            if (!Exists)
                throw new Exception("Extension does not exist");

            RegistryKey root = Registry.ClassesRoot;

            RegistryKey key = root.OpenSubKey(progId, true);

            RegistryKey tmpKey = key.OpenSubKey("shell", true);

            if (tmpKey != null)
            {
                key.DeleteSubKeyTree("shell");
            }

            tmpKey = key.CreateSubKey("shell");

            foreach (ProgramVerb verb in verbs)
            {
                RegistryKey newVerb = tmpKey.CreateSubKey(verb.Name.ToLower());
                RegistryKey command = newVerb.CreateSubKey("command");

                command.SetValue(string.Empty, verb.Command, RegistryValueKind.ExpandString);

                command.Close();
                newVerb.Close();
            }

            ShellNotification.NotifyOfChange();
        }


        #endregion

        #region Add/Remove Single Verb

        /// <summary>
        /// Adds single <see cref="ProgramVerb"/> that define the verb supported by this ProgID.
        /// </summary>
        /// <param name="verb">Single <see cref="ProgramVerb"/> that contains supported verb.</param>
        protected void AddVerbInternal(ProgramVerb verb)
        {
            RegistryKey root = Registry.ClassesRoot;

            RegistryKey key = root.OpenSubKey(progId).OpenSubKey("shell", true);

            if (key == null)
            {
                key = root.OpenSubKey(progId, true).CreateSubKey("shell");
            }

            RegistryKey tmpkey = key.OpenSubKey(verb.Name, true);
            if (tmpkey == null)
            {
                tmpkey = key.CreateSubKey(verb.Name);
            }
            key = tmpkey;

            tmpkey = key.OpenSubKey("command", true);
            if (tmpkey == null)
            {
                tmpkey = key.CreateSubKey("command");
            }

            tmpkey.SetValue(string.Empty, verb.Command, RegistryValueKind.ExpandString);


            tmpkey.Close();
            key.Close();
            root.Close();

            ShellNotification.NotifyOfChange();
        }

        /// <summary>
        /// Removes single <see cref="ProgramVerb"/> that define the verb supported by this ProgID.
        /// </summary>
        /// <param name="name">Name of verb to remove</param>
        protected void RemoveVerbInternal(string name)
        {
            RegistryKey root = Registry.ClassesRoot;

            RegistryKey key = root.OpenSubKey(progId);

            key = key.OpenSubKey("shell", true);

            if (key == null)
                throw new RegistryException("Shell key not found");

            string[] subkeynames = key.GetSubKeyNames();
            foreach (string s in subkeynames)
            {
                if (s == name)
                {
                    key.DeleteSubKeyTree(name);
                    break;
                }
            }

            key.Close();
            root.Close();

            ShellNotification.NotifyOfChange();
        }
        #endregion



        /// <summary>
        /// Deletes the current prog id.
        /// </summary>
        public void Delete()
        {
            if (!Exists)
            {
                throw new Exception("Key not found.");
            }

            RegistryKey root = Registry.ClassesRoot;

            root.DeleteSubKeyTree(progId);
        }

        /// <summary>
        /// Adds single <see cref="ProgramVerb"/> that define the verb supported by this ProgID.
        /// </summary>
        /// <param name="verb">Single <see cref="ProgramVerb"/> that contains supported verb.</param>
        public void AddVerb(ProgramVerb verb)
        {
            AddVerbInternal(verb);
        }

        /// <summary>
        /// Removes single <see cref="ProgramVerb"/> that define the verb supported by this ProgID.
        /// </summary>
        /// <param name="verb">Single <see cref="ProgramVerb"/> that contains supported verb.</param>
        public void RemoveVerb(ProgramVerb verb)
        {
            if (verb == null)
                throw new NullReferenceException();

            RemoveVerb(verb.Name);
        }

        /// <summary>
        /// Removes single <see cref="ProgramVerb"/> that define the verb supported by this ProgID.
        /// </summary>
        /// <param name="name">Name of verb to remove.</param>
        public void RemoveVerb(string name)
        {
            RemoveVerbInternal(name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramAssociationInfo"/> class, which acts as a wrapper for a Programmatic Identifier within the registry.
        /// </summary>
        /// <param name="progId">Name of program id to interface with.</param>
        public ProgramAssociationInfo(string progId)
        {
            this.progId = progId;
        }

        /// <summary>
        /// Attempts to convert the value within the input byte array into an integer.
        /// </summary>
        /// <param name="arr">Byte array containing number.</param>
        /// <param name="val">Converted integer if successful.</param>
        /// <returns>True on success, false on failure.</returns>
        bool TryGetInt(byte[] arr, out int val)
        {
            try
            {
                if (arr.Length == 0)
                {
                    val = -1;
                    return false;
                }
                else if (arr.Length == 1)
                {
                    val = arr[0];
                }
                else
                {
                    val = BitConverter.ToInt32(arr, 0);
                }

                return true;
            }
            catch
            {

            }
            val = 0;
            return false;
        }
    }
}