using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageGlass.Library.FileAssociations
{
    public class ExecApplication
    {
        public ExecApplication(string appPath)
        {
            Path = appPath.Trim();
        }

        public readonly string Path;

        /// <summary>
        /// Gets a value indicating whether this Executable Application is an .exe, and that it exists.
        /// </summary>
        public bool IsValid
        {
            get
            {
                FileInfo getInfo = new FileInfo(Path);
                if (getInfo.Exists)
                    return true;
                else
                    return false;
            }
        }
    }
}
