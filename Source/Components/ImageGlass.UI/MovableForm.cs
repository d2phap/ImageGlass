using System.Drawing;
using System.Windows.Forms;

namespace ImageGlass.UI
{
    /// <summary>
    /// Make the frameless form movable when dragging itself or its controls
    /// </summary>
    public class MovableForm
    {
        private Form _form;
        private Point _lastMouseLocation;
        private bool _isMouseDown;
        private bool _isKeyDown = true;



        /// <summary>
        /// Manually enable / disable moving
        /// </summary>
        public bool IsAllowMoving { get; set; } = true;

        /// <summary>
        /// Gets, sets the mouse button press for moving
        /// </summary>
        public MouseButtons MouseButton { get; set; } = MouseButtons.Left;

        /// <summary>
        /// Gets, sets the Key press for moving
        /// </summary>
        public Keys Key { get; set; } = Keys.None;


        /// <summary>
        /// Initialize the MovableForm
        /// </summary>
        /// <param name="form">The form to make it movable</param>
        public MovableForm(Form form)
        {
            _form = form;
            _isKeyDown = this.Key == Keys.None;

            _form.KeyDown += this.Form_KeyDown;
            _form.KeyUp += this.Form_KeyUp;
        }



        /// <summary>
        /// Enable moving ability on this form
        /// </summary>
        public void Enable()
        {
            _form.MouseDown += Control_MouseDown;
            _form.MouseMove += Control_MouseMove;
            _form.MouseUp += Control_MouseUp;
        }

        /// <summary>
        /// Enable moving ability on the given controls
        /// </summary>
        /// <param name="controls"></param>
        public void Enable(params Control[] controls)
        {
            foreach (var item in controls)
            {
                item.MouseDown += Control_MouseDown;
                item.MouseMove += Control_MouseMove;
                item.MouseUp += Control_MouseUp;
            }
        }

        /// <summary>
        /// Disable moving ability on this form
        /// </summary>
        public void Disable()
        {
            _form.MouseDown -= Control_MouseDown;
            _form.MouseMove -= Control_MouseMove;
            _form.MouseUp -= Control_MouseUp;
        }


        /// <summary>
        /// Disable moving ability on the given controls
        /// </summary>
        /// <param name="controls"></param>
        public void Disable(params Control[] controls)
        {
            foreach (var item in controls)
            {
                item.MouseDown -= Control_MouseDown;
                item.MouseMove -= Control_MouseMove;
                item.MouseUp -= Control_MouseUp;
            }
        }


        #region Events: Frameless form moving

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.Key == Keys.None)
            {
                _isKeyDown = true;
            }
            else
            {
                _isKeyDown = e.KeyData == this.Key;
            }
        }

        private void Form_KeyUp(object sender, KeyEventArgs e)
        {
            _isKeyDown = this.Key == Keys.None;
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 1 && this.IsAllowMoving && _isKeyDown && e.Button == this.MouseButton)
                _isMouseDown = true;

            _lastMouseLocation = e.Location;
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return; // not moving windows, ignore

            _form.Location = new Point(
                _form.Location.X - _lastMouseLocation.X + e.X,
                _form.Location.Y - _lastMouseLocation.Y + e.Y
            );

            _form.Update();
        }

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
        }

        #endregion

    }
}
