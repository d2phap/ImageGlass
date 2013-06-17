using System;
using System.Collections;
using System.IO;

namespace ImageGlass.Library.Comparer
{
    public class FileLogicalComparer
    {
        private ArrayList _files = null;

        public ArrayList Files
        {
            get { return _files; }
            set { _files = value; }
        }

        #region Local Functions
        public void AddFile(string file)
        {
            if (file == null) return;
            if (_files == null) _files = new ArrayList();
            _files.Add(new DictionaryEntry(Path.GetFileName(file), file));
        }

        
        public void AddFiles(string[] f)
        {
            if (f == null) return;
            for (int i = 0; i < f.Length; i++)
            {
                AddFile(f[i]);
            }
        }

        public ArrayList GetSorted()
        {
            _files.Sort(new DictionaryEntryComparer(new ImageGlass.Library.Comparer.NumericComparer()));
            return _files;
        }
        #endregion


        /// <summary>
        /// Sort an string array
        /// </summary>
        /// <param name="stringArray">String array</param>
        /// <returns></returns>
        public static string[] Sort(string[] stringArray)
        {
            if (stringArray == null) return null;

            FileLogicalComparer fc = new FileLogicalComparer();
            fc.AddFiles(stringArray);
            ArrayList ds = fc.GetSorted();

            if (ds == null) return stringArray;

            for (int i = 0; i < ds.Count; i++)
            {
                stringArray[i] = (string)((DictionaryEntry)ds[i]).Value;
            }

            return stringArray;
        }

    }



    

}