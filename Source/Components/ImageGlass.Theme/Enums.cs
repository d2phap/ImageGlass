using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGlass.Theme
{
    public enum ThemeUninstallResult
    {
        SUCCESS = 0,
        ERROR = 1,
        ERROR_THEME_NOT_FOUND = 2
    }

    public enum ThemeInstallResult {
        UNKNOWN = -1,
        SUCCESS = 0,
        ERROR = 1
    }

}
