/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.UI.Windowing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Tools;

public partial class LosslessCompressionWindow : ModalWindow
{
    // the file can be briefly locked by a pending read (post-save reload, thumbnail,
    // metadata), which makes the optimizer fail to overwrite it => retry a few times.
    private const int MAX_ATTEMPTS = 5;
    private const int RETRY_DELAY_MS = 300;

    private readonly CancellationTokenSource _cancel = new();
    private readonly FileInfo _srcFileInfo;
    private bool _isRunning; // UI thread only



    public LosslessCompressionWindow(string srcFilePath)
    {
        if (string.IsNullOrEmpty(srcFilePath)) throw new ArgumentNullException(nameof(srcFilePath));

        _srcFileInfo = new FileInfo(srcFilePath);

        ShowInTaskbar = true;
        Note = $"""
            {srcFilePath}

            {Core.Photos.CurrentMetadata?.FileSizeFormatted}
            """;
        NoteStyle = InfoBarSeverity.Info;
        Thumbnail = Core.Photos.Current?.GalleryThumbnail;

        IsButton1Visible = true;
        IsButton2Visible = true;
        IsButton3Visible = false;
        DefaultButton = DialogButton.Button1;
        DefaultFocus = DialogFocus.Button1;
    }



    #region Override Methods

    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = Core.Lang[LangId.Menu_MnuLosslessCompression];
        Heading = Core.Lang[LangId.Menu_MnuLosslessCompression_Confirm];
        Description = Core.Lang[LangId.Menu_MnuLosslessCompression_Description];
        Button1Text = Core.Lang[LangId._Yes];
        Button2Text = Core.Lang[LangId._No];
    }


    protected override void OnDialogSubmitted(DialogEventArgs e)
    {
        // block re-entry: pressing Enter again would start a second concurrent run,
        // and the two runs then collide while overwriting the same file
        if (_isRunning) return;
        _isRunning = true;

        _ = RunAsync(_srcFileInfo);
    }


    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // signal the running job to stop updating this window, whatever closed it
        _cancel.Cancel();
        _cancel.Dispose();
    }

    #endregion // Override Methods



    #region Private methods

    /// <summary>
    /// Performs lossless compression.
    /// </summary>
    private async Task RunAsync(FileInfo fi)
    {
        var oldFileLength = fi.Length;
        var oldFileSizeFormatted = Core.Photos.CurrentMetadata?.FileSizeFormatted
            ?? BHelper.FormatSize(oldFileLength);
        var token = _cancel.Token;

        // 1. switch the dialog to the 'compressing' state
        _btn1.IsEnabled = false;
        _btn1.IsDefault = false;
        _btn2.Focus(Avalonia.Input.NavigationMethod.Tab);

        IsButton1Visible = false;
        IsButton2Visible = true;
        Button2Text = Core.Lang[LangId._Cancel];
        DefaultButton = DialogButton.Button2; // Enter must not re-submit

        IsProgressVisible = true;
        IsProgressIndeterminate = true;
        ProgressValue = 0;

        Heading = Core.Lang[LangId.Menu_MnuLosslessCompression_Compressing];
        Description = Core.Lang[LangId.Menu_MnuLosslessCompression_Description];
        Note = $"""
            {fi.FullName}

            {oldFileSizeFormatted}
            """;


        // 2. compress the file
        // never let this throw: an unobserved failure would leave the dialog stuck in the
        // 'compressing' state and surface later as an unhandled exception (app freeze).
        try
        {
            await CompressAsync(fi.FullName, token);
            await Task.Delay(200); // make it feel slow for better UX
            if (token.IsCancellationRequested) return;

            // 3.1. done, show stats
            var newFi = new FileInfo(fi.FullName);
            var percent = Math.Round((1 - (newFi.Length * 1f / oldFileLength)) * 100f, 2);

            Heading = Core.Lang[LangId.Menu_MnuLosslessCompression_Done];
            Note = $"""
                {newFi.FullName}

                {oldFileSizeFormatted} ⇒ {BHelper.FormatSize(newFi.Length)} (↓ {percent}%)
                """;
        }
        catch (Exception ex)
        {
            if (token.IsCancellationRequested) return;

            // 3.2. failed, show the reason
            Heading = Core.Lang[LangId.Menu_MnuLosslessCompression_Error];
            NoteStyle = InfoBarSeverity.Danger;
            Note = $"""
                {fi.FullName}

                {ex.Message}
                """;
            Details = BHelper.GetExceptionDetails(ex);
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                // always leave the dialog closable
                ProgressValue = 100;
                IsProgressIndeterminate = false;
                IsProgressVisible = false;

                Button2Text = Core.Lang[LangId._Close];
                SetDefaultButton(DialogButton.Button2);
                _btn2.Focus(Avalonia.Input.NavigationMethod.Tab);
            }
        }
    }


    /// <summary>
    /// Compresses the file on a background thread, retrying while the file is still
    /// locked by another reader.
    /// </summary>
    private static async Task<bool> CompressAsync(string filePath, CancellationToken token)
    {
        for (var attempt = 1; ; attempt++)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                // run on a dedicated thread: compression is CPU-bound and can take seconds
                return await Task.Factory.StartNew(() => MagickCodec.LosslessCompress(filePath),
                    CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                    .ConfigureAwait(false);
            }
            catch (IOException) when (attempt < MAX_ATTEMPTS)
            {
                // the file is in use, back off and retry
                await Task.Delay(RETRY_DELAY_MS * attempt).ConfigureAwait(false);
            }
        }
    }

    #endregion // Private Methods


}
