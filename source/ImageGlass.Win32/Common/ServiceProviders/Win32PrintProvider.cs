/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using ImageGlass.Common;
using ImageGlass.Common.Printing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Win32.Common.ServiceProviders;

public class Win32PrintProvider : PrintProviderBase
{
    /// <inheritdoc/>
    protected override Task<IReadOnlyList<PrinterInfo>> GetSystemPrintersAsync(CancellationToken token)
    {
        return Task.FromResult<IReadOnlyList<PrinterInfo>>([]);
    }


    /// <inheritdoc/>
    protected override Task<PrinterCapabilities> GetSystemCapabilitiesAsync(PrinterInfo printer, CancellationToken token)
    {
        return Task.FromResult(PdfCapabilities);
    }


    /// <inheritdoc/>
    protected override Task PrintToSystemAsync(PrintJob job, IProgress<PrintProgress>? progress, CancellationToken token)
    {
        throw new NotSupportedException();
    }


    /// <inheritdoc/>
    public override bool CanShowSystemDialog => true;


    /// <summary>
    /// Opens Print Pictures with the shown frame, every edit applied.
    /// </summary>
    public override async Task ShowSystemDialogAsync(PhWindow owner, PrintJob job, CancellationToken token)
    {
        var path = await WriteShownImagePngAsync(job, token);
        if (string.IsNullOrEmpty(path)) return;

        Win32PrintApi.OpenPrintDialog(BHelper.GetRealPlatformPath(path));
    }
}
