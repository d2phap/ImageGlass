/*
 * imaeg - generic image utility in C#
 * Copyright (C) 2010  ed <tripflag@gmail.com>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License v2
 * (version 2) as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, refer to the following URL:
 * http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ImageGlass.Core
{
    public class InputBox
    {
        private static bool _isNumberOnly = false;
        private static string _message = "";
        
        /// <summary>
        /// Get, set input style
        /// </summary>
        public static bool IsNumberOnly
        {
            get { return _isNumberOnly; }
            set { _isNumberOnly = value; }
        }

        /// <summary>
        /// Get, set the message user inputs
        /// </summary>
        public static string Message
        {
            get { return InputBox._message; }
            set { _message = value; }
        }

        /// <summary>
        /// Show input dialog box
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="message">Message</param>
        /// <returns></returns>
        public static DialogResult ShowDiaLog(string title, string message)
        {
            frmDialogBox f = new frmDialogBox(title, message);
            f.Title = title;
            f.IsNumberOnly = false;
            f.ShowDialog();

            //Lưu nội dung vừa nhập
            InputBox.Message = f.Content;

            return f.DialogResult;
        }

        public static DialogResult ShowDiaLog(string title, string message, string defaultValue)
        {
            frmDialogBox f = new frmDialogBox(title, message);
            f.Title = title;
            f.IsNumberOnly = false;
            f.Content = defaultValue;
            f.ShowDialog();

            //Lưu nội dung vừa nhập
            InputBox.Message = f.Content;

            return f.DialogResult;
        }

        public static DialogResult ShowDiaLog(string title, string message, string defaultValue, bool isNumberOnly)
        {
            frmDialogBox f = new frmDialogBox(title, message);
            f.Title = title;
            f.IsNumberOnly = isNumberOnly;
            f.Content = defaultValue;
            f.ShowDialog();

            //Lưu nội dung vừa nhập
            InputBox.Message = f.Content;

            return f.DialogResult;
        }

    }
}
