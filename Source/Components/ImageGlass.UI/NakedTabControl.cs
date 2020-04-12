using System;
using System.Windows.Forms;

namespace ImageGlass.UI {
    public class NakedTabControl: TabControl {
        public NakedTabControl() {
            // hide the one-line navigation buttons
            if (!this.DesignMode) this.Multiline = true;
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == 0x1328 && !this.DesignMode)
                m.Result = new IntPtr(1);
            else
                base.WndProc(ref m);
        }
    }
}
