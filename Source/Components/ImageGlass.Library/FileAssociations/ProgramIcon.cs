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

namespace ImageGlass.Library.FileAssociations
{
    /// <summary>
    /// 
    /// </summary>
    public class ProgramIcon
    {
        /// <summary>
        /// Represents and empty or nonexistent Program Icon
        /// </summary>
        public static readonly ProgramIcon None = new ProgramIcon();

        private string path;
        private int index;

        /// <summary>
        /// Gets or sets value that specifies icons index within a file.
        /// </summary>
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        /// <summary>
        /// Gets or sets a value that specifies the file containing the icon.
        /// </summary>
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        /// <summary>
        /// Creates instance of ProgramIcon.
        /// </summary>
        /// <param name="path">Filename of file containing icon.</param>
        /// <param name="index">Index of icon within the file.</param>
        public ProgramIcon(string path, int index)
        {
            this.path = path;
            this.index = index;
        }

        /// <summary>
        /// Creates instance of ProgramIcon.
        /// </summary>
        /// <param name="path">Filename of file containing icon.</param>
        public ProgramIcon(string path)
        {
            this.path = path;
            this.index = 0;
        }

        /// <summary>
        /// Creates instance of ProgramIcon.
        /// </summary>
        public ProgramIcon()
        {
            this.path = string.Empty;
            this.index = 0;
        }

        /// <summary>
        /// Returns string representation of current ProgramIcon.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return path + "," + index.ToString();
        }

        /// <summary>
        /// Parses string to create and instance of ProgramIcon.
        /// </summary>
        /// <param name="regString">String specifying file path. Icon can be included as well.</param>
        /// <returns>ProgramIcon based on input string.</returns>
        public static ProgramIcon Parse(string regString)
        {
            if (regString == string.Empty)
                return new ProgramIcon("");

            if (regString.StartsWith("\"") && regString.EndsWith("\""))
            {
                if (regString.Length > 3)
                    regString = regString.Substring(1, regString.Length - 2);
            }

            string path;
            int index = 0;

            int commaPos = regString.IndexOf(",");

            if (commaPos == -1)
            {
                commaPos = regString.Length;
            }
            else
            {
                index = int.Parse(regString.Substring(commaPos + 1));
            }

            path = regString.Substring(0, commaPos);


            return new ProgramIcon(path, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="rv"></param>
        /// <returns></returns>
        public static bool operator ==(ProgramIcon lv, ProgramIcon rv)
        {
            if (ReferenceEquals(lv, null) && ReferenceEquals(rv, null))
                return true;
            else if (ReferenceEquals(lv, null) || ReferenceEquals(rv, null))
                return false;
            else
            {
                if (lv.path == rv.path
                   && lv.index == rv.index)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="rv"></param>
        /// <returns></returns>
        public static bool operator !=(ProgramIcon lv, ProgramIcon rv)
        {
            return !(lv == rv);
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current System.Object.</param>
        /// <returns>true if the specified System.Object is equal to the current System.Object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            //Exists only to avoid compiler warning
            return this == (obj as ProgramIcon);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current System.Object.</returns>
        public override int GetHashCode()
        {
            //Exists only to avoid compiler warning
            return base.GetHashCode();
        }


    }
}