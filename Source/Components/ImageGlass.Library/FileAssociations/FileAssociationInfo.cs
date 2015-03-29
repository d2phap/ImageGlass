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
using Microsoft.Win32;

using System.Xml.Serialization;


namespace ImageGlass.Library.FileAssociations
{

    #region Public Enums
    /// <summary>
    /// Broad categories of system recognized file format types.
    /// </summary>
    public enum PerceivedTypes
    {
        /// <summary>
        /// No 
        /// </summary>
        None,
        /// <summary>
        /// Image file
        /// </summary>
        Image,
        /// <summary>
        /// Text file
        /// </summary>
        Text,
        /// <summary>
        /// Audio file
        /// </summary>
        Audio,
        /// <summary>
        /// Video file
        /// </summary>
        Video,
        /// <summary>
        /// Compressed file
        /// </summary>
        Compressed,
        /// <summary>
        /// System file
        /// </summary>
        System,
    }
    #endregion

    /// <summary>
    /// Provides instance methods for the creation, modification, and deletion of file extension associations in the Windows registry.
    /// </summary>
    public class FileAssociationInfo
    {
        private RegistryWrapper registryWrapper = new RegistryWrapper();

        /// <summary>
        /// Gets array containing known file extensions from HKEY_CLASSES_ROOT.
        /// </summary>
        /// <returns>String array containing extensions.</returns>
        public static string[] GetExtensions()
        {
            RegistryKey root = Registry.ClassesRoot;
            List<string> extensionList = new List<string>();

            string[] subKeys = root.GetSubKeyNames();

            foreach (string subKey in subKeys)
            {
                //TODO: Consider removing dot?
                if (subKey.StartsWith("."))
                {
                    extensionList.Add(subKey);
                }
            }
            return extensionList.ToArray(); ;
        }


        private string extension;

