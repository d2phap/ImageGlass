using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageGlass.Library.FileAssociations
{
    public class ProgramIcon
    {
        public ProgramIcon(string iconPath)
        {
            IconPath = iconPath.Trim();
        }

        public readonly string IconPath;

        public bool IsValid
        {
            get
            {
                FileInfo getInfo = new FileInfo(IconPath);

                if (getInfo.Exists && getInfo.Extension == ".ico")
                    return true;
                else
                    return false;
            }
        }
    }
}
