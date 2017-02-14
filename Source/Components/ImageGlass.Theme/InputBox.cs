/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
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
using System.Drawing;

namespace ImageGlass.Theme
{
    public class InputBox
    {
        private static string _message = string.Empty;

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
            return InputBox.ShowDiaLog(title, message, string.Empty, false);
        }

        /// <summary>
        /// Show input dialog box
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="message">Message</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns></returns>
        public static DialogResult ShowDiaLog(string title, string message, string defaultValue)
        {
            return InputBox.ShowDiaLog(title, message, defaultValue, false);
        }

        /// <summary>
        /// Show input dialog box
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="message">Message</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="isNumberOnly">Number input</param>
        /// <returns></returns>
        public static DialogResult ShowDiaLog(string title, string message, string defaultValue, bool isNumberOnly)
        {
            frmDialogBox f = new frmDialogBox(title, message);
            f.Title = title;
            f.IsNumberOnly = isNumberOnly;
            f.Content = defaultValue;

            if (f.ShowDialog() == DialogResult.OK)
            {
                //Save input data
                InputBox.Message = f.Content;
            }

            return f.DialogResult;
        }

    }
}
