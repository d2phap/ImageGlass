using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImageGlass.Library
{
    public static class Helper
    {
        /// <summary>
        /// Check if the given form's location is visible on screen
        /// </summary>
        /// <param name="location">The location of form to check</param>
        /// <returns></returns>
        public static bool IsOnScreen(Point location)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                if (screen.WorkingArea.Contains(location))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
