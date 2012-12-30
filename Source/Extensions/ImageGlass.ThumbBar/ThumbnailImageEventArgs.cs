using System;
using System.Collections.Generic;
using System.Text;

namespace ImageGlass.ThumbBar
{
    public class ThumbnailImageEventArgs : EventArgs
    {
        public ThumbnailImageEventArgs(int size)
        {
            this.Size = size;
        }

        public int Size;
    }

    public delegate void ThumbnailImageEventHandler(object sender, ThumbnailImageEventArgs e);
}
