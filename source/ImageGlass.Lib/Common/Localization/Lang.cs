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
using ImageGlass.Common.Types;
using ImageGlass.Common.Types.JsonTypeConverters;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImageGlass.Common.Localization;


[JsonSerializable(typeof(Lang))]
public partial class LangJsonContext : JsonSerializerContext { }


/// <summary>
/// ImageGlass language pack (<c>*.iglang.json</c>)
/// </summary>
public class Lang
{
    /// <summary>
    /// Minimum <see cref="LangMetadata.MinVersion"/> a pack must declare to be compatible.
    /// </summary>
    public static float SPEC_VERSION => 10;


    #region JSON Serializable Properties

    /// <summary>
    /// Gets, sets the language metadata.
    /// </summary>
    [JsonPropertyName("_Metadata")]
    public LangMetadata Metadata { get; set; } = new();


    /// <summary>
    /// Gets, sets the language string dictionary. Unknown keys (from version-skewed packs) are
    /// skipped on load rather than collapsing onto <c>default(LangId)</c>.
    /// </summary>
    [JsonConverter(typeof(JsonLangItemsConverter))]
    public IDictionary<LangId, string> Items { get; set; } = FrozenDictionary<LangId, string>.Empty;

    #endregion // JSON Serializable Properties


    #region Non-Serializable Properties

    /// <summary>
    /// Gets the path of language file.
    /// Example: <c>C:\ImageGlass\Languages\Vietnameses.iglang.json</c>
    /// </summary>
    [JsonIgnore]
    public string FilePath { get; private set; } = "English";


    /// <summary>
    /// Gets <see cref="FilePath"/> as the OS sees it, for display: a pack ships in the app dir or is
    /// installed into the config dir, and either can be reached through a platform indirection.
    /// </summary>
    [JsonIgnore]
    public string DisplayFilePath => BHelper.GetRealPlatformPath(FilePath);


    /// <summary>
    /// Gets the name of language file.
    /// Example: <c>Vietnameses.iglang.json</c>
    /// </summary>
    [JsonIgnore]
    public string FileName => Path.GetFileName(FilePath);


    /// <summary>
    /// Check if the this is the built-in language pack.
    /// </summary>
    [JsonIgnore]
    public bool IsBuiltIn => !Path.IsPathRooted(FilePath);


    /// <summary>
    /// Gets the formatted language string. If not exist, returns the key name.
    /// </summary>
    /// <param name="key">The key to get the language string</param>
    /// <param name="args">The arguments to format the language string.</param>
    /// <remarks>
    /// This is a shortcut for <see cref="Get(string, object?[])"/> method.
    /// </remarks>
    [JsonIgnore]
    public string this[string? key, params object?[] args] => Get(key, args);


    /// <summary>
    /// Gets the formatted language string. If not exist, returns empty string.
    /// </summary>
    /// <param name="key">The key to get the language string</param>
    /// <param name="args">The arguments to format the language string.</param>
    /// <remarks>
    /// This is a shortcut for <see cref="Get(LangId?, object?[])"/> method.
    /// </remarks>
    [JsonIgnore]
    public string this[LangId? key, params object?[] args] => Get(key, args);

    #endregion // Non-Serializable Properties


    #region Instance Initialization

    /// <summary>
    /// Initializes a language pack.
    /// </summary>
    public Lang() { }


