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
using ImageGlass.Common.Printing;
using ImageGlass.Common.ServiceProviders;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Storage.Xps;

namespace ImageGlass.Win32.Common;


/// <summary>
/// Prints a job through GDI: each cell is a raster drawn by the same renderer as the preview, sent in bands by a thread that owns the device context.
/// </summary>
internal static unsafe class Win32GdiPrintJob
{
    // the largest band of a cell's raster, so a big print never needs one huge bitmap
    private const long BAND_BYTES = 32L * 1024 * 1024;

    // vectors and captions print no finer than this, which bounds the spool of a full page
    private const float MAX_RASTER_DPI = 600;

    private const int GDI_ERROR = -1;


    /// <summary>
    /// Prints the job on the printer of its settings with the given DEVMODE; <paramref name="ownCopies"/> are copies the driver cannot make.
    /// </summary>
    public static Task RunAsync(PrintJob job, byte[] devMode, int ownCopies, IProgress<PrintProgress>? progress, CancellationToken token)
    {
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            var isAborted = false;
            try
            {
                Run(job, devMode, ownCopies, progress, token, ref isAborted);
                done.SetResult();
            }
            catch (OperationCanceledException)
            {
                done.SetCanceled(token);
            }
            catch (Exception ex)
            {
                done.SetException(ex);
            }

            // the window has its answer, so this thread can wait for the spooler to let go of the file
            if (isAborted && !string.IsNullOrEmpty(job.Settings.OutputPath)) DeleteAbortedOutput(job.Settings.OutputPath);
        })
        {
            IsBackground = true,
            Name = "ImageGlass print job",
        };

        // a driver may open a window of its own while printing, which needs a single-threaded apartment
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return done.Task;
    }


    private static void Run(PrintJob job, byte[] devMode, int ownCopies, IProgress<PrintProgress>? progress,
        CancellationToken token, ref bool isAborted)
    {
        var settings = job.Settings;
        HDC hdc;

        fixed (char* pName = settings.Printer.Id)
        fixed (byte* pDevMode = devMode)
        {
            hdc = PInvoke.CreateDCW(default, pName, default, (DEVMODEW*)pDevMode);
        }

        if (hdc.IsNull) throw new Win32Exception(Marshal.GetLastSystemError());

        var isStarted = false;
        try
        {
            // the driver scales each raster to the paper with its best filter
            _ = PInvoke.SetStretchBltMode(hdc, STRETCH_BLT_MODE.HALFTONE);
            _ = PInvoke.SetBrushOrgEx(hdc, 0, 0, null);
            var device = DeviceGeometry.Read(hdc);

            // a printer that writes a file is given the path the user picked, so its driver asks nothing
            fixed (char* pTitle = job.Title)
            fixed (char* pOutput = settings.OutputPath)
            {
                var info = new DOCINFOW
                {
                    cbSize = sizeof(DOCINFOW),
                    lpszDocName = pTitle,
                    lpszOutput = pOutput,
                };

                if (PInvoke.StartDoc(hdc, &info) <= 0) throw new Win32Exception(Marshal.GetLastSystemError());
            }

            isStarted = true;
            var sheets = GetSheets(job.Layout.Pages.Count, ownCopies, settings.Collate, settings.Duplex != PrintDuplex.None);

            for (var i = 0; i < sheets.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                progress?.Report(new PrintProgress(i, sheets.Count));

                if (PInvoke.StartPage(hdc) <= 0) throw new Win32Exception(Marshal.GetLastSystemError());
                if (sheets[i] is int pageIndex) DrawPage(hdc, device, job, pageIndex, token);
                if (PInvoke.EndPage(hdc) <= 0) throw new Win32Exception(Marshal.GetLastSystemError());
            }

            if (PInvoke.EndDoc(hdc) <= 0) throw new Win32Exception(Marshal.GetLastSystemError());
            isStarted = false;

            progress?.Report(new PrintProgress(sheets.Count, sheets.Count));
        }
        finally
        {
            if (isStarted)
            {
                _ = PInvoke.AbortDoc(hdc);
                isAborted = true;
            }

            _ = PInvoke.DeleteDC(hdc);
        }
    }


    /// <summary>
    /// Orders the pages of every copy the app makes itself; a <c>null</c> sheet is a blank back, so a two-sided copy starts on a new sheet.
    /// </summary>
    private static List<int?> GetSheets(int pageCount, int copies, bool collate, bool isTwoSided)
    {
        var sheets = new List<int?>(pageCount * copies);

        if (!collate)
        {
            for (var page = 0; page < pageCount; page++)
            {
                for (var copy = 0; copy < copies; copy++) sheets.Add(page);
            }

            return sheets;
        }

        for (var copy = 0; copy < copies; copy++)
        {
            for (var page = 0; page < pageCount; page++) sheets.Add(page);
            if (isTwoSided && pageCount % 2 == 1 && copy < copies - 1) sheets.Add(null);
        }

        return sheets;
    }


    /// <summary>
    /// Draws the cells of a page, each as a raster at its image's own resolution, which the driver scales to the paper.
    /// </summary>
    private static void DrawPage(HDC hdc, DeviceGeometry device, PrintJob job, int pageIndex, CancellationToken token)
    {
        var maxDpi = Math.Min(MAX_RASTER_DPI, Math.Min(device.DpiX, device.DpiY));

        // images are requested at the device's resolution, so measuring a cell and drawing it share one decode
        var render = job.Render with { ColorMode = job.Settings.ColorMode, TargetDpi = maxDpi, IsPreview = false };

        foreach (var cell in job.Layout.Pages[pageIndex].Cells)
        {
            token.ThrowIfCancellationRequested();

            var dpi = PrintPageRenderer.GetCellRasterDpiAsync(cell, job.Session, render, maxDpi, token).GetAwaiter().GetResult();
            DrawCell(hdc, device, job, pageIndex, cell.RectPt, dpi, render, token);
        }
    }


    /// <summary>
    /// Renders one cell, caption included, in horizontal bands and stretches each band onto its part of the device page.
    /// </summary>
    private static void DrawCell(HDC hdc, DeviceGeometry device, PrintJob job, int pageIndex,
        SKRect rectPt, float dpi, PrintRenderOptions render, CancellationToken token)
    {
        var left = device.ToDeviceX(rectPt.Left);
        var top = device.ToDeviceY(rectPt.Top);
        var right = device.ToDeviceX(rectPt.Right);
        var bottom = device.ToDeviceY(rectPt.Bottom);
        if (right <= left || bottom <= top) return;

        var width = Math.Max(1, (int)Math.Ceiling(PrintUnits.PtToPx(rectPt.Width, dpi)));
        var height = Math.Max(1, (int)Math.Ceiling(PrintUnits.PtToPx(rectPt.Height, dpi)));
        var bandRows = (int)Math.Clamp(BAND_BYTES / (width * 4L), 1, height);

        for (var y = 0; y < height; y += bandRows)
        {
            token.ThrowIfCancellationRequested();
            var rows = Math.Min(bandRows, height - y);

            // 1. the band of the cell, white like the paper
            var info = new SKImageInfo(width, rows, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            using var band = new SKBitmap(info);
            using (var canvas = new SKCanvas(band))
            {
                canvas.Clear(SKColors.White);
                canvas.Translate(0, -y);
                canvas.Scale(width / rectPt.Width, height / rectPt.Height);
                canvas.Translate(-rectPt.Left, -rectPt.Top);
                canvas.ClipRect(rectPt);

                PrintPageRenderer.DrawPageAsync(canvas, job.Layout, pageIndex, job.Session, render, token).GetAwaiter().GetResult();
            }

            // 2. its rows of the device rectangle, rounded so neighboring bands neither overlap nor leave a gap
            var destTop = top + (int)Math.Round((bottom - top) * (y / (double)height));
            var destBottom = top + (int)Math.Round((bottom - top) * ((y + rows) / (double)height));
            if (destBottom <= destTop) continue;

            var bitmapInfo = new BITMAPINFO
            {
                bmiHeader = new BITMAPINFOHEADER
                {
                    biSize = (uint)sizeof(BITMAPINFOHEADER),
                    biWidth = width,
                    biHeight = -rows,
                    biPlanes = 1,
                    biBitCount = 32,
                    biCompression = 0,
                },
            };

            var copied = PInvoke.StretchDIBits(hdc, left, destTop, right - left, destBottom - destTop,
                0, 0, width, rows, (void*)band.GetPixels(), &bitmapInfo, DIB_USAGE.DIB_RGB_COLORS, ROP_CODE.SRCCOPY);

            if (copied == 0 || copied == GDI_ERROR) throw new Win32Exception(Marshal.GetLastSystemError());
        }
    }


    /// <summary>
    /// Deletes the file an aborted job leaves, which the spooler opens with the first page and still holds for a moment after the abort.
    /// </summary>
    private static void DeleteAbortedOutput(string path)
    {
        for (var attempt = 0; attempt < 20 && File.Exists(path); attempt++)
        {
            try
            {
                File.Delete(path);
                return;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            Thread.Sleep(250);
        }
    }


    /// <summary>
    /// The resolution and printable area of a device page, which maps points on the paper to device pixels.
    /// </summary>
    private readonly record struct DeviceGeometry(int DpiX, int DpiY, int OffsetX, int OffsetY)
    {
        public static DeviceGeometry Read(HDC hdc)
        {
            return new DeviceGeometry(
                PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.LOGPIXELSX),
                PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.LOGPIXELSY),
                PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.PHYSICALOFFSETX),
                PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.PHYSICALOFFSETY));
        }


        /// <summary>
        /// Converts a horizontal position on the paper to the device, whose origin is the corner of the printable area.
        /// </summary>
        public int ToDeviceX(float pt) => (int)Math.Round(PrintUnits.PtToPx(pt, DpiX)) - OffsetX;


        /// <summary>
        /// Converts a vertical position on the paper to the device, whose origin is the corner of the printable area.
        /// </summary>
        public int ToDeviceY(float pt) => (int)Math.Round(PrintUnits.PtToPx(pt, DpiY)) - OffsetY;
    }
}
