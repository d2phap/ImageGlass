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
        public static string Derp(string query, string std)
        {
            InputBox ib = new InputBox(query, std);
            ib.ShowDialogue();
            return ib.ret;
        }
        Form form;
        Label label;
        TextBox textbox;
        public string ret;
        public bool closed;
        public InputBox(string query, string std)
        {
            form = null;
            label = null;
            textbox = null;
            closed = false;
            ret = null;

            form = new Form();
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            Label derp = new Label(); derp.Dock = DockStyle.Fill;
            form.Controls.Add(derp); Size pad = new Size(
                form.Width - derp.Width,
                form.Height - derp.Height);
            derp.Dispose();

            label = new Label();
            label.Text = query;
            label.AutoSize = true;
            label.Location = new Point(16, 16);
            label.Visible = true;
            form.Controls.Add(label);

            textbox = new TextBox();
            textbox.Text = std;
            textbox.Width = Math.Max(
                480, label.Width);
            textbox.Location = new Point(
                label.Location.X,
                label.Location.Y +
                label.Height + 16);
            textbox.Visible = true;
            form.Controls.Add(textbox);

            form.Width = textbox.Width + 32 + pad.Width;
            form.Height = textbox.Top + textbox.Height + 8 + pad.Height;
            textbox.KeyDown += new KeyEventHandler(textbox_KeyDown);
        }

        public void ShowDialogue()
        {
            form.ShowDialog();
            textbox.Focus();
        }

        void textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ret = textbox.Text;
                form.Close();
            }
            if (e.KeyCode == Keys.Escape)
            {
                form.Close();
            }
        }
    }
}
