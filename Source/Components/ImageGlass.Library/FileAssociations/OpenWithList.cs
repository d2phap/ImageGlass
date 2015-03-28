using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageGlass.Library.FileAssociations
{
    public class OpenWithList
    {
        public OpenWithList(string[] openWithPaths)
        {
            List<string> toReturn = new List<string>();
            FileInfo getInfo;

            foreach (string file in openWithPaths)
            {
                getInfo = new FileInfo(file);
                toReturn.Add(getInfo.Name);
            }

            List = toReturn.ToArray();
        }

        public readonly string[] List;
    }
}
