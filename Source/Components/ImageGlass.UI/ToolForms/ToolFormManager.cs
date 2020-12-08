/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageGlass.UI.ToolForms {
    /// <summary>
    /// Manager for multiple ToolForms
    /// </summary>
    public class ToolFormManager {
        /// <summary>
        /// The list of ToolForms to manage
        /// </summary>
        private readonly List<ToolForm> _formList = new();

        /// <summary>
        /// Add a new ToolForm to the manager
        /// </summary>
        /// <param name="client">A ToolForm</param>
        public void Add(ToolForm client) {
            _formList.Add(client);
        }

        /// <summary>
        /// How many pixels to leave between snapped forms (top/bottom)
        /// </summary>
        private const int MARGIN = 10;

        /// <summary>
        /// Snap the provided ToolForm to the "nearest" Toolform in the list
        /// </summary>
        /// <param name="formToSnap">Another ToolForm</param>
        public void SnapToNearest(ToolForm formToSnap) {
            // NOTE: merely finds the "other" form
            // TODO: when more than 2 toolforms possible, this needs to find the "nearest"
            var destForm = _formList.Find(x => x != formToSnap);
            if (destForm == null) {
                return;
            }

            // snap to top/bottom as appropriate
            if (destForm.Top > formToSnap.Bottom - MARGIN) {
                // snapping form ABOVE other form
                formToSnap.Top = destForm.Top - formToSnap.Height - MARGIN;
            }
            else {
                // snapping form BELOW or OVERLAP other form
                if (destForm.Bottom + MARGIN < formToSnap.Top ||
                    formToSnap.Top < destForm.Bottom + MARGIN) {
                    formToSnap.Top = destForm.Bottom + MARGIN;
                }
            }

            // snap to left/right edge as appropriate
            if (destForm.Left < formToSnap.Left) {
                // snapping form to the RIGHT or OVERLAP other form
                formToSnap.Left = destForm.Right - formToSnap.Width;
            }
            else {
                // snapping form to the LEFT of other form
                formToSnap.Left = destForm.Left;
            }
        }

        /// <summary>
        /// Move all ToolForms together (preserving relative positions)
        /// </summary>
        /// <param name="lastLoc">Last location</param>
        /// <param name="currLoc">Current location</param>
        public void MoveSnappedTools(Point lastLoc, Point currLoc) {
            // TODO: if more than 2 toolforms possible, may wish to mark a set as 'snapped' instead of ALL

            foreach (var toolForm in _formList) {
                var delta = new Point(toolForm.Location.X - lastLoc.X, toolForm.Location.Y - lastLoc.Y);
                toolForm.Location = new Point(delta.X + currLoc.X,
                    delta.Y + currLoc.Y);
                toolForm.Update();
            }
        }
    }
}
