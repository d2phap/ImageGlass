
using ImageGlass.Base;
using ImageGlass.Settings;
using ImageGlass.UI;

namespace ImageGlass;

public partial class FrmSlideshow : Form
{
    /// <summary>
    /// Gets, sets the index of the slideshow.
    /// </summary>
    public int SlideshowIndex { get; init; }

    /// <summary>
    /// Gets, sets value indicates whether the slideshow is requested to close from <see cref="FrmMain"/>.
    /// </summary>
    public bool IsRequestedToClose { get; set; } = false;


    public FrmSlideshow(int slideshowIndex)
    {
        InitializeComponent();

        SlideshowIndex = slideshowIndex;
        Text = $"Slideshow {SlideshowIndex + 1} - {Application.ProductName}";

        UpdateTheme();
    }

    private void FrmSlideshow_Load(object sender, EventArgs e)
    {
        Local.OnImageLoading += Local_OnImageLoading;
        Local.OnImageLoaded += Local_OnImageLoaded;

        _ = LoadCurrentImage();
    }

    private void FrmSlideshow_FormClosing(object sender, FormClosingEventArgs e)
    {
        Local.OnImageLoading -= Local_OnImageLoading;
        Local.OnImageLoaded -= Local_OnImageLoaded;

        Local.RaiseSlideshowWindowClosedEvent(new(SlideshowIndex));
    }

    private void UpdateTheme(SystemThemeMode mode = SystemThemeMode.Unknown)
    {
        var themMode = mode;

        if (mode == SystemThemeMode.Unknown)
        {
            themMode = ThemeUtils.GetSystemThemeMode();
        }

        // correct theme mode
        var isDarkMode = themMode != SystemThemeMode.Light;

        // background
        BackColor = Config.BackgroundColor;
        PicSlideshow.BackColor = Config.BackgroundColor;
        PicSlideshow.ForeColor = Config.Theme.Settings.TextColor;

        // navigation buttons
        PicSlideshow.NavHoveredColor = Color.FromArgb(200, Config.Theme.Settings.ToolbarBgColor);
        PicSlideshow.NavPressedColor = Color.FromArgb(240, Config.Theme.Settings.ToolbarBgColor);
        PicSlideshow.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicSlideshow.NavRightImage = Config.Theme.Settings.NavButtonRight;
    }

    

    private void Local_OnImageLoading(ImageLoadingEventArgs e)
    {
        PicSlideshow.ClearMessage();

        if (e.CurrentIndex >= 0 || !string.IsNullOrEmpty(e.FilePath))
        {
            PicSlideshow.ShowMessage(Config.Language["FrmMain._Loading"], "", delayMs: 1500);
        }
    }

    private void Local_OnImageLoaded(ImageLoadedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(Local_OnImageLoaded, e);
            return;
        }


        // image error
        if (e.Error != null)
        {
            PicSlideshow.SetImage(null);
            Local.IsImageError = true;
            Local.ImageModifiedPath = "";

            var currentFile = Local.Images.GetFilePath(e.Index);
            if (!string.IsNullOrEmpty(currentFile) && !File.Exists(currentFile))
            {
                Local.Images.Unload(e.Index);
            }

            PicSlideshow.ShowMessage(e.Error.Source + ": " + e.Error.Message,
                Config.Language["FrmMain.PicMain._ErrorText"]);
        }

        else if (!(e.Data?.ImgData.IsImageNull ?? true))
        {
            var isImageBigForFading = Local.Metadata.Width > 16000
                    || Local.Metadata.Height > 16000;
            var enableFading = !isImageBigForFading;

            // set the main image
            PicSlideshow.SetImage(e.Data.ImgData, enableFading, 0.1f, 0.02f);

            // Reset the zoom mode if KeepZoomRatio = FALSE
            if (!e.KeepZoomRatio)
            {
                // reset zoom mode
                if (PicSlideshow.ZoomMode == Config.ZoomMode)
                {
                    PicSlideshow.Refresh();
                }
                else
                {
                    PicSlideshow.ZoomMode = Config.ZoomMode;
                }
            }

            PicSlideshow.ClearMessage();
        }
    }


    private async Task LoadCurrentImage()
    {
        var photo = await Local.Images.GetAsync(Local.CurrentIndex);

        Local_OnImageLoaded(new()
        {
            Index = Local.CurrentIndex,
            Error = photo.Error,
            Data = photo,
            KeepZoomRatio = false,
        });
    }
}
