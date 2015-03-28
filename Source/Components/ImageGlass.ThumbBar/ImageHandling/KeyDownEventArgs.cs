/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2014 DUONG DIEU PHAP
Project homepage: http://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ImageGlass.ThumbBar.ImageHandling
{
    public class KeyDownEventArgs : EventArgs
    {
        public KeyDownEventArgs(KeyEventArgs ev)
        {
            this._keyEv = ev;
        }

        private ThumbnailBox _tb;
        private int _index = -1;
        private KeyEventArgs _keyEv;

        public KeyEventArgs KeyEvent
        {
            get { return _keyEv; }
            set { _keyEv = value; }
        }

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public ThumbnailBox ThumbnailItem
        {
            get { return _tb; }
            set { _tb = value; }
        }
    }
}