    /// <summary>
    /// Initializes a language pack.
    /// </summary>
    /// <param name="filePath">E.g. <c>C:\ImageGlass\Language\Vietnamese.iglang.json</c></param>
    public Lang(string filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            FilePath = filePath;
        }
    }

    #endregion // Instance Initialization


    #region Public Methods

    /// <summary>
    /// Reads <see cref="FilePath"/> and loads language strings.
    /// </summary>
    public async Task LoadAsync()
    {
        if (!File.Exists(FilePath)) return;

        // 1. create json context
        var jsonOptions = BHelper.CreateJsonOptions();
        var jsonContext = new LangJsonContext(jsonOptions);

        try
        {
            // 2. load language strings
            var lang = await BHelper.ReadJsonFromFileAsync(FilePath, jsonContext.Lang);
            if (lang == null) return;

            // 3. store the language strings
            Metadata = lang.Metadata;
            Items = lang.Items.ToFrozenDictionary();
        }
        catch (Exception ex)
        {
            // don't let a bad pack abort startup, but surface it (this silently hid a real bug)
            System.Diagnostics.Debug.WriteLine($"[Lang.LoadAsync] failed for '{FilePath}': {ex.Message}");
        }
    }


    /// <summary>
    /// Saves current language to JSON file.
    /// </summary>
    public async Task SaveAsFileAsync(string filePath)
    {
        var lang = new Lang()
        {
            Metadata = Metadata,
            Items = Items,
        };

        if (Metadata.EnglishName.Equals("English", StringComparison.OrdinalIgnoreCase))
        {
            lang.Metadata.EnglishName = "<Your_language_name_in_English>";
            lang.Metadata.LocalName = "<Local_name_of_your_language>";
            lang.Metadata.Author = "<Your_name_here>";
        }


        var jsonOptions = BHelper.CreateJsonOptions();
        var jsonContext = new LangJsonContext(jsonOptions);

        await BHelper.WriteJsonToFileAsync(filePath, lang, jsonContext.Lang);
    }


    /// <summary>
    /// Gets a valid <see cref="LangId"/> from string.
    /// </summary>
    public static LangId? GetKey(string? key)
    {
        if (Enum.TryParse<LangId>(key, out var langKey))
        {
            return langKey;
        }

        return null;
    }


    /// <summary>
    /// Gets the formatted language string. If not exist, returns the key.
    /// </summary>
    /// <param name="key">The key to get the language string</param>
    /// <param name="args">The arguments to format the language string.</param>
    public string Get(string? key, params object?[] args)
    {
        if (GetKey(key) is LangId langKey)
        {
            return Get(langKey, args);
        }

        return key ?? string.Empty;
    }


    /// <summary>
    /// Gets the formatted language string. If not exist, returns empty string.
    /// </summary>
    /// <param name="key">The key to get the language string</param>
    /// <param name="args">The arguments to format the language string.</param>
    public string Get(LangId? key, params object?[] args)
    {
        if (key is null) return string.Empty;
        string? value = null;


        // 1. try getting value from language file
        if (Items.TryGetValue(key.Value, out value))
        {
            // do nothing
        }

        // 2. try getting value from default language dictionary
        else if (DefaultLangMap.TryGetValue(key.Value, out value))
        {
            // do nothing
        }
        else
        {
            return string.Empty;
        }


        // 3. if value has arguments, return the formatted string
        if (args.Length > 0)
        {
            return string.Format(value, args);
        }

        // 4. returns the non-formatted string
        return value;
    }


    /// <summary>
    /// Gets the formatted language string. If not exist, returns the key.
    /// </summary>
    /// <param name="key">The key to get the language string</param>
    public string Get(string? key) => Get(key, []);


    /// <summary>
    /// Gets the formatted language string. If not exist, returns empty string.
    /// </summary>
    /// <param name="key">The key to get the language string</param>
    public string Get(LangId? key) => Get(key, []);


    /// <summary>
    /// Resolves a stored language value (a bare <c>*.iglang.json</c> file name or an absolute path)
    /// to a full path. A user pack in the Config dir takes precedence over a built-in pack of the
    /// same name in the app base dir.
    /// </summary>
    public static string ResolveFilePath(string fileNameOrPath)
    {
        if (string.IsNullOrEmpty(fileNameOrPath) || Path.IsPathRooted(fileNameOrPath))
            return fileNameOrPath;

        var userPath = BHelper.ConfigDir(Dir.Language, fileNameOrPath);
        if (File.Exists(userPath)) return userPath;

        return BHelper.BaseDir(Dir.Language, fileNameOrPath);
    }


    /// <summary>
    /// Loads every installed language pack: built-in packs from the app base dir and user packs
    /// from the Config dir. Incompatible packs are skipped. Packs are de-duplicated by file name
    /// (a user pack shadows a built-in one of the same name). The result is sorted by local name.
    /// </summary>
    public static async Task<List<Lang>> LoadAllLanguagePacksAsync()
    {
        var found = new Dictionary<string, Lang>(StringComparer.OrdinalIgnoreCase);

        // base dir (built-in) first, then Config dir (user) so user packs win on a name clash
        foreach (var rootDir in new[] { BHelper.BaseDir(Dir.Language), BHelper.ConfigDir(Dir.Language) })
        {
            if (!Directory.Exists(rootDir)) continue;

            foreach (var file in Directory.EnumerateFiles(rootDir, "*.iglang.json"))
            {
                if (GetPackMinVersion(file) < SPEC_VERSION) continue;

                var lang = new Lang(file);
                await lang.LoadAsync().ConfigureAwait(false);
                found[lang.FileName] = lang;
            }
        }

        return found.Values
            .OrderBy(l => l.Metadata.LocalName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }


    /// <summary>
    /// Installs language packs by copying the given <c>*.iglang.json</c> files into the user
    /// language folder, skipping packs built for an older ImageGlass version.
    /// </summary>
    public static async Task<LangPackInstallResult> InstallLanguagePacksAsync(IEnumerable<string> iglangFilePaths)
    {
        var destDir = BHelper.ConfigDir(Dir.Language);
        Directory.CreateDirectory(destDir);

        return await Task.Run(() =>
        {
            var installed = 0;
            var incompatible = new List<string>();

            foreach (var file in iglangFilePaths)
            {
                if (!File.Exists(file)) continue;

                // reject packs made for an older version (see GetPackMinVersion)
                if (GetPackMinVersion(file) < SPEC_VERSION)
                {
                    incompatible.Add(GetPackFileName(file));
                    continue;
                }

                try
                {
                    File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
                    installed++;
                }
                catch { }
            }

            return new LangPackInstallResult(installed, incompatible);
        }).ConfigureAwait(false);
    }


    /// <summary>
    /// Whether the pack file targets this app version. The built-in English pack (no/missing file)
    /// is always compatible; a real pack must declare <c>_Metadata.MinVersion</c> &gt;= <see cref="SPEC_VERSION"/>.
    /// </summary>
    public static bool IsPackFileCompatible(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return true;
        return GetPackMinVersion(filePath) >= SPEC_VERSION;
    }


    /// <summary>
    /// Reads a pack's declared <c>_Metadata.MinVersion</c> from the file. Returns 0 (incompatible)
    /// when the file is invalid or the value is missing. MinVersion may be a number or a string.
    /// </summary>
    private static float GetPackMinVersion(string filePath)
    {
        try
        {
            using var doc = BHelper.ReadJsonDocFromFile(filePath);
            if (doc?.RootElement is not { ValueKind: JsonValueKind.Object } root
                || !root.TryGetProperty("_Metadata", out var meta)
                || !meta.TryGetProperty(nameof(LangMetadata.MinVersion), out var ver))
                return 0;

            if (ver.ValueKind == JsonValueKind.Number && ver.TryGetSingle(out var num)) return num;
            if (ver.ValueKind == JsonValueKind.String
                && float.TryParse(ver.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var str))
                return str;
        }
        catch { }

        return 0;
    }


    /// <summary>
    /// Gets the pack name from a path, stripping the trailing <c>.iglang.json</c>.
    /// </summary>
    private static string GetPackFileName(string filePath)
    {
        var name = Path.GetFileName(filePath);
        const string suffix = ".iglang.json";
        if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) name = name[..^suffix.Length];
        return name;
    }


    #endregion // Public Methods


    /// <summary>
    /// Map of <see cref="LangId"/> and language key. Built once (these never change at runtime);
    /// previously a computed property that rebuilt the whole frozen dictionary on every access.
    /// </summary>
    public static FrozenDictionary<LangId, string> KeysMap { get; }


    /// <summary>
    /// Map of <see cref="LangId"/> and default localization. Built once; see <see cref="KeysMap"/>.
    /// </summary>
    public static FrozenDictionary<LangId, string> DefaultLangMap { get; }


    // build the read-only maps once; the static ctor runs after all static field initializers,
    // so _defaultLangList (declared below) is already populated here.
    static Lang()
    {
        var keys = new Dictionary<LangId, string>();
        foreach (var name in Enum.GetNames<LangId>())
        {
            keys[Enum.Parse<LangId>(name)] = name;
        }
        KeysMap = keys.ToFrozenDictionary();

        DefaultLangMap = new Dictionary<LangId, string>(_defaultLangList).ToFrozenDictionary();
    }


    // the default language list
    private static IReadOnlyCollection<KeyValuePair<LangId, string>> _defaultLangList = [

        #region General

        new(LangId._OK, "OK"), // v9.0
        new(LangId._Cancel, "Cancel"), // v9.0
        new(LangId._Apply, "Apply"), // v9.0
        new(LangId._Close, "Close"), // v9.0
        new(LangId._Yes, "Yes"), // v9.0
        new(LangId._No, "No"), // v9.0
        new(LangId._LearnMore, "Learn more…"), // v9.0
        new(LangId._Continue, "Continue"), // v9.0
        new(LangId._Quit, "Quit"), // v9.0
        new(LangId._Back, "Back"), // v9.0
        new(LangId._Next, "Next"), // v9.0
        new(LangId._Save, "Save"), // v9.0
        new(LangId._Warning, "Warning"), // v9.0
        new(LangId._Copy, "Copy"), //v9.0
        new(LangId._Browse, "Browse…"), //v9.0
        new(LangId._Reset, "Reset"), //v9.0
        new(LangId._ResetToDefault, "Reset to default"), //v9.0
        new(LangId._CheckForUpdate, "Check for update…"), //v5.0
        new(LangId._Update, "Update"), //v9.0
        new(LangId._Download, "Download"), //v10.0
        new(LangId._RestartNow, "Restart now"), //v10.0
        new(LangId._Website, "Website"), //v9.0
        new(LangId._TypeToFilter, "Type to filter…"),
        new(LangId._Delete, "Delete"),
        new(LangId._Add, "Add"),
        new(LangId._Edit, "Edit"),
        new(LangId._ID, "ID"),
        new(LangId._Name, "Name"),
        new(LangId._Hotkeys, "Hotkeys"),
        new(LangId._Executable, "Executable"),
        new(LangId._Argument, "Argument"),
        new(LangId._CommandPreview, "Command preview"),
        new(LangId._FileExtension, "File extension"),
        new(LangId._Codec, "Codec"),
        new(LangId._Decoder, "Decoder"),
        new(LangId._Encoder, "Encoder"),
        new(LangId._Empty, "(empty)"),
        new(LangId._Separator, "Separator"),
        new(LangId._Icon, "Icon"),
        new(LangId._Description, "Description"),
        new(LangId._Type, "Type"),
        new(LangId._Version, "Version"),
        new(LangId._Author, "Author"),
        new(LangId._View, "View"),
        new(LangId._GetHelp, "Get help"),
        new(LangId._GetMoreTools, "Get more tools…"),
        new(LangId._Start, "Start"),
        new(LangId._Register, "Register"),
        new(LangId._Unregister, "Unregister"),
        new(LangId._ClearCache, "Clear cache"),
        new(LangId._ClearCache_Success, "Cache are cleared"),
        new(LangId._ClearCache_Error, "Could not clear the cache"),

        new(LangId._UnhandledException, "Unhandled exception"), // v9.0
        new(LangId._UnhandledException_Description, "Unhandled exception has occurred. If you click Continue, the application will ignore this error and attempt to continue. If you click Quit, the application will close immediately."), // v9.0
        new(LangId._DoNotShowThisMessageAgain, "Do not show this message again"), // v9.0
        new(LangId._IncompatibleConfig, "Incompatible settings file"),
        new(LangId._IncompatibleConfig_Description, "Your settings file was created by an older version of ImageGlass and is not compatible with this version. It will be reset to the default settings.\r\n\r\nDo you want to continue?"),
        new(LangId._IncompatibleConfig_BackupNote, "Please manually back up your settings file before continuing if you want to keep it. Choosing No will quit ImageGlass without changing the file."),
        new(LangId._IncompatibleTheme, "Incompatible theme packs"),
        new(LangId._IncompatibleTheme_Description, "Your theme packs were created for the previous version of ImageGlass and are not compatible with this version."),
        new(LangId._IncompatibleLanguage, "Incompatible language packs"),
        new(LangId._IncompatibleLanguage_Description, "Your language packs were created for the previous version of ImageGlass and are not compatible with this version."),
        new(LangId._CreatingFile, "Creating a temporary image file…"), //v9.0
        new(LangId._CreatingFileError, "Could not create temporary image file"), //v9.0
        new(LangId._NotSupported, "Unsupported format"), //v9.0

        new(LangId._InvalidAction, "Invalid action"), //v9.0
        new(LangId._InvalidAction_Transformation, "ImageGlass does not support rotation, flipping for this image."), //v9.0

        new(LangId._UserAction_Win32ExeError, "Cannot execute command '{0}'. Make sure the name is correct."), // v9.0

        // Gallery tooltip
        new(LangId._Metadata_FileSize, "File size"), //v9.0
        new(LangId._Metadata_FileLastWriteTime, "Date modified"), //v9.0
        new(LangId._Metadata_FrameCount, "Frames"), //v9.0
        new(LangId._Metadata_ExifRatingPercent, "Rating"), //v9.0
        new(LangId._Metadata_ColorSpace, "Color space"), //v9.0
        new(LangId._Metadata_ColorProfile, "Color profile"), //v9.0
        new(LangId._Metadata_ExifDateTime, "EXIF: DateTime"), //v9.0
        new(LangId._Metadata_ExifDateTimeOriginal, "EXIF: DateTimeOriginal"), //v9.0

        // image info
        new(LangId._ImageInfo_ListCount, "{0} file(s)"), //v9.0
        new(LangId._ImageInfo_FrameCount, "{0} frame(s)"), //v9.0

        // layout position
        new(LangId._Position_Left, "Left"),
        new(LangId._Position_Right, "Right"),
        new(LangId._Position_Top, "Top"),
        new(LangId._Position_Bottom, "Bottom"),

        // validation
        new(LangId._Validation_Required, "Required"),
        new(LangId._Validation_RegexPattern, "Invalid value"),
        new(LangId._Validation_IntValueOnly, "Must be an integer"),
        new(LangId._Validation_UnsignedIntValueOnly, "Must be a non-negative integer"),
        new(LangId._Validation_FloatValueOnly, "Must be a number"),
        new(LangId._Validation_UnsignedFloatValueOnly, "Must be a non-negative number"),
        new(LangId._Validation_FileNameValueOnly, "Invalid filename"),
        new(LangId._Validation_FilePathValueOnly, "Invalid file path"),
        new(LangId._Validation_FileExtensionValueOnly, "Must be a single file extension (e.g. .psd)"),
        new(LangId._Validation_FileExtensionsValueOnly, "Must be file extensions separated by ';' (e.g. .jpg;.png)"),


        // main window
        new(LangId._PicMain_ErrorText, "Could not load this image"), // v2.0 beta, updated 4.0, 9.0, 10.0
        new(LangId._OpenFileDialog, "All supported files"),
        new(LangId._Loading, "Loading…"), // v3.0
        new(LangId._ReachedFirstImage, "Reached the first image"), // v4.0
        new(LangId._ReachedLastImage, "Reached the last image"), // v4.0
        new(LangId._SwitchedToNextFolder, "Switched to next folder\r\n{0}"), // v10.0
        new(LangId._SwitchedToPreviousFolder, "Switched to previous folder\r\n{0}"), // v10.0
        new(LangId._ClipboardImage, "Clipboard image"), //v9.0

        // about
        new(LangId._Slogan, "A Fast, Seamless Photo Viewer"),
        new(LangId._AboutVersion, "Version:"),
        new(LangId._License, "Software license"),
        new(LangId._Privacy, "Privacy policy"),
        new(LangId._Homepage, "Homepage"),
        new(LangId._Credits, "Credits"),
        new(LangId._Donate, "Donate"),

        // slideshow
        new(LangId._PauseSlideshow, "Slideshow is paused." ), // v9.0
        new(LangId._ResumeSlideshow, "Slideshow is resumed." ), // v9.0
        new(LangId._MnuPauseResumeSlideshow, "Pause/resume slideshow" ), // v9.0
        new(LangId._MnuToggleCountdown, "Show slideshow countdown" ), // v9.0
        new(LangId._MnuExitSlideshow, "Exit slideshow" ), // v9.0

        // export frames
        new(LangId._Title, "Export image frames" ), //v9.0
        new(LangId._FolderPickerTitle, "Select output folder for exporting image frames" ), //v9.0
        new(LangId._Exporting, "Exporting {0}/{1} frames \r\n{2}…" ), //v9.0
        new(LangId._ExportDone, "Exported {0} frames successfully to \r\n{1}" ), //v9.0
        new(LangId._OpenOutputFolder, "Open output folder" ), //v9.0

        #endregion // General
    
        
        #region Enums

        // ImageOrderBy
        new(LangId.ImageOrderBy_Name, "Name (default)"), //v8.0
        new(LangId.ImageOrderBy_Random, "Random"), //v8.0
        new(LangId.ImageOrderBy_FileSize, "File size"), //v8.0
        new(LangId.ImageOrderBy_Extension, "Extension"), //v8.0
        new(LangId.ImageOrderBy_DateCreated, "Date created"), //v8.0
        new(LangId.ImageOrderBy_DateAccessed, "Date accessed"), //v8.0
        new(LangId.ImageOrderBy_DateModified, "Date modified"), //v8.0
        new(LangId.ImageOrderBy_ExifDateTaken, "EXIF: Date taken"), //v9.0
        new(LangId.ImageOrderBy_ExifRating, "EXIF: Rating"), //v9.0


        // ImageOrderType
        new(LangId.ImageOrderType_Asc, "Ascending"),  //v8.0
        new(LangId.ImageOrderType_Desc, "Descending"),  //v8.0

        // BrowsingMode
        new(LangId.BrowsingMode_Turbo, "Turbo"),
        new(LangId.BrowsingMode_Turbo_Description, "Fastest. Images may be skipped while you browse quickly."),
        new(LangId.BrowsingMode_Sequential, "Sequential"),
        new(LangId.BrowsingMode_Sequential_Description, "Never skips. Each image is fully shown before the next."),

        // AfterEditAppAction
        new(LangId.AfterEditAppAction_Nothing, "Nothing"), //v8.0
        new(LangId.AfterEditAppAction_Minimize, "Minimize"), //v8.0
        new(LangId.AfterEditAppAction_Close, "Close"), //v8.0

        // ColorProfileOption
        new(LangId.ColorProfileOption_None, "None"),
        new(LangId.ColorProfileOption_CurrentMonitorProfile, "Current monitor profile"),
        new(LangId.ColorProfileOption_Custom, "Custom…"),

        // BackdropStyle
        new(LangId.BackdropStyle_None, "None"),

        // MouseWheelEvent
        new(LangId.MouseWheelEvent_Scroll, "Scroll"),
        new(LangId.MouseWheelEvent_CtrlAndScroll, "Hold Ctrl and scroll"),
        new(LangId.MouseWheelEvent_ShiftAndScroll, "Hold Shift and scroll"),
        new(LangId.MouseWheelEvent_AltAndScroll, "Hold Alt and scroll"),

        // MouseWheelAction
        new(LangId.MouseWheelAction_DoNothing, "Do nothing"),
        new(LangId.MouseWheelAction_Zoom, "Zoom in / out"),
        new(LangId.MouseWheelAction_PanVertically, "Pan up / down"),
        new(LangId.MouseWheelAction_PanHorizontally, "Pan left / right"),
        new(LangId.MouseWheelAction_BrowseImages, "View next / previous Image"),

        // MouseClickEvent
        new(LangId.MouseClickEvent_LeftClick, "Left click"),
        new(LangId.MouseClickEvent_LeftDoubleClick, "Left double-click"),
        new(LangId.MouseClickEvent_RightClick, "Right click"),
        new(LangId.MouseClickEvent_WheelClick, "Wheel click"),
        new(LangId.MouseClickEvent_XButton1Click, "Back button (XButton1) click"),
        new(LangId.MouseClickEvent_XButton2Click, "Forward button (XButton2) click"),

        // CheckerboardType
        new(LangId.CheckerboardType_None, "None"),
        new(LangId.CheckerboardType_Client, "Entire viewer"),
        new(LangId.CheckerboardType_Image, "Image region only"),

        #endregion // Enums


        #region Main Window
        new(LangId.Menu_MnuMain, "Main menu"), // v3.0
        new(LangId.Menu_MnuToolbarOverflow, "View more buttons"), // v10.0


        #region Main Window > Main Menu

        #region Main Menu > File
        new(LangId.Menu_MnuFile, "File"), //v7.0
        new(LangId.Menu_MnuOpenFile, "Open file…"), //v3.0
        new(LangId.Menu_MnuNewWindow, "Open new window"), //v7.0
        new(LangId.Menu_MnuNewWindow_Error, "Cannot open new window because only one instance is allowed"), //v7.0
        new(LangId.Menu_MnuSave, "Save"), //v8.1
        new(LangId.Menu_MnuSave_Confirm, "Are you sure you want to override this image?"), //v9.0
        new(LangId.Menu_MnuSave_ConfirmDescription, "ImageGlass is not a professional photo editor, please be aware of losing quality, metadata, layers,… when saving your image."), //v9.0
        new(LangId.Menu_MnuSave_Saving, "Saving image…"), //v9.0
        new(LangId.Menu_MnuSave_Success, "Image is saved"), //v9.0
        new(LangId.Menu_MnuSave_Error, "Could not save image"), //v9.0
        new(LangId.Menu_MnuSaveAs, "Save as…"), //v3.0
        new(LangId.Menu_MnuExportFrames, "Export image frames…"), //v7.5

        new(LangId.Menu_MnuOpenWith, "Open with…"), //v7.6
        new(LangId.Menu_MnuEdit, "Edit image {0}…"), //v3.0,
        new(LangId.Menu_MnuEdit_AppNotFound, "Could not find the associated app for editing. You can assign an app for editing this format in ImageGlass Settings > Edit."), //v9.0
        new(LangId.Menu_MnuPrint, "Print…"), //v3.0
        new(LangId.Menu_MnuPrint_Error, "Could not print image"), //v9.0
        new(LangId.Menu_MnuShare, "Share…"), //v8.6
        new(LangId.Menu_MnuShare_Error, "Could not open Share dialog."), //v9.0
        new(LangId.Menu_MnuOpenLocation, "Open image location"), //v3.0

        new(LangId.Menu_MnuRename, "Rename image…"), //v3.0
        new(LangId.Menu_MnuRename_Description, "Enter a new filename:"), // v9.0
        new(LangId.Menu_MnuMoveToRecycleBin, "Move to Recycle Bin"), //v3.0
        new(LangId.Menu_MnuMoveToRecycleBin_Description, "Do you want to move this file to Recycle bin?"), //v3.0
        new(LangId.Menu_MnuDeleteFromHardDisk, "Delete permanently"), //v3.0
        new(LangId.Menu_MnuDeleteFromHardDisk_Description, "Are you sure you want to permanently delete this file?"), //v3.0
        #endregion // Main Menu > File

        #region Main Menu > Navigation
        new(LangId.Menu_MnuNavigation, "Navigation"), //v3.0
        new(LangId.Menu_MnuViewNext, "View next image"), //v3.0
        new(LangId.Menu_MnuViewPrevious, "View previous image"), //v3.0

        new(LangId.Menu_MnuGoTo, "Go to…"), //v3.0
        new(LangId.Menu_MnuGoTo_Description, "Type image number to view, and then press ENTER"),
        new(LangId.Menu_MnuGoToFirst, "Go to first image"), //v3.0
        new(LangId.Menu_MnuGoToLast, "Go to last image"), //v3.0

        new(LangId.Menu_MnuViewNextFrame, "View next frame"),
        new(LangId.Menu_MnuViewPreviousFrame, "View previous frame"),
        new(LangId.Menu_MnuViewFirstFrame, "View first frame"),
        new(LangId.Menu_MnuViewLastFrame, "View last frame"),
        #endregion // Main Menu > Navigation

        #region Main Menu > Zoom
        new(LangId.Menu_MnuZoom, "Zoom"), //v7.0
        new(LangId.Menu_MnuZoomIn, "Zoom in"), //v3.0
        new(LangId.Menu_MnuZoomOut, "Zoom out"), //v3.0
        new(LangId.Menu_MnuCustomZoom, "Custom zoom…"), // v8.3
        new(LangId.Menu_MnuCustomZoom_Description, "Enter a new zoom value"), // v8.3
        new(LangId.Menu_MnuScaleToFit, "Scale to fit"), //v3.5
        new(LangId.Menu_MnuScaleToFill, "Scale to fill"), //v7.5
        new(LangId.Menu_MnuActualSize, "Actual size"), //v3.0
        new(LangId.Menu_MnuLockZoom, "Lock zoom ratio"), //v3.0
        new(LangId.Menu_MnuAutoZoom, "Auto zoom"), //v5.5
        new(LangId.Menu_MnuScaleToWidth, "Scale to width"), //v3.0
        new(LangId.Menu_MnuScaleToHeight, "Scale to height"), //v3.0
        #endregion // Main Menu > Zoom

        #region Main Menu > Panning
        new(LangId.Menu_MnuPanning, "Panning"), //v9.0

        new(LangId.Menu_MnuPanLeft, "Pan image left"), //v9.0
        new(LangId.Menu_MnuPanRight, "Pan image right"), //v9.0
        new(LangId.Menu_MnuPanUp, "Pan image up"), //v9.0
        new(LangId.Menu_MnuPanDown, "Pan image down"), //v9.0

        new(LangId.Menu_MnuPanToLeftSide, "Pan image to left edge"), //v9.0
        new(LangId.Menu_MnuPanToRightSide, "Pan image to right edge"), //v9.0
        new(LangId.Menu_MnuPanToTop, "Pan image to top"), //v9.0
        new(LangId.Menu_MnuPanToBottom, "Pan image to bottom"), //v9.0
        #endregion // Main Menu > Panning

        #region Main Menu > Image
        new(LangId.Menu_MnuImage, "Image"), //v7.0

        new(LangId.Menu_MnuRefresh, "Refresh"), //v3.0
        new(LangId.Menu_MnuReload, "Reload image"), //v5.5
        new(LangId.Menu_MnuReloadImageList, "Reload image list"), //v7.0
        new(LangId.Menu_MnuUnload, "Unload image"), //v9.0

        new(LangId.Menu_MnuViewChannels, "View channels"), //v7.0
        new(LangId.Menu_MnuLoadingOrders, "Loading orders"), //v8.0
        new(LangId.Menu_MnuInvertColors, "Invert colors"), // v9.3
        new(LangId.Menu_MnuToggleImageAnimation, "Start / stop animating image"), //v3.0

        new(LangId.Menu_MnuRotateLeft, "Rotate left"), //v7.5
        new(LangId.Menu_MnuRotateRight, "Rotate right"), //v7.5
        new(LangId.Menu_MnuFlipHorizontal, "Flip Horizontal"), // V6.0
        new(LangId.Menu_MnuFlipVertical, "Flip Vertical"), // V6.0
        
        new(LangId.Menu_MnuSetDesktopBackground, "Set as Desktop background"), //v3.0
        new(LangId.Menu_MnuSetDesktopBackground_Error, "Could not set image as desktop background"), // v6.0
        new(LangId.Menu_MnuSetDesktopBackground_Success, "Desktop background is updated"), // v6.0
        new(LangId.Menu_MnuSetLockScreen, "Set as Lock screen image"), // V6.0
        new(LangId.Menu_MnuSetLockScreen_Error, "Could not set image as lock screen image"), // v6.0
        new(LangId.Menu_MnuSetLockScreen_Success, "Lock screen image is updated"), // v6.0

        new(LangId.Menu_MnuImageProperties, "Image properties"), //v3.0
        #endregion // Main Menu > Image

        #region Main Menu > Clipboard
        new(LangId.Menu_MnuClipboard, "Clipboard"), //v3.0
        new(LangId.Menu_MnuCopyFile, "Copy file"), //v3.0
        new(LangId.Menu_MnuCopyFile_Success, "Copied {0} file(s)"), // v2.0 final
        new(LangId.Menu_MnuCopyImagePixels, "Copy image pixels"), //v5.0
        new(LangId.Menu_MnuCopyImagePixels_Copying, "Copying image pixels. It's going to take a while…"), // v9.0
        new(LangId.Menu_MnuCopyImagePixels_Success, "Copied image pixels"), // v5.0
        new(LangId.Menu_MnuCutFile, "Cut file"), //v3.0
        new(LangId.Menu_MnuCutFile_Success, "Cut {0} file(s)"), // v2.0 final
        new(LangId.Menu_MnuCopyPath, "Copy image path"), //v3.0
        new(LangId.Menu_MnuCopyPath_Success, "Copied image path"), // v9.0
        new(LangId.Menu_MnuPasteImage, "Paste image"), //v3.0
        new(LangId.Menu_MnuClearClipboard, "Clear clipboard"), //v3.0
        new(LangId.Menu_MnuClearClipboard_Success, "Cleared clipboard"), // v2.0 final
        #endregion // Main Menu > Clipboard

        new(LangId.Menu_MnuWindowFit, "Window Fit"), //v7.5
        new(LangId.Menu_MnuFullScreen, "Full Screen"), //v3.0
        new(LangId.Menu_MnuFrameless, "Frameless"), //v7.5
        new(LangId.Menu_MnuFrameless_EnableDescription, "Drag the top area to move the window"), // v7.5
        new(LangId.Menu_MnuSlideshow, "Slideshow"), //v3.0

        #region Main Menu > Layout
        new(LangId.Menu_MnuLayout, "Layout"), //v3.0
        new(LangId.Menu_MnuToggleToolbar, "Toolbar"), //v3.0
        new(LangId.Menu_MnuToggleGallery, "Gallery panel"), //v3.0
        new(LangId.Menu_MnuToggleCheckerboard, "Checkerboard background"), //v3.0, updated v5.0
        new(LangId.Menu_MnuToggleTopMost, "Keep window always on top"), //v3.2
        new(LangId.Menu_MnuToggleTopMost_Enable, "Enabled window always on top"), // v9.0
        new(LangId.Menu_MnuToggleTopMost_Disable, "Disabled window always on top"), // v9.0
        new(LangId.Menu_MnuChangeBackgroundColor, "Change background color…"), // v9.0
        #endregion // Main Menu > Layout

        #region Main Menu > Tools
        new(LangId.Menu_MnuTools, "Tools"), //v3.0
        new(LangId.Menu_MnuColorPicker, "Color picker"), //v5.0
        new(LangId.Menu_MnuCropTool, "Crop image"), // v7.6
        new(LangId.Menu_MnuResizeTool, "Resize image"), // v9.2
        new(LangId.Menu_MnuFrameNav, "Frame navigation"), // v7.5
        new(LangId.Menu_MnuHdrToneMapper, "HDR tone mapper"), // v10.0
        new(LangId.Menu_MnuToolsSettings, "Tools settings…"),

        new(LangId.Menu_MnuLosslessCompression, "Magick.NET Lossless Compression"), // v9.1
        new(LangId.Menu_MnuLosslessCompression_Confirm, "Are you sure you want to proceed?"), // v9.1
        new(LangId.Menu_MnuLosslessCompression_Description, "This tool uses Magick.NET library for lossless compression, optimizing file size. Overwrites only if the compressed file is smaller than the original."), // v9.1
        new(LangId.Menu_MnuLosslessCompression_Compressing, "Performing lossless compression…"), // v9.1
        new(LangId.Menu_MnuLosslessCompression_Done, "Done lossless compression."), // v9.1
        new(LangId.Menu_MnuLosslessCompression_Error, "Could not compress the file."), // v10.0
        #endregion // Main Menu > Tools

        new(LangId.Menu_MnuSettings, "Settings"), // v3.0

        #region Main Menu > Help
        new(LangId.Menu_MnuHelp, "Help"), //v7.0
        new(LangId.Menu_MnuAbout, "About"), //v3.0

        new(LangId.Menu_MnuUpgradeLicense, "Upgrade to Pro"), // v10.0
        new(LangId.Menu_MnuUpgradeLicense_Description, "Get the most out of ImageGlass. Pro unlocks advanced tools, smoother workflows, and priority support."), // v10.0
        new(LangId.Menu_MnuUpgradeLicense_ImportLicense, "Import license file…"), // v10.0
        new(LangId.Menu_MnuUpgradeLicense_RetrieveFromEmail, "Retrieve from email…"), // v10.0
        new(LangId.Menu_MnuUpgradeLicense_BuyOnline, "Buy online…"), // v10.0
        new(LangId.Menu_MnuUpgradeLicense_ViewFeatures, "View Pro features"), // v10.0
        new(LangId.Menu_MnuUpgradeLicense_OutOfScope, "Your {0} license covers ImageGlass {1}, not ImageGlass {2}. Upgrade your license to turn Pro back on."), // v10.0
        new(LangId.Menu_MnuUpgradeLicense_Expired, "Your {0} license expired on {1}, and ImageGlass has switched to the Classic edition. Purchase a new license or import a renewal to reactivate Pro features."), // v10.0
        new(LangId.Menu_MnuUpgradeLicense_ExpiredTitle, "Your license is expired"), // v10.0
        new(LangId.Menu_MnuUpgradeLicense_SwitchToClassic, "Switch to Classic edition"), // v10.0

        new(LangId.Menu_MnuManageLicense, "Manage license"), // v10.0
        new(LangId.Menu_MnuManageLicense_ChangeLicense, "Change license file…"), // v10.0
        new(LangId.Menu_MnuManageLicense_UpgradePlan, "Upgrade plan…"), // v10.0
        new(LangId.Menu_MnuManageLicense_LicensedTo, "Licensed to"), // v10.0
        new(LangId.Menu_MnuManageLicense_Plan, "Plan"), // v10.0
        new(LangId.Menu_MnuManageLicense_Seats, "Seats"), // v10.0
        new(LangId.Menu_MnuManageLicense_Expires, "Valid until"), // v10.0
        new(LangId.Menu_MnuManageLicense_Perpetual, "Perpetual license"), // v10.0
        new(LangId.Menu_MnuManageLicense_LicenseId, "License ID"), // v10.0
        new(LangId.Menu_MnuManageLicense_Source, "Purchased from"), // v10.0
        new(LangId.Menu_MnuManageLicense_ImportSuccess, "License imported successfully. Restart ImageGlass now?"), // v10.0
        new(LangId.Menu_MnuManageLicense_ImportFailed, "Could not import this license file."), // v10.0
        new(LangId.Menu_MnuManageLicense_ImportExpired, "This license has expired and can no longer activate ImageGlass Pro."), // v10.0
        new(LangId.Menu_MnuManageLicense_LicenseFileType, "ImageGlass license file"), // v10.0
        new(LangId.Menu_MnuManageLicense_ExportLicense, "Export license to file…"), // v10.0
        new(LangId.Menu_MnuManageLicense_ExportNote, "Save a copy of your license file to unlock Pro in any other installations, on this or another computer. It covers the ImageGlass version line you bought."), // v10.0
        new(LangId.Menu_MnuManageLicense_ExportSuccess, "License file saved."), // v10.0
        new(LangId.Menu_MnuManageLicense_ExportFailed, "Could not save the license file."), // v10.0

        new(LangId.Menu_MnuQuickSetup, "Open ImageGlass Quick Setup"), //v9.0
        new(LangId.Menu_MnuReportIssue, "Report an issue…"), //v3.0

        new(LangId.Menu_MnuCheckForUpdate_NewVersion, "A new update is available!"), //v5.0
        new(LangId.Menu_MnuCheckForUpdate_NoUpdate, "You are using the latest version!"),
        new(LangId.Menu_MnuCheckForUpdate_Checking, "Checking for update…"),
        new(LangId.Menu_MnuCheckForUpdate_Failed, "Could not check for update!"),
        new(LangId.Menu_MnuCheckForUpdate_SkipVersion, "Skip this version"),
        new(LangId.Menu_MnuCheckForUpdate_CurrentVersion, "Current version: {0}" ), //v9.0
        new(LangId.Menu_MnuCheckForUpdate_LatestVersion, "The latest version: {0}" ), //v9.0
        new(LangId.Menu_MnuCheckForUpdate_PublishedDate, "Published date: {0}" ), //v9.0
        new(LangId.Menu_MnuCheckForUpdate_ReadyToInstall, "Update ready to install"), //v10.0
        new(LangId.Menu_MnuCheckForUpdate_Downloading, "Downloading the update…"), //v10.0
        new(LangId.Menu_MnuInstallUpdate, "Install update…"), //v10.0
        new(LangId.Menu_MnuInstallUpdate_Confirm, "ImageGlass will close and reopen to finish the update."), //v10.0

        new(LangId.Menu_MnuSetDefaultPhotoViewer, "Set default photo viewer"), //v9.0
        new(LangId.Menu_MnuSetDefaultPhotoViewer_Success, "You have successfully set ImageGlass as default photo viewer."), //v9.0
        new(LangId.Menu_MnuSetDefaultPhotoViewer_Error, "Could not set ImageGlass as default photo viewer."), //v9.0

        new(LangId.Menu_MnuRemoveDefaultPhotoViewer, "Remove default photo viewer"), //v9.0
        new(LangId.Menu_MnuRemoveDefaultPhotoViewer_Success, "ImageGlass is no longer the default photo viewer."), //v9.0
        new(LangId.Menu_MnuRemoveDefaultPhotoViewer_Error, "Could not remove ImageGlass as the default photo viewer."), //v9.0
        #endregion // Main Menu > Help

        new(LangId.Menu_MnuExit, "Exit"), //v7.0

        #endregion

        #endregion // Main Window

        
        #region Settings

        new(LangId.Settings_ResetSettings, "Reset settings"), // v9.1
        new(LangId.Settings_UnmanagedSettingReminder, "This is an unmanaged setting, it writes registry entries or files that are not cleaned up automatically. You will need to manually disable it before you uninstall the app."), // v9.1
        new(LangId.Settings_SearchPlaceholder, "Search settings…"), // v10.0
        new(LangId.Settings_AdminLockWarning, "Some settings are managed by your administrator and can't be changed here."), // v10.0
        new(LangId.Settings_ProFeatureHint, "This is an ImageGlass Pro feature."), // v10.0


        #region Settings > Navbar
        new(LangId.Settings_Nav_General, "General"),
        new(LangId.Settings_Nav_Image, "Image"),
        new(LangId.Settings_Nav_Slideshow, "Slideshow"),
        new(LangId.Settings_Nav_Edit, "Edit"),
        new(LangId.Settings_Nav_Viewer, "Viewer"),
        new(LangId.Settings_Nav_Toolbar, "Toolbar"),
        new(LangId.Settings_Nav_Gallery, "Gallery"),
        new(LangId.Settings_Nav_Layout, "Layout"),
        new(LangId.Settings_Nav_Mouse, "Mouse"),
        new(LangId.Settings_Nav_Keyboard, "Keyboard"),
        new(LangId.Settings_Nav_FileTypeAssociations, "File type associations"),
        new(LangId.Settings_Nav_Tools, "Tools"),
        new(LangId.Settings_Nav_Plugins, "Plugins"),
        new(LangId.Settings_Nav_Language, "Language"),
        new(LangId.Settings_Nav_Appearance, "Appearance"),
        #endregion // Settings > Navbar


        #region Settings > Tab General
        // General > General
        new(LangId.Settings_StartupDir, "Startup location"),
        new(LangId.Settings_ConfigDir, "Configuration location"),
        new(LangId.Settings_UserConfigFile, "User settings file (igconfig.json)"),

        // General > Startup
        new(LangId.Settings_Startup, "Startup"),
        new(LangId.Settings_EnableWelcomeImage, "Show welcome image"),
        new(LangId.Settings_EnableLastSeenImage, "Open the last seen image"),


        // General > Real-time update
        new(LangId.Settings_EnableFileWatcher, "Monitor file changes in the viewing folder and update in realtime"),
        new(LangId.Settings_EnableAutoOpenNewAddedImage, "Open the new added image automatically"),

        // General > App update
        new(LangId.Settings_AppUpdate, "App update"),
        new(LangId.Settings_AutoUpdate_Description, "The update check also includes a short anonymous message that helps improve ImageGlass. It contains no name, no account, and no identifier, and nothing about your files. Turning this off stops it completely."),
        new(LangId.Settings_SeeWhatIsSent, "See exactly what is sent…"),
        new(LangId.Settings_EnableAutoInstallUpdate, "Download and install updates automatically"), //v10.0

        // General > Others
        new(LangId.Settings_Others, "Others"),
        new(LangId.Settings_AutoUpdate, "Check for update automatically"),
        new(LangId.Settings_EnableMultiInstances, "Allow multiple instances of the program"),
        new(LangId.Settings_ShowAppIcon, "Show app icon on the title bar"),
        new(LangId.Settings_InAppMessageDuration, "In-app message duration (milliseconds)"),
        new(LangId.Settings_ImageInfoTags, "Image information tags"),
        new(LangId.Settings_AvailableImageInfoTags, "Available tags:"),
        #endregion // Settings > Tab General

            
        #region Settings > Tab Image
        // Image > Browsing
        new(LangId.Settings_Browsing, "Browsing"),
        new(LangId.Settings_ImageLoadingOrder, "Image loading order"),
        new(LangId.Settings_EnableExplorerSortOrder, "Use Explorer sort order"),
        new(LangId.Settings_EnableSubfoldersLoading, "Load images in subfolders"),
        new(LangId.Settings_EnableImageFolderGrouping, "Group images by directory"),
        new(LangId.Settings_EnableHiddenImagesLoading, "Load hidden images"),
        new(LangId.Settings_EnableLoopBackNavigation, "Loop back to the first image when reaching the end of the image list"),
        new(LangId.Settings_EnableAutoSwitchSiblingDir, "Switch to the sibling folder at the start/end of the list"),
        new(LangId.Settings_BrowsingMode, "Browsing mode"),
        new(LangId.Settings_EnableImagePreview, "Display image preview while it's being loaded"),

        new(LangId.Settings_ImagePreview, "Image preview"),
        new(LangId.Settings_EnableOnlyLoadRawPreview, "Load only the embedded thumbnail for RAW formats"),
        new(LangId.Settings_EnableOnlyLoadNonRawPreview, "Load only the embedded thumbnail for other formats"),
        new(LangId.Settings_MinEmbeddedThumbnailSize, "Minimum size of the embedded thumbnail to be loaded"),
        new(LangId.Settings_MinEmbeddedThumbnailSize_Width, "Width"),
        new(LangId.Settings_MinEmbeddedThumbnailSize_Height, "Height"),

        // Image > File watcher
        new(LangId.Settings_FileWatcher, "File watcher"),

        // Image > Caching
        new(LangId.Settings_Caching, "Caching"),
        new(LangId.Settings_ImageBoosterCacheMaxMemoryInMb, "Maximum memory used for caching images (in megabytes)"),
        new(LangId.Settings_ImageBoosterCacheMaxDimension, "Maximum image dimension to be cached (in pixels)"),
        new(LangId.Settings_ImageBoosterCacheMaxFileSizeInMb, "Maximum image file size to be cached (in megabytes)"),

        // Image > Color management
        new(LangId.Settings_ColorManagement, "Color management"),
        new(LangId.Settings_EnableHdrToneMapping, "Apply HDR tone mapping to HDR images"),
        new(LangId.Settings_EnableAlwaysApplyColorProfile, "Always apply for image without embedded color profile"),
        new(LangId.Settings_ColorProfile, "Color profile"),
        new(LangId.Settings_CurrentMonitorProfile_Description, "ImageGlass does not auto-update the color when moving its window between monitors"),
        #endregion // Settings > Tab Image


        #region Settings > Tab Slideshow
        // Slideshow > Appearance
        new(LangId.Settings_Slideshow_Appearance, "Appearance"),
        new(LangId.Settings_EnableSlideshowCountdown, "Show slideshow countdown"),
        new(LangId.Settings_EnableFullscreenSlideshow, "Start slideshow in Full Screen mode"),
        new(LangId.Settings_SlideshowBackgroundColor, "Slideshow background color"),

        // Slideshow > Playback
        new(LangId.Settings_Slideshow_Playback, "Playback"),
        new(LangId.Settings_EnableLoopSlideshow, "Loop back to the first image when reaching the end of the slideshow"),
        new(LangId.Settings_EnableSlideshowRandomInterval, "Use random interval"),
        new(LangId.Settings_SlideshowInterval, "Slideshow interval:"),
        new(LangId.Settings_SlideshowInterval_From, "From"),
        new(LangId.Settings_SlideshowInterval_To, "To"),

        new(LangId.Settings_SlideshowImagesToNotifySound, "Number of images to trigger a notification sound"),
        #endregion // Settings > Tab Slideshow


        #region Settings > Tab Edit
        // Edit > Saving
        new(LangId.Settings_Edit_Saving, "Saving"),
        new(LangId.Settings_EnableDeleteConfirmation, "Show confirmation dialog when deleting file"),
        new(LangId.Settings_EnableSaveConfirmation, "Show confirmation dialog when overriding file"),
        new(LangId.Settings_EnablePreserveModifiedDate, "Preserve the image's modified date on save"),
        new(LangId.Settings_EnableOpenSaveAsInCurrentFolder, "Open the Save As dialog in the current image directory"), // v9.1
        new(LangId.Settings_ImageEditQuality, "Image quality: {0}"),

        // Edit > Clipboard
        new(LangId.Settings_Clipboard, "Clipboard"),
        new(LangId.Settings_EnableCopyMultipleFiles, "Enable the copying of multiple files at once"),
        new(LangId.Settings_EnableCutMultipleFiles, "Enable the cutting of multiple files at once"),

        // Edit > Image editing apps
        new(LangId.Settings_AfterEditingAction, "After opening editing app"),
        new(LangId.Settings_EditApps, "Image editing apps"),
        new(LangId.Settings_EditApps_AppName, "App name"),
        new(LangId.Settings_EditAppDialog_AddApp, "Add an app for editing"),
        new(LangId.Settings_EditAppDialog_EditApp, "Edit app"),

        #endregion // Settings > Tab Edit


        #region Settings > Tab Layout
        // Layout > Window
        new(LangId.Settings_Window, "Window"),

        // Layout > Controls
        new(LangId.Settings_Controls, "Controls"),
        new(LangId.Settings_Layout_ArrangeHint, "Drag the toolbar and gallery onto a slot to rearrange them in the app window."),
        new(LangId.Settings_Layout_Viewer, "Viewer"),

        // Layout > Layout
        new(LangId.Settings_Layout_Toolbar, "Toolbar"),
        new(LangId.Settings_Layout_Gallery, "Gallery"),
        new(LangId.Settings_Layout_ToolbarPosition, "Toolbar position"),
        new(LangId.Settings_Layout_GalleryPosition, "Gallery position"),
        #endregion // Settings > Tab Layout


        #region Settings > Tab Viewer
        // Viewer > Appearance
        new(LangId.Settings_Appearance, "Appearance"),
        new(LangId.Settings_EnableNavigationButtons, "Show navigation arrow buttons"),
        new(LangId.Settings_EnableCenterWindowFit, "Automatically center the window in Window Fit mode"),
        new(LangId.Settings_EnableVectorRenderer, "Use the vector renderer for SVG images"),
        new(LangId.Settings_CheckerboardMode, "Checkerboard background"),

        // Viewer > Panning
        new(LangId.Settings_Panning, "Panning"),
        new(LangId.Settings_EnableFreePan, "Enable free panning"),
        new(LangId.Settings_PanMargin, "Panning margin: {0}"),
        new(LangId.Settings_PanSpeed, "Panning speed: {0}"),

        // Viewer > Zooming
        new(LangId.Settings_Zooming, "Zooming"),
        new(LangId.Settings_ImageInterpolation, "Image interpolation"),
        new(LangId.Settings_ImageInterpolation_ScaleDown, "When zoom < 100%"),
        new(LangId.Settings_ImageInterpolation_ScaleUp, "When zoom > 100%"),
        new(LangId.Settings_ZoomSpeed, "Zoom speed: {0}"),
        new(LangId.Settings_ZoomLevels, "Zoom levels"),
        new(LangId.Settings_UseSmoothZooming, "Use smooth zooming"),
        new(LangId.Settings_LoadDefaultZoomLevels, "Load default zoom levels"),
        #endregion // Settings > Tab Viewer


        #region Settings > Tab Toolbar
        // Toolbar > Toolbar
        new(LangId.Settings_Toolbar_ShowToolbarInFullscreen, "Show toolbar in Full Screen mode"),
        new(LangId.Settings_Toolbar_ToolbarIconHeight, "Toolbar icon size: {0}"),

        new(LangId.Settings_Toolbar_AddNewButton, "Add a custom toolbar button"),
        new(LangId.Settings_Toolbar_EditButton, "Edit toolbar button"),


        new(LangId.Settings_Toolbar_ToolbarButtons, "Toolbar buttons"),
        new(LangId.Settings_Toolbar_AddCustomButton, "Add a custom button…"),
        new(LangId.Settings_Toolbar_AvailableButtons, "Available buttons"),
        new(LangId.Settings_Toolbar_CurrentButtons, "Current buttons"),
        new(LangId.Settings_Toolbar_Errors_ButtonIdDuplicated, "A button with the ID '{0}' has already been defined. Please choose a different and unique ID for your button to avoid conflicts."),

        new(LangId.Settings_Toolbar_ButtonText, "Button text"),
        new(LangId.Settings_Toolbar_ShowButtonText, "Show text beside the icon"),
        new(LangId.Settings_Toolbar_AlignRight, "Place on the right side of the toolbar"),
        new(LangId.Settings_Toolbar_CustomIcon, "Custom…"),
        new(LangId.Settings_Toolbar_ConfigBinding, "Enable toggle binding"),
        new(LangId.Settings_Toolbar_ConfigBindingName, "Config name"),
        new(LangId.Settings_Toolbar_ConfigBindingValue, "Config value"),
        new(LangId.Settings_Toolbar_RecordHotkeyHint, "Click here, then press a hotkey"),
        new(LangId.Settings_Toolbar_BuiltInReadonly, "This is a built-in button, so its properties can't be changed."),

        new(LangId.Settings_Toolbar_ArrangeHint, "Drag to reorder items, or move them between lists to add and remove. Arrow keys can also be used for positioning."),

        #endregion // Settings > Tab Toolbar


        #region Settings > Tab Gallery
        // Gallery > Gallery
        new(LangId.Settings_ShowGalleryInFullscreen, "Show gallery in Full Screen mode"),
        new(LangId.Settings_ShowGalleryFileName, "Show thumbnail filename"),
        new(LangId.Settings_EnableGalleryShellThumbnail, "Use system shell for thumbnails"),
        new(LangId.Settings_ThumbnailSize, "Thumbnail size (in pixels): {0}"),
        new(LangId.Settings_GalleryCacheSizeInMb, "Maximum gallery cache size (in megabytes)"),
        new(LangId.Settings_GalleryColumns, "Number of thumbnail columns in vertical gallery layout: {0}"),
        #endregion // Settings > Tab Gallery


        #region Settings > Tab Mouse
        // Mouse > Mouse wheel action
        new(LangId.Settings_MouseWheelAction, "Mouse wheel action"),
        // Mouse > Mouse click action
        new(LangId.Settings_MouseClickAction, "Mouse click action"),
        #endregion // Settings > Tab Mouse


        #region Settings > Tab Keyboard
        new(LangId.Settings_Keyboard_MenuHotkeys, "Menu hotkeys"),
        new(LangId.Settings_Keyboard_Action, "Action"),
        new(LangId.Settings_Keyboard_NoResults, "No matching actions"),
        new(LangId.Settings_Keyboard_EditTitle, "Edit hotkeys"),
        new(LangId.Settings_Keyboard_Conflict, "This hotkey is assigned to more than one action."),
        #endregion // Settings > Tab Mouse & Keyboard


        #region Settings > Tab File type associations
        // File type associations > File extension icons
        new(LangId.Settings_FileExtensionIcons, "File extension icons"),
        new(LangId.Settings_FileExtensionIcons_Description, "For customizing file extension icons, download an icon pack, place all .ICO files in the extension icon folder, and click the '{0}' button."),
        new(LangId.Settings_OpenExtensionIconFolder, "Open extension icon folder"),
        new(LangId.Settings_GetExtensionIconPacks, "Get extension icon packs…"),

        // File type associations > Default photo viewer
        new(LangId.Settings_DefaultPhotoViewer, "Default photo viewer"),
        new(LangId.Settings_AppMenuEntry, "Applications menu"), //v10.0
        new(LangId.Settings_AppMenuEntry_Description, "Register ImageGlass in your applications menu, so you can launch it from there and set it as your default image viewer."), //v10.0
        new(LangId.Settings_AppMenuEntry_Success, "ImageGlass has been registered in your applications menu."), //v10.0
        new(LangId.Settings_AppMenuEntry_RemoveSuccess, "ImageGlass is no longer in your applications menu."), //v10.0
        new(LangId.Settings_AppMenuEntry_Error, "Could not update the applications menu."), //v10.0
        new(LangId.Settings_DefaultPhotoViewer_Description, "Register the supported formats of ImageGlass with Windows. You might need to open the Default apps settings and manually select ImageGlass from the list for it to take effect."),
        new(LangId.Settings_DefaultPhotoViewer_ScopePerMachine, "Scope: all user accounts on this computer (per-machine)."),
        new(LangId.Settings_DefaultPhotoViewer_ScopePerUser, "Scope: your user account only (per-user)."),
        new(LangId.Settings_OpenDefaultAppsSetting, "Open Default apps setting"),

        // File type associations > File formats
        new(LangId.Settings_FileFormats, "File formats"),
        new(LangId.Settings_TotalSupportedFormats, "Total supported formats: {0}"),
        new(LangId.Settings_AddNewFileExtension, "Add new file extension"),

        #endregion // Settings > Tab File type associations


        #region Settings > Tab Tools
        // Tools > Tools
        new(LangId.Settings_Tools_AddNewTool, "Add an external tool"),
        new(LangId.Settings_Tools_EditTool, "Edit external tool"),
        new(LangId.Settings_Tools_ToolLaunchFailed, "Could not launch \"{0}\""),
        new(LangId.Settings_Tools_ToolLaunchFailed_Description, "Do you want to update the tool again?"),
        new(LangId.Settings_Tools_Integrated, "Integrated"),
        new(LangId.Settings_Tools_IntegratedWith, "Integrated with {0}"),
        new(LangId.Settings_Tools_Errors_ToolIdDuplicated, "The tool ID \"{0}\" is already in use."),
        #endregion // Settings > Tab Tools


        #region Settings > Tab Plugins
        new(LangId.Settings_Plugins_OpenPluginFolder, "Open plugin folder"),
        new(LangId.Settings_Plugins_GetMorePlugins, "Get more plugins…"),
        new(LangId.Settings_Plugins_SupportedExtensions, "Supported extensions"),
        new(LangId.Settings_Plugins_ViewMetadata, "Plugin information"),
        new(LangId.Settings_Plugins_FolderPath, "Folder"),
        new(LangId.Settings_Plugins_InstallSuccess, "Plugin installed successfully"),
        new(LangId.Settings_Plugins_DeleteConfirm, "Delete this plugin? This permanently removes it from ImageGlass."),
        new(LangId.Settings_Plugins_Status, "Status"),
        new(LangId.Settings_Plugins_Enable, "Enable"),
        new(LangId.Settings_Plugins_TrustAndEnable, "Trust and enable"),
        new(LangId.Settings_Plugins_StatusEnabled, "Enabled"),
        new(LangId.Settings_Plugins_StatusDisabled, "Disabled"),
        new(LangId.Settings_Plugins_StatusUntrusted, "Not enabled"),
        new(LangId.Settings_Plugins_StatusChanged, "File changed"),
        new(LangId.Settings_Plugins_StatusNotLoaded, "Not loaded"),
        new(LangId.Settings_Plugins_TrustTitle, "Enable this plugin?"),
        new(LangId.Settings_Plugins_TrustPrompt, "\"{0}\" is a native plugin that runs inside ImageGlass with full access to your files and system. Only enable plugins you obtained from a source you trust."),
        new(LangId.Settings_Plugins_TrustChangedWarning, "This plugin's file has changed since it was last enabled. Only re-enable it if you updated the plugin yourself."),
        new(LangId.Settings_Plugins_EnableToLoad, "Enable the plugin below to load it."),
        new(LangId.Settings_Plugins_ExtensionsHint, "This plugin can open and save the file types below. Turn off any you would rather leave to ImageGlass or another plugin."),
        new(LangId.Settings_Plugins_FormatsAfterEnable, "Enable this plugin to see and choose the file types it supports."),
        new(LangId.Settings_Plugins_NoFormats, "No file types supported."),
        #endregion // Settings > Tab Plugins


        #region Settings > Tab Language
        // Language > Language
        new(LangId.Settings_DisplayLanguage, "Display language"),
        new(LangId.Settings_Refresh, "Refresh"),
        new(LangId.Settings_InstallNewLanguagePack, "Install new language packs…"),
        new(LangId.Settings_GetMoreLanguagePacks, "Get more language packs…"),
        new(LangId.Settings_ExportLanguagePack, "Export language pack…"),
        new(LangId.Settings_Contributors, "Contributors"),
        #endregion // Settings > Tab Language


        #region Settings > Tab Appearance
        // Appearance > Appearance
        new(LangId.Settings_WindowBackdrop, "Window backdrop"),
        new(LangId.Settings_BackgroundColor, "Viewer background color"),

        // Appearance > Theme
        new(LangId.Settings_Theme, "Theme"),
        new(LangId.Settings_DarkTheme, "Dark"),
        new(LangId.Settings_LightTheme, "Light"),
        new(LangId.Settings_Theme_OpenThemeFolder, "Open theme folder"),
        new(LangId.Settings_Theme_GetMoreThemes, "Get more theme packs…"),
        new(LangId.Settings_Theme_InstallTheme, "Install theme packs"),

        new(LangId.Settings_UseThemeForDarkMode, "Use this theme for dark mode"),
        new(LangId.Settings_UseThemeForLightMode, "Use this theme for light mode"),
        #endregion // Settings > Tab Appearance

        #endregion // Settings
        

        #region Tool: Crop
        new(LangId.Tool_Crop_LblAspectRatio, "Aspect ratio"), //v9.0
        new(LangId.Tool_Crop_LblLocation, "Location"), //v9.0
        new(LangId.Tool_Crop_LblSize, "Size"), //v9.0

        new(LangId.Tool_Crop_SelectionAspectRatio_FreeRatio, "Free ratio"), //v9.0
        new(LangId.Tool_Crop_SelectionAspectRatio_Custom, "Custom…"), //v9.0
        new(LangId.Tool_Crop_SelectionAspectRatio_Original, "Original"), //v9.0

        new(LangId.Tool_Crop_BtnReset, "Reset"), //v9.0
        new(LangId.Tool_Crop_BtnSave, "Save"), //v9.0
        new(LangId.Tool_Crop_BtnSaveAs, "Save as…"), //v9.0
        new(LangId.Tool_Crop_BtnCrop, "Crop"), //v9.0
        new(LangId.Tool_Crop_BtnCopy, "Copy"), //v9.0

        // Crop settings
        new(LangId.Tool_Crop_Title, "Crop settings"), //v9.0
        new(LangId.Tool_Crop_ChkCloseToolAfterSaving, "Close Crop tool after saving"), //v9.0
        new(LangId.Tool_Crop_LblDefaultSelection, "Default selection"), //v9.0
        new(LangId.Tool_Crop_ChkAutoCenterSelection, "Auto-center selection"), //v9.0

        new(LangId.Tool_Crop_DefaultSelectionType_UseTheLastSelection, "Use the last selection"), //v9.0
        new(LangId.Tool_Crop_DefaultSelectionType_SelectNone, "Select none"), //v9.0
        new(LangId.Tool_Crop_DefaultSelectionType_SelectX, "Select {0}"), //v9.0
        new(LangId.Tool_Crop_DefaultSelectionType_SelectAll, "Select all"), //v9.0
        new(LangId.Tool_Crop_DefaultSelectionType_CustomArea, "Custom area…"), //v9.0

        #endregion // Tool: Crop


        #region Tool: HDR tone mapper
        new(LangId.Tool_Hdr_LblMode, "Mode"), //v10.0
        new(LangId.Tool_Hdr_LblExposure, "Exposure: {0}"), //v10.0
        new(LangId.Tool_Hdr_LblReferenceWhite, "Reference white: {0} nits"), //v10.0
        new(LangId.Tool_Hdr_LblHighlightCompression, "Highlight compression: {0}"), //v10.0
        new(LangId.Tool_Hdr_LblSaturation, "Saturation: {0}"), //v10.0
        new(LangId.Tool_Hdr_BtnReset, "Reset"), //v10.0
        #endregion // Tool: HDR tone mapper


        #region Tool: Color picker


        // Color picker settings
        new(LangId.Tool_ColorPicker_Title, "Color picker settings"), //v9.0
        new(LangId.Tool_ColorPicker_ChkShowRgbA, "Use RGB format with alpha value"), //v5.0
        new(LangId.Tool_ColorPicker_ChkShowHexA, "Use HEX format with alpha value"), //v5.0
        new(LangId.Tool_ColorPicker_ChkShowHslA, "Use HSL format with alpha value"), //v5.0
        new(LangId.Tool_ColorPicker_ChkShowHsvA, "Use HSV format with alpha value"), //v8.0
        new(LangId.Tool_ColorPicker_ChkShowCmykA, "Use CMYK format with alpha value"), //v10.0
        new(LangId.Tool_ColorPicker_ChkShowCIELabA, "Use CIELAB format with alpha value"), //v9.0

        #endregion // Tool: Color picker


        #region Tool: Resizer
        new(LangId.Tool_Resizer_RadResizeByPixels, "Pixels" ), // v9.2
        new(LangId.Tool_Resizer_RadResizeByPercentage, "Percentage" ), // v9.2
        new(LangId.Tool_Resizer_ChkKeepRatio, "Keep ratio propotional" ), // v9.2
        new(LangId.Tool_Resizer_LblResample, "Resample:" ), // v9.2
        new(LangId.Tool_Resizer_LblCurrentSize, "Current Size:" ), // v9.2
        new(LangId.Tool_Resizer_LblNewSize, "New Size:" ), // v9.2
        #endregion // Tool: Resizer

        
        #region Quick setup

        new(LangId.QuickSetup_Title, "ImageGlass Quick Setup" ), //v10.0
        new(LangId.QuickSetup_StepInfo, "Step {0} of {1}" ), //v10.0
        new(LangId.QuickSetup_SkipAndLaunch, "Skip this and launch ImageGlass" ), //v10.0

        new(LangId.QuickSetup_SelectLanguage, "Display language" ), //v10.0
        new(LangId.QuickSetup_SeeWhatNew, "See what's new in this version…" ), //v10.0
        new(LangId.QuickSetup_SelectProfile, "Select a profile" ), //v10.0
        new(LangId.QuickSetup_StandardUser, "Standard user" ), //v10.0
        new(LangId.QuickSetup_ProfessionalUser, "Professional user" ), //v10.0
        new(LangId.QuickSetup_SettingsWillBeApplied, "Settings will be applied:" ), //v10.0
        new(LangId.QuickSetup_SettingProfileDescription, "To modify these settings later, simply open the app Settings." ), //v10.0

        new(LangId.QuickSetup_RegisterImageFormats, "Do you want to register the supported image formats of ImageGlass with Windows? You can unregister it later in Settings > File type associations." ), //v10.0

        new(LangId.QuickSetup_ConfirmCloseProcess, "Are you ready to apply the settings?" ), //v10.0
        new(LangId.QuickSetup_ConfirmCloseProcess_Description, "All other running instances of ImageGlass will be closed before the settings are saved." ), //v10.0

        #endregion // Quick setup


    ];

}


/// <summary>
/// Outcome of a language-pack install batch (installed count + names of the incompatible packs).
/// </summary>
public readonly record struct LangPackInstallResult(
    int InstalledCount,
    IReadOnlyList<string> IncompatiblePackNames);


