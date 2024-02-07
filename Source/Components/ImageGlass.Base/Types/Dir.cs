
namespace ImageGlass.Base;

/// <summary>
/// Directory name constants
/// </summary>
public static class Dir
{
    /// <summary>
    /// Gets the Themes folder name
    /// </summary>
    public static string Themes => "Themes";

    /// <summary>
    /// Gets the Icons folder name
    /// </summary>
    public static string Icons => "Icons";

    /// <summary>
    /// Gets the Ext-Icons folder name
    /// </summary>
    public static string ExtIcons => "Ext-Icons";

    /// <summary>
    /// Gets the Languages folder name
    /// </summary>
    public static string Language => "Language";

    /// <summary>
    /// Gets the WebUI folder name
    /// </summary>
    public static string WebUI => "WebUI";

    /// <summary>
    /// Gets the WebView2_Runtime folder.
    /// </summary>
    public static string WebView2Runtime => "WebView2_Runtime";

    /// <summary>
    /// Gets the cached thumbnails folder name
    /// </summary>
    public static string ThumbnailsCache => "ThumbnailsCache";

    /// <summary>
    /// Gets the License folder name
    /// </summary>
    public static string License => "License";

    /// <summary>
    /// Gets the temporary folder name
    /// </summary>
    public static string Temporary => "Temp";

#if DEBUG
    /// <summary>
    /// Logging should not be to the temporary folder, as it is deleted on shutdown
    /// </summary>
    public static string Log => "Log";
#endif

}
