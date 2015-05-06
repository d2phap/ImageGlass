using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageGlass.Services.InstanceManagement
{
    /// <summary>
    /// Holds a list of arguments given to an application at startup.
    /// </summary>
    public class ArgumentsReceivedEventArgs : EventArgs
    {
        public String[] Args { get; set; }
    }
}
