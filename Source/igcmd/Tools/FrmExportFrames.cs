/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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

using Cysharp.Text;
using ImageGlass.Base;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Settings;

namespace igcmd.Tools;

public partial class FrmExportFrames : DialogForm
{
    private CancellationTokenSource? _exportCancellable;
    private IProgress<(int, string)> _exportProgress;


    private string SrcFilePath { get; set; } = string.Empty;
    private string DestDirPath { get; set; } = string.Empty;
    private int FrameCount { get; set; } = 0;


    public FrmExportFrames(string srcFilePath, string destDirPath) : base()
    {
        InitializeComponent();

        // load form data
        SrcFilePath = srcFilePath;
        DestDirPath = destDirPath;

        FrameCount = PhotoCodec.LoadMetadata(SrcFilePath).FrameCount;
        ProgressBar.Step = 1000;
        ProgressBar.Maximum = FrameCount * ProgressBar.Step;
        ProgressBar.Style = ProgressBarStyle.Marquee;


        LblStatus.Text = ZString.Format(Config.Language[$"{Name}._Exporting"], 1, FrameCount, SrcFilePath);

        _exportProgress = new Progress<(int, string)>(ReportProgress);
    }


    // Override / Virtual methods
    #region Override / Virtual methods

    protected override void OnLoad(EventArgs e)
    {
        // must be before base.OnLoad()
        ApplyLanguage();

        // update form height
        base.OnLoad(e);

        // must be after base.OnLoad()
        ApplyTheme(Config.Theme.Settings.IsDarkMode);
        _ = Config.UpdateFormIcon(this);


        Activate();
        _ = BHelper.RunAsThread(StartExporting);
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();

        LblStatus.DarkMode = DarkMode;
        TableTop.BackColor = Config.Theme.ColorPalatte.AppBg;


        base.ApplyTheme(darkMode, style);
        ResumeLayout();
    }


    protected override int OnUpdateHeight(bool performUpdate = true)
    {
        var baseHeight = base.OnUpdateHeight(false);
        var formHeight = TableTop.Height + baseHeight;

        if (performUpdate && Height != formHeight)
        {
            Height = formHeight;
        }

        return formHeight;
    }


    protected override void OnAcceptButtonClicked()
    {
        var files = Directory.EnumerateFiles(DestDirPath);
        var filePath = files.FirstOrDefault();

        BHelper.OpenFilePath(filePath);
    }


    protected override void OnCancelButtonClicked()
    {
        // cancel the progress
        _exportCancellable?.Cancel();

        base.OnCancelButtonClicked();
    }

    #endregion // Override / Virtual methods


    // Private methods
    #region Private methods

    private void ApplyLanguage()
    {
        BtnCancel.Text = Config.Language["_._Cancel"];

        Text = Config.Language[$"{Name}._Title"];
    }


    /// <summary>
    /// Starts exporting image frames.
    /// </summary>
    private void StartExporting()
    {
        _exportCancellable?.Cancel();
        _exportCancellable = new();

        _ = StartExportingAsync();
    }


    /// <summary>
    /// Starts exporting image frames and reports the progress.
    /// </summary>
    private async Task StartExportingAsync()
    {
        await foreach (var info in PhotoCodec.SaveFramesAsync(SrcFilePath, DestDirPath, _exportCancellable.Token))
        {
            _exportProgress.Report((info.FrameNumber, info.FileName));
        }
    }


    /// <summary>
    /// Reports progress to the UI.
    /// </summary>
    private void ReportProgress((int FrameNumber, string FileName) info)
    {
        if (ProgressBar.Style != ProgressBarStyle.Continuous)
        {
            ProgressBar.Style = ProgressBarStyle.Continuous;
        }

        ProgressBar.PerformStep();

        var percent = Math.Round((info.FrameNumber * 100f) / FrameCount, 0);
        Text = $"{Config.Language[$"{Name}._Title"]} ({percent}%)";

        // Done
        if (info.FrameNumber == FrameCount)
        {
            AcceptButtonText = Config.Language[$"{Name}._OpenOutputFolder"];
            CancelButtonText = Config.Language["_._Close"];

            ShowAcceptButton = true;
            BtnAccept.Focus();

            ProgressBar.Value -= 1;
            LblStatus.Text = ZString.Format(Config.Language[$"{Name}._ExportDone"], info.FrameNumber, $"\"{DestDirPath}\"");
        }
        // in progress
        else
        {
            var frameFilePath = Path.Combine(DestDirPath, info.FileName);
            LblStatus.Text = ZString.Format(Config.Language[$"{Name}._Exporting"], info.FrameNumber, FrameCount, $"\"{frameFilePath}\"");
        }

        OnUpdateHeight();
    }

    #endregion // Private methods

}
