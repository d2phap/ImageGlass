using System;
using System.Collections;
using System.IO;

namespace ImageGlass.Library.Comparer
{
    public class DictionaryEntryComparer : IComparer
    {
        private IComparer nc = null;

        public DictionaryEntryComparer(IComparer nc)
        {
            if (nc == null) throw new Exception("Null IComparer");
            this.nc = nc;
        }

        public int Compare(object x, object y)
        {
            if ((x is DictionaryEntry) && (y is DictionaryEntry))
            {
                return nc.Compare(((DictionaryEntry)x).Key, ((DictionaryEntry)y).Key);
            }
            return -1;
        }
    }
}
