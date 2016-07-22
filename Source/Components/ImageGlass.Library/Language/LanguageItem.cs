using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageGlass.Library
{
    public enum LanguageItemState
    {
        Inactive = 0,
        Active = 1
    }

    public class LanguageItem<K, V> : Dictionary<K, V>
    {
        public Version Version { get; set; }
        public LanguageItemState State { get; set; }
        public string Remarks { get; set; }

        public LanguageItem()
        {
            this.Version = new Version("3.5.0.0");
            this.State = LanguageItemState.Inactive;
            this.Remarks = string.Empty;
        }
    }
    
}
