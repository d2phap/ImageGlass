using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageGlass
{
    public class ToolFormManager
    {
        // The list of ToolForms to manage
        private static List<ToolForm> _formList = new List<ToolForm>();


        /// <summary>
        /// Track a new ToolForm with this manager
        /// </summary>
        /// <param name="client"></param>
        public void Add(ToolForm client)
        {
            _formList.Add(client);
        }


        // How many pixels to leave between snapped forms (top/bottom)
        private static int MARGIN = 2;


        /// <summary>
        /// Snap the provided Toolform to the "nearest" Toolform in the list
        /// </summary>
        /// <param name="formToSnap"></param>
        public void SnapToNearest(ToolForm formToSnap)
        {
            // NOTE: merely finds the "other" form
            // TODO: when more than 2 toolforms possible, this needs to find the "nearest"
            var destForm = _formList.FirstOrDefault(x => x != formToSnap);
            if (destForm == null)
                return;

            // snap to top/bottom as appropriate
            if (destForm.Top > formToSnap.Bottom - MARGIN)
            {
                // snapping form ABOVE other form
                formToSnap.Top = destForm.Top - formToSnap.Height - MARGIN;
            }
            else
            {
                // snapping form BELOW or OVERLAP other form
                if (destForm.Bottom+MARGIN < formToSnap.Top ||
                    formToSnap.Top < destForm.Bottom + MARGIN)
                    formToSnap.Top = destForm.Bottom + MARGIN;
            }

            // snap to left/right edge as appropriate
            if (destForm.Left < formToSnap.Left)
            {
                // snapping form to the RIGHT or OVERLAP other form
                formToSnap.Left = destForm.Right - formToSnap.Width;
            }
            else
            {
                // snapping form to the LEFT of other form
                formToSnap.Left = destForm.Left;
            }
        }


        public void MoveSnappedTools(Point lastLoc, Point currLoc)
        {
            // Move all toolforms together (preserving relative positions)
            // TODO: if more than 2 toolforms possible, may wish to mark a set as 'snapped' instead of ALL

            foreach (var toolForm in _formList)
            {
                Point delta = new Point(toolForm.Location.X - lastLoc.X, toolForm.Location.Y - lastLoc.Y);
                toolForm.Location = new Point(delta.X + currLoc.X,
                    delta.Y + currLoc.Y);
                toolForm.Update();
            }
        }
    }
}