        /// <summary>
        /// Gets or sets a value that determines the MIME type of the file.
        /// </summary>
        public string ContentType
        {
            get { return GetContentType(this); }
            set { SetContentType(this, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the extension exists.
        /// </summary>
        public bool Exists
        {
            get
            {
                RegistryKey root = Registry.ClassesRoot;
                try
                {
                    RegistryKey key = root.OpenSubKey(extension);

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

        /// <summary>
        /// Gets the name of the extension.
        /// </summary>
        public string Extension
        {
            get { return extension; }
            set { extension = value; }
        }

        /// <summary>
        /// Gets or sets array of containing program file names which should be displayed in the Open With List.
        /// </summary>
        /// <example>notepad.exe, wordpad.exe, othertexteditor.exe</example>
        public string[] OpenWithList
        {
            get { return GetOpenWithList(this); }
            set { SetOpenWithList(this, value); }
        }

        /// <summary>
        /// Gets or sets a value that determines the <see cref="PerceivedType"/> of the file.
        /// </summary>
        public PerceivedTypes PerceivedType
        {
            get { return GetPerceivedType(this); }
            set { SetPerceivedType(this, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates the filter component that is used to search for text within documents of this type.
        /// </summary>
        public Guid PersistentHandler
        {
            get { return GetPersistentHandler(this); }
            set { SetPersistentHandler(this, value); }
        }

        /// <summary>
        /// Gets or set a value that indicates the name of the associated application with the behavior to handle this extension.
        /// </summary>
        [XmlAttribute()]
        public string ProgID
        {
            get { return GetProgID(this); }
            set { SetProgID(this, value); }
        }

        /// <summary>
        /// Creates the extension key.
        /// </summary>
        public void Create()
        {
            Create(this);
        }

        /// <summary>
        /// Deletes the extension key.
        /// </summary>
        public void Delete()
        {
            Delete(this);
        }

        /// <summary>
        /// Verifies that given extension exists and is associated with given program id
        /// </summary>
        /// <param name="extension">Extension to be checked for.</param>
        /// <param name="progId">progId to be checked for.</param>
        /// <returns>True if association exists, false if it does not.</returns>
        public bool IsValid(string extension, string progId)
        {
            FileAssociationInfo fai = new FileAssociationInfo(extension);

            if (!fai.Exists)
                return false;

            if (progId != fai.ProgID)
                return false;

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAssociationInfo"/>FileAssociationInfo class, which acts as a wrapper for a file extension within the registry.
        /// </summary>
        /// <param name="extension">The dot prefixed extension.</param>
        /// <example>FileAssociationInfo(".mp3")
        /// FileAssociationInfo(".txt")
        /// FileAssociationInfo(".doc")</example>
        public FileAssociationInfo(string extension)
        {
            this.extension = extension;
        }

        #region Public Functions - Creators

        /// <summary>
        /// Creates actual extension association key in registry for the specified extension and supplied attributes.
        /// </summary>
        /// <param name="progId">Name of expected handling program.</param>
        /// <returns>FileAssociationInfo instance referring to specified extension.</returns>
        public FileAssociationInfo Create(string progId)
        {
            return Create(progId, PerceivedTypes.None, string.Empty, null);
        }

        /// <summary>
        /// Creates actual extension association key in registry for the specified extension and supplied attributes.
        /// </summary>
        /// <param name="progId">Name of expected handling program.</param>
        /// <param name="perceivedType"><see cref="PerceivedTypes"/>PerceivedType of file type.</param>
        /// <returns>FileAssociationInfo instance referring to specified extension.</returns>
        public FileAssociationInfo Create(string progId, PerceivedTypes perceivedType)
        {
            return Create(progId, perceivedType, string.Empty, null);
        }

        /// <summary>
        /// Creates actual extension association key in registry for the specified extension and supplied attributes.
        /// </summary>
        /// <param name="progId">Name of expected handling program.</param>
        /// <param name="perceivedType"><see cref="PerceivedTypes"/>PerceivedType of file type.</param>
        /// <param name="contentType">MIME type of file type.</param>
        /// <returns>FileAssociationInfo instance referring to specified extension.</returns>
        public FileAssociationInfo Create(string progId, PerceivedTypes perceivedType, string contentType)
        {
            return Create(progId, PerceivedTypes.None, contentType, null);
        }

        /// <summary>
        /// Creates actual extension association key in registry for the specified extension and supplied attributes.
        /// </summary>
        /// <param name="progId">Name of expected handling program.</param>
        /// <param name="perceivedType"><see cref="PerceivedTypes"/>PerceivedType of file type.</param>
        /// <param name="contentType">MIME type of file type.</param>
        /// <param name="openwithList"></param>
        /// <returns>FileAssociationInfo instance referring to specified extension.</returns>
        public FileAssociationInfo Create(string progId, PerceivedTypes perceivedType, string contentType, string[] openwithList)
        {
            FileAssociationInfo fai = new FileAssociationInfo(extension);

            if (fai.Exists)
            {
                fai.Delete();
            }

            fai.Create();
            fai.ProgID = progId;

            if (perceivedType != PerceivedTypes.None)
                fai.PerceivedType = perceivedType;

            if (contentType != string.Empty)
                fai.ContentType = contentType;

            if (openwithList != null)
                fai.OpenWithList = openwithList;

            return fai;
        }

        #endregion

        #region Private Functions - Property backend

        /// <summary>
        /// Gets array of containing program file names which should be displayed in the Open With List.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> that provides specifics of the extension to be changed.</param>
        /// <returns>Program file names</returns>
        protected string[] GetOpenWithList(FileAssociationInfo file)
        {
            if (!file.Exists)
                throw new Exception("Extension does not exist");

            RegistryKey root = Registry.ClassesRoot;

            RegistryKey key = root.OpenSubKey(file.extension);

            key = key.OpenSubKey("OpenWithList");

            if (key == null)
            {
                return new string[0];
            }

            return key.GetSubKeyNames();

        }

        /// <summary>
        /// Sets array of containing program file names which should be displayed in the Open With List.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> that provides specifics of the extension to be changed.</param>
        /// <param name="programList">Program file names</param>
        protected void SetOpenWithList(FileAssociationInfo file, string[] programList)
        {
            if (!file.Exists)
                throw new Exception("Extension does not exist");

            RegistryKey root = Registry.ClassesRoot;

            RegistryKey key = root.OpenSubKey(file.extension, true);

            RegistryKey tmpkey = key.OpenSubKey("OpenWithList", true);

            if (tmpkey != null)
            {
                key.DeleteSubKeyTree("OpenWithList");
            }

            key = key.CreateSubKey("OpenWithList");

            foreach (string s in programList)
            {
                key.CreateSubKey(s);
            }

            ShellNotification.NotifyOfChange();
        }

        /// <summary>
        /// Gets or value that determines the <see cref="PerceivedType"/>PerceivedType of the file.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> that provides specifics of the extension to be changed.</param>
        /// <returns><see cref="PerceivedTypes"/> that specifies Perceived Type of extension.</returns>
        protected PerceivedTypes GetPerceivedType(FileAssociationInfo file)
        {
            if (!file.Exists)
                throw new Exception("Extension does not exist");

            object val = registryWrapper.Read(file.extension, "PerceivedType");
            PerceivedTypes actualType = PerceivedTypes.None;

            if (val == null)
                return actualType;

            try
            {
                actualType = (PerceivedTypes)Enum.Parse(typeof(PerceivedTypes), val.ToString(), true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            return actualType;
        }

        /// <summary>
        /// Sets a value that determines the <see cref="PerceivedType"/>PerceivedType of the file.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> that provides specifics of the extension to be changed.</param>
        /// <param name="type"><see cref="PerceivedTypes"/> to be set that specifies Perceived Type of extension.</param>
        protected void SetPerceivedType(FileAssociationInfo file, PerceivedTypes type)
        {
            if (!file.Exists)
                throw new Exception("Extension does not exist");

            registryWrapper.Write(file.extension, "PerceivedType", type.ToString());

            ShellNotification.NotifyOfChange();
        }


        /// <summary>
        /// Gets a value that indicates the filter component that is used to search for text within documents of this type.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> that provides specifics of the extension to be changed.</param>
        /// <returns>Guid of filter component.</returns>
        protected Guid GetPersistentHandler(FileAssociationInfo file)
        {
            if (!file.Exists)
                throw new Exception("Extension does not exist");

            object val = registryWrapper.Read(file.extension + "\\PersistentHandler", string.Empty);

            if (val == null)
                return new Guid();
            else
                return new Guid(val.ToString());
        }

        /// <summary>
        /// Sets a value that indicates the filter component that is used to search for text within documents of this type.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> that provides specifics of the extension to be changed.</param>
        /// <param name="persistentHandler">Guid of filter component.</param>
        protected void SetPersistentHandler(FileAssociationInfo file, Guid persistentHandler)
        {
            if (!file.Exists)
                throw new Exception("Extension does not exist");

            if (persistentHandler == Guid.Empty)
                return;

            this.registryWrapper.Write(file.extension + "\\" + PersistentHandler, string.Empty, persistentHandler);

            ShellNotification.NotifyOfChange();
        }

        /// <summary>
        /// Gets a value that determines the MIME type of the file.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> that provides specifics of the extension to be changed.</param>
        /// <returns>MIME content type of extension.</returns>
        protected string GetContentType(FileAssociationInfo file)
        {
            if (!file.Exists)
                throw new Exception("Extension does not exist");

            object val = registryWrapper.Read(file.extension, "Content Type");

            if (val == null)
            {
                return string.Empty;
            }
            else
            {
                return val.ToString();
            }
        }

        /// <summary>
        /// Sets a value that determines the MIME type of the file.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> that provides specifics of the extension to be changed.</param>
        /// <param name="type">MIME content type of extension.</param>
        protected void SetContentType(FileAssociationInfo file, string type)
        {
            if (!file.Exists)
                throw new Exception("Extension does not exist");

            registryWrapper.Write(file.extension, "Content Type", type);

            ShellNotification.NotifyOfChange();
        }


        /// <summary>
        /// Gets a value that indicates the name of the associated application with the behavior to handle this extension.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> that provides specifics of the extension to be changed.</param>
        /// <returns>Associated Program ID of handling program.</returns>
        protected string GetProgID(FileAssociationInfo file)
        {
            if (!file.Exists)
                throw new Exception("Extension does not exist");

            object val = registryWrapper.Read(file.extension, string.Empty);

            if (val == null)
                return string.Empty;

            return val.ToString();
        }

        /// <summary>
        /// Set a value that indicates the name of the associated application with the behavior to handle this extension.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> that provides specifics of the extension to be changed.</param>
        /// <param name="progId">Associated Program ID of handling program.</param>
        protected void SetProgID(FileAssociationInfo file, string progId)
        {
            if (!file.Exists)
                throw new Exception("Extension does not exist");

            registryWrapper.Write(file.extension, string.Empty, progId);

            ShellNotification.NotifyOfChange();
        }

        #endregion

        /// <summary>
        /// Creates actual file extension entry in registry.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> instance that contains specifics on extension to be created.</param>
        protected void Create(FileAssociationInfo file)
        {
            if (file.Exists)
            {
                file.Delete();
            }

            RegistryKey root = Registry.ClassesRoot;

            root.CreateSubKey(file.extension);
        }

        /// <summary>
        /// Deletes actual file extension entry in registry.
        /// </summary>
        /// <param name="file"><see cref="FileAssociationInfo"/> instance that contains specifics on extension to be deleted.</param>
        protected void Delete(FileAssociationInfo file)
        {
            if (!file.Exists)
            {
                throw new Exception("Key not found.");
            }

            RegistryKey root = Registry.ClassesRoot;

            root.DeleteSubKeyTree(file.extension);
        }
    }
}