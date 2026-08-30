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
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.ServiceProviders.Licensing;
using ImageGlass.Common.ServiceProviders.Update;
using ImageGlass.Common.Types;
using ImageGlass.Plugins;
using ImageGlass.SDK.Plugins;
using ImageGlass.SDK.Tools;
using ImageGlass.Tools;
using ImageGlass.UI.Viewer;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common;

public static class Core
{
    public static readonly AppInstance AppInstance = new AppInstance("IG_APP"); // MacOS has length limit

    public static event EventHandler? LanguageChanged;
    public static event EventHandler<ThemePackChangedEventArgs>? ThemeChanged;
    public static event EventHandler<PhotoUnloadedEventArgs>? PhotoUnloaded;
    public static event EventHandler<PhotoSaveEventArgs>? PhotoSaved;
    public static event EventHandler<DestColorProfileChangedEventArgs>? ColorProfileChanged;

    private static string _initImagePathFromArgs = string.Empty;



    #region Platform Service Provider

    /// <summary>
    /// Gets the build information.
    /// </summary>
    /// <remarks>
    /// <c>**NOTE:</c> Set this from the platform project's <c>Program.Main()</c>
    /// using the MSBuild-generated <c>AppBuildInfo</c> class.
    /// </remarks>
    public static IAppBuildInfo BuildInfo { get; set; } = null!;


    /// <summary>
    /// Provides a singleton instance to detect color profile of monitor.
    /// </summary>
    public static IWindowColorProfileProvider? ColorProfileProvider { get; set; } = null;


    /// <summary>
    /// Provides a singleton instance to retrieve photo preview & thumbnail.
    /// </summary>
    public static IPhotoPreviewProvider PreviewProvider { get; set; } = null!;


    /// <summary>
    /// Provides a singleton instance to search photo files.
    /// </summary>
    public static IFileSearchProvider FileSearchProvider { get; set; } = null!;


    /// <summary>
    /// Provides a singleton instance to manage OS shell & file path.
    /// </summary>
    public static IShellProvider? ShellProvider { get; set; } = null;


    /// <summary>
    /// Provides a singleton instance to manage Print service.
    /// </summary>
    public static IPrintProvider PrintProvider { get; set; } = null!;


    /// <summary>
    /// Provides an optional OS-level verifier for the identity of a tool process connecting
    /// to the host IPC pipe. Windows-only; <c>null</c> elsewhere (the pipe's
    /// <c>CurrentUserOnly</c> restriction is the cross-platform baseline).
    /// </summary>
    public static IPipeSecurityProvider? PipeSecurityProvider { get; set; } = null;


    /// <summary>
    /// Provides the Pro entitlement granted by the platform app store this build came from, e.g.
    /// the Microsoft Store. <c>null</c> for a self-distributed build, which is every platform that
    /// has no store channel yet. Must be registered before
    /// <see cref="App.InitializeAppInstance"/>, because the license is resolved there, before the
    /// other service providers are installed.
    /// </summary>
    public static IStoreEntitlementProvider? StoreEntitlementProvider { get; set; } = null;


    /// <summary>
    /// Provides a singleton instance to access app APIs.
    /// </summary>
    public static AppAPIProvider API { get; set; } = null!;


    /// <summary>
    /// Provides the slideshow service for managing slideshow playback.
    /// </summary>
    public static SlideshowProvider? Slideshow { get; set; } = null;


    /// <summary>
    /// Provides the update service for checking and downloading app updates.
    /// </summary>
    public static UpdateProvider Update { get; set; } = null!;


    /// <summary>
    /// Installs a downloaded update in place; <c>null</c> on a channel that cannot update itself.
    /// </summary>
    public static IAppUpdateInstaller? UpdateInstaller { get; set; } = null;


    /// <summary>
    /// Whether a verified update package is downloaded and waiting to be installed.
    /// </summary>
    public static bool HasPendingUpdate => !string.IsNullOrWhiteSpace(Config?.UpdatePendingVersion);


    /// <summary>
    /// Singleton loader that owns every native plugin shared library for the process lifetime.
    /// Also exposes <see cref="PluginRegistry.FailureManager"/> for quarantine bookkeeping.
    /// </summary>
    public static PluginRegistry PluginRegistry { get; } = new();


    /// <summary>
    /// Gets the central registry for all tools (built-in, hosted, and external).
    /// Owns <see cref="ToolRegistry.ExternalTools"/> for external tool process management.
    /// </summary>
    public static ToolRegistry ToolRegistry { get; } = new();


    /// <summary>
    /// Gets the registry for built-in and future external photo codecs.
    /// </summary>
    public static CodecRegistry CodecRegistry { get; } = new();


    #endregion // Platform Service Provider



    #region Public Properties

    /// <summary>
    /// Gets the app settings.
    /// </summary>
    public static Config Config { get; set; } = new();


    /// <summary>
    /// Gets the arguments passed to the application.
    /// </summary>
    public static string[] Args { get; set; } = [];


    /// <summary>
    /// Gets the photo manager.
    /// </summary>
    public static PhotoManager Photos { get; set; } = new();


    /// <summary>
    /// Gets, sets app busy state.
    /// </summary>
    public static bool IsBusy { get; set; } = false;


    /// <summary>
    /// Gets, sets the current color mode of OS.
    /// </summary>
    public static bool IsSystemDarkMode { get; set; } = true;


    /// <summary>
    /// Gets the app accent color derived from the theme/system accent (used to build accent resources).
    /// </summary>
    public static Color AccentColor { get; private set; } = new();


    /// <summary>
    /// Gets the current app theme pack.
    /// </summary>
    public static IgTheme Theme { get; private set; } = new();


    /// <summary>
    /// Gets, sets the current language.
    /// </summary>
    public static Lang Lang
    {
        get; set
        {
            if (field == value) return;

            field = value;
            Core.OnLanguageChanged();
        }
    } = new();


    /// <summary>
    /// Gets or sets the HDR tone mapping options used when rendering high dynamic range images.
    /// </summary>
    public static HdrToneMappingOptions HdrToneMappingConfig { get; set; } = new();


    /// <summary>
    /// The active, signature-verified Pro license, or null when running as Classic.
    /// </summary>
    public static LicenseInfo? AppLicense { get; set; }


    /// <summary>
    /// A signature-verified license that does not cover this app version, or null. Kept so the
    /// upgrade prompt can name the license the user already owns instead of pitching generically.
    /// </summary>
    public static LicenseInfo? OutOfScopeLicense { get; set; }


    /// <summary>
    /// Whether Pro features are unlocked (a valid license is active).
    /// </summary>
    public static bool IsProEnabled => AppLicense is not null;


    /// <summary>
    /// Gets the resolved path of the image file from the arguments.
    /// </summary>
    public static string InputImagePathFromArgs => _initImagePathFromArgs;


    /// <summary>
    /// Gets, sets the changes of the current viewing image.
    /// </summary>
    public static PhotoTransform ImageTransform { get; set; } = new();


    /// <summary>
    /// Gets, sets copied filename collection (multi-copy).
    /// </summary>
    public static HashSet<string> StringClipboard { get; set; } = [];


    /// <summary>
    /// Gets, sets the clipboard photo.
    /// </summary>
    public static Photo? ClipboardImage { get; set; }


    /// <summary>
    /// Gets, sets the path of the temporary image (clipboard image, temp image for printing, background,...)
    /// </summary>
    public static string? TempImagePath { get; set; }


    /// <summary>
    /// Gets the color profile of current photo.
    /// </summary>
    public static SKColorSpace? DestColorProfile { get; private set; }


    /// <summary>
    /// Checks if the current color profile is supported by Skia.
    /// </summary>
    public static bool IsDestColorProfileSupported { get; private set; } = true;


    /// <summary>
    /// Gets the color channels setting.
    /// </summary>
    public static ColorChannels ColorChannels { get; set; } = ColorChannels.RGBA;

    #endregion // Public Properties



    #region Public Methods


    /// <summary>
    /// Disposes all singletons.
    /// </summary>
    public static void Dispose()
    {
        Config.CleanUpPropertyChangedEvents();

        // Dispose tools (StopAllAsync already called in OnClosing for external tools)
        ToolRegistry.Dispose();
        PluginRegistry.Dispose();
        CodecRegistry.Dispose();

        DisposeClipboardPhoto();

        Core.Slideshow?.Dispose();
        Core.Slideshow = null;

        Core.Photos.Dispose();
        Core.ColorProfileProvider?.Dispose();
        Core.ColorProfileProvider = null;

        Core.FileSearchProvider?.Dispose();
        Core.FileSearchProvider = null!;

        Core.ShellProvider?.Dispose();
        Core.ShellProvider = null;
    }


    /// <summary>
    /// The background plugin-discovery task; completes once startup discovery finishes. Awaited
    /// before opening a file whose type isn't yet supported, so a still-loading codec plugin can claim it.
    /// </summary>
    public static Task PluginDiscoveryTask { get; private set; } = Task.CompletedTask;


    /// <summary>
    /// Discovers native plugins from the <c>_plugins</c> directory and registers their codecs.
    /// Runs on a background thread to avoid blocking app startup.
    /// </summary>
    public static void DiscoverPlugins()
    {
        PluginDiscoveryTask = Task.Run(() =>
        {
            StartupTrace.Mark("plugins:discover:begin");
            var pluginsDir = BHelper.ConfigDir(Dir.Plugins);

            // reap stashed installs before any library loads
            PluginRegistry.CleanupTrashDirs(pluginsDir);

            var discovered = PluginRegistry.DiscoverManifests(pluginsDir);

            // plugin codec exts; NOT persisted (browsable only while the plugin is loaded)
            var pluginExtensions = new List<string>();

            foreach (var (manifest, dir) in discovered)
            {
                try
                {
                    RegisterPluginCodecs(manifest, dir, pluginExtensions);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Core.DiscoverPlugins] '{manifest.Id}' failed: {ex.Message}");
                }
            }

            StartupTrace.Mark("plugins:discover:end");

            // purge legacy plugin exts from config; no list reload (races the initial open)
            if (pluginExtensions.Count > 0)
            {
                Dispatcher.UIThread.Post(() => Config.PurgePluginFileFormats(pluginExtensions));
            }

            StartupTrace.Flush();
        });
    }


    // Loaded plugin id -> its registered proxies, so a hot-disable can unregister exactly those.
    private static readonly Dictionary<string, List<NativeCodecProxy>> _pluginProxies = new(StringComparer.Ordinal);
    private static readonly Lock _pluginProxiesLock = new();


    /// <summary>
    /// Loads one plugin, registers its codecs, tracks the proxies for hot-unload, and collects their
    /// extensions. Returns <c>true</c> if any codec registered. Thread-safe; may run off the UI thread.
    /// </summary>
    private static bool RegisterPluginCodecs(PluginManifest manifest, string dir, List<string> pluginExtensions)
    {
        var handle = PluginRegistry.LoadAndProbe(manifest, dir);
        if (handle is null) return false;

        return RegisterProxies(handle, pluginExtensions);
    }


    /// <summary>
    /// Builds and registers proxies for an already-loaded plugin. <paramref name="declaredExtensions"/>
    /// collects the pre-filter extensions, which is the set <see cref="Config.PurgePluginFileFormats"/>
    /// needs. Returns <c>true</c> if any codec registered.
    /// </summary>
    private static bool RegisterProxies(NativePlugin handle, List<string> declaredExtensions)
    {
        var proxies = new List<NativeCodecProxy>();
        foreach (var proxy in PluginRegistry.CreateProxies(handle))
        {
            // Everything switched off: registering it would leave an empty-extension codec that
            // the catch-all convention mistakes for the fallback decoder.
            if (proxy.DecodingExtensions.Count == 0 && proxy.EncodingExtensions.Count == 0)
            {
                declaredExtensions.AddRange(proxy.DeclaredDecodingExtensions);
                proxy.Dispose();
                continue;
            }

            try
            {
                CodecRegistry.Register(proxy);
                proxies.Add(proxy);
                declaredExtensions.AddRange(proxy.DeclaredDecodingExtensions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Core.RegisterProxies] register '{proxy.CodecId}' failed: {ex.Message}");
                proxy.Dispose();
            }
        }

        if (proxies.Count == 0) return false;

        lock (_pluginProxiesLock)
        {
            _pluginProxies[handle.PluginId] = proxies;
        }
        return true;
    }


    /// <summary>
    /// Rebuilds a loaded plugin's codec proxies in place so a change to its per-extension choices
    /// takes effect without a restart. Reuses the probed capabilities: no library load, no re-hash.
    /// UI thread only (writes <see cref="Config.FileFormats"/>); no-op when not loaded.
    /// </summary>
    public static void ReloadPluginCodecs(string pluginId)
    {
        var handle = PluginRegistry.GetLoadedPlugin(pluginId);
        if (handle is null) return;

        // Cached photos hold pixels and metadata from the codec that is about to stop winning.
        // Also cancels in-flight prefetch, so nothing decodes against a proxy we are disposing.
        Photos.ClearCache();

        List<NativeCodecProxy>? old;
        lock (_pluginProxiesLock)
        {
            _pluginProxies.Remove(pluginId, out old);
        }

        var oldDecodeExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (old is not null)
        {
            foreach (var proxy in old)
            {
                foreach (var ext in proxy.DecodingExtensions) oldDecodeExts.Add(ext);

                // Unregister before disposing: Unregister clears the selection caches, and
                // Register would reject a CodecId that is still present.
                CodecRegistry.Unregister(proxy);
                proxy.Dispose();
            }
        }

        var declared = new List<string>();
        RegisterProxies(handle, declared);

        if (declared.Count > 0) Config.PurgePluginFileFormats(declared);

        // A decode change alters the browsable set, so the image list has to be rebuilt.
        // An encode-only change needs neither reload; the caller refreshes its own UI.
        var newDecodeExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        lock (_pluginProxiesLock)
        {
            if (_pluginProxies.TryGetValue(pluginId, out var fresh))
            {
                foreach (var proxy in fresh)
                {
                    foreach (var ext in proxy.DecodingExtensions) newDecodeExts.Add(ext);
                }
            }
        }

        if (!oldDecodeExts.SetEquals(newDecodeExts))
        {
            InvalidateCodecsAndReload();
            AppAPIProvider.IG_ReloadList();
        }
    }


    /// <summary>
    /// Hot-loads a plugin just enabled (after <c>PluginTrustPolicy.TrustAsync</c>) and reloads the
    /// current photo. Returns <c>false</c> if it could not be loaded (quarantined, ABI mismatch, ...).
    /// </summary>
    public static async Task<bool> EnablePluginAsync(PluginManifest manifest, string pluginDir)
    {
        if (PluginRegistry.IsLoaded(manifest.Id)) return true;

        var extensions = new List<string>();

        // native load + SHA-256 hashing is I/O; keep it off the UI thread
        var loaded = await Task.Run(() => RegisterPluginCodecs(manifest, pluginDir, extensions));
        if (!loaded) return false;

        // plugin formats are not persisted; purge any leftovers older versions baked in
        if (extensions.Count > 0) Config.PurgePluginFileFormats(extensions);

        Photos.ClearCache();
        InvalidateCodecsAndReload();

        // the plugin's formats are now live -> rebuild the image list so they become browsable
        if (extensions.Count > 0) AppAPIProvider.IG_ReloadList();

        return true;
    }


    /// <summary>
    /// Hot-unloads a plugin the user just disabled (after <c>PluginTrustPolicy.DisableAsync</c>):
    /// unregisters its codecs, frees the native library, and reloads the current photo.
    /// </summary>
    public static void DisablePlugin(string pluginId)
    {
        // free cached plugin buffers + cancel caching before the library is unloaded
        Photos.ClearCache();

        List<NativeCodecProxy>? proxies;
        lock (_pluginProxiesLock)
        {
            _pluginProxies.Remove(pluginId, out proxies);
        }

        if (proxies is not null)
        {
            foreach (var proxy in proxies)
            {
                CodecRegistry.Unregister(proxy);
                proxy.Dispose();
            }
        }

        // frees the native library; late buffer releases are gated by PluginLiveToken
        PluginRegistry.UnloadPlugin(pluginId);

        InvalidateCodecsAndReload();
    }


    /// <summary>
    /// Returns the effective set of supported image extensions: the persisted
    /// <see cref="Config.FileFormats"/> unioned with the extensions contributed by all currently
    /// loaded codec plugins. Plugin extensions are intentionally kept out of the persisted config
    /// so they disappear when a plugin is disabled or removed; this is the set to use for file
    /// listing, watching, picking, and default-viewer association.
    /// </summary>
    public static HashSet<string> GetSupportedFileExtensions()
    {
        var set = new HashSet<string>(Config.FileFormats, StringComparer.OrdinalIgnoreCase);

        lock (_pluginProxiesLock)
        {
            foreach (var proxies in _pluginProxies.Values)
            {
                foreach (var proxy in proxies)
                {
                    foreach (var ext in proxy.DecodingExtensions) set.Add(ext);
                }
            }
        }

        return set;
    }


    /// <summary>
    /// Drops the codec-selection caches and reloads the current photo (keeping zoom + pan) so a
    /// plugin enable/disable takes effect immediately.
    /// </summary>
    private static void InvalidateCodecsAndReload()
    {
        CodecRegistry.InvalidateSelectionCaches();
        AppAPIProvider.IG_Reload(false);
    }


    /// <summary>
    /// Registers external tools defined in <see cref="Config.Tools"/> into the
    /// <see cref="ToolRegistry"/>. Idempotent. Call after <see cref="Config"/> is loaded
    /// and before any UI consumes the registry.
    /// </summary>
    public static void RegisterExternalTools()
    {
        foreach (var tool in Config.Tools)
        {
            if (string.IsNullOrEmpty(tool.ToolId)) continue;
            if (ToolRegistry.Contains(tool.ToolId)) continue;

            try
            {
                var proxy = new ExternalToolProxy(tool, ToolRegistry.ExternalTools);
                ToolRegistry.Register(tool.ToolId, proxy);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Core.RegisterExternalTools] '{tool.ToolId}' failed: {ex.Message}");
            }
        }
    }


    /// <summary>
    /// Rebuilds the external tools in <see cref="ToolRegistry"/> from the current
    /// <see cref="Config.Tools"/>. Call after the tools setting is edited so the live registry
    /// (and thus the Tools menu / <c>IG_OpenTool</c>) reflects the changes.
    /// </summary>
    public static void ReloadExternalTools()
    {
        ToolRegistry.RemoveExternalTools();
        RegisterExternalTools();
    }


    public static bool SetTheme(IgTheme theme)
    {
        if (Core.Theme == theme) return false;

        Core.Theme = theme;
        return true;
    }


    public static bool SetAccentColor(Color accent)
    {
        if (Core.AccentColor == accent) return false;

        Core.AccentColor = accent;
        return true;
    }


    /// <summary>
    /// Updates base controls resources
    /// </summary>
    public static void UpdateBaseResources()
    {
        if (Application.Current is not App app) return;

        // 1. use modern Segoe UI font
        if (FontManager.Current.DefaultFontFamily.Name.Equals("Segoe UI"))
        {
            var fm = FontFamily.Parse("Segoe UI Variable Text");
            Resx.Set(ResxId.ContentControlThemeFontFamily, fm);
        }

        // 2. Set default Inter font size for macOS, Linux
        if (BHelper.OS != OSType.Windows)
        {
            app.Resources["ControlContentThemeFontSize"] =
                app.Resources["ToolTipContentThemeFontSize"] = Const.FONT_SIZE_BODY;
        }
    }


    /// <summary>
    /// Updates the app resources according to current color mode.
    /// </summary>
    public static void UpdateAppThemedColorResources()
    {
        if (Application.Current is not App app) return;

        // update theme colors
        Resx.Set(ResxId.IG_ThemeBackgroundBrush, AppThemeColors.BgBrush);
        Resx.Set(ResxId.IG_ThemeForegroundBrush, AppThemeColors.TextColorBrush);
        Resx.Set(ResxId.IG_ThemeToolbarBackgroundBrush, AppThemeColors.ToolbarBgBrush);
        Resx.Set(ResxId.IG_ThemeGalleryBackgroundBrush, AppThemeColors.GalleryBgBrush);
        Resx.Set(ResxId.IG_ThemeMenuBackgroundBrush, AppThemeColors.MenuBgBrush);
        UpdateViewerBackgroundBrushResource();

        // update situational colors
        if (Core.Theme.Settings.IsDarkMode)
        {
            Resx.Set(ResxId.IG_BackgroundInfoBrush, AppThemeColors.BackgroundInfoDark.ToBrush());
            Resx.Set(ResxId.IG_BackgroundSuccessBrush, AppThemeColors.BackgroundSuccessDark.ToBrush());
            Resx.Set(ResxId.IG_BackgroundWarningBrush, AppThemeColors.BackgroundWarningDark.ToBrush());
            Resx.Set(ResxId.IG_BackgroundDangerBrush, AppThemeColors.BackgroundDangerDark.ToBrush());

            Resx.Set(ResxId.IG_TextSuccessBrush, AppThemeColors.TextSuccessDark.ToBrush());
            Resx.Set(ResxId.IG_TextWarningBrush, AppThemeColors.TextWarningDark.ToBrush());
            Resx.Set(ResxId.IG_TextDangerBrush, AppThemeColors.TextDangerDark.ToBrush());

            Resx.Set(ResxId.IG_TextSuccessColor, AppThemeColors.TextSuccessDark);
            Resx.Set(ResxId.IG_TextWarningColor, AppThemeColors.TextWarningDark);
            Resx.Set(ResxId.IG_TextDangerColor, AppThemeColors.TextDangerDark);
        }
        else
        {
            Resx.Set(ResxId.IG_BackgroundInfoBrush, AppThemeColors.BackgroundInfoLight.ToBrush());
            Resx.Set(ResxId.IG_BackgroundSuccessBrush, AppThemeColors.BackgroundSuccessLight.ToBrush());
            Resx.Set(ResxId.IG_BackgroundWarningBrush, AppThemeColors.BackgroundWarningLight.ToBrush());
            Resx.Set(ResxId.IG_BackgroundDangerBrush, AppThemeColors.BackgroundDangerLight.ToBrush());

            Resx.Set(ResxId.IG_TextSuccessBrush, AppThemeColors.TextSuccessLight.ToBrush());
            Resx.Set(ResxId.IG_TextWarningBrush, AppThemeColors.TextWarningLight.ToBrush());
            Resx.Set(ResxId.IG_TextDangerBrush, AppThemeColors.TextDangerLight.ToBrush());

            Resx.Set(ResxId.IG_TextSuccessColor, AppThemeColors.TextSuccessLight);
            Resx.Set(ResxId.IG_TextWarningColor, AppThemeColors.TextWarningLight);
            Resx.Set(ResxId.IG_TextDangerColor, AppThemeColors.TextDangerLight);
        }

        var bgNeutralAlpha = 100;
        var bgColor = AppThemeColors.BgBrush.Color.NoAlpha();
        var bgNeutral = bgColor.Blend(Core.Theme.InvertedBaseColor, 0.95f, bgNeutralAlpha);
        var borderNeutral = bgColor.Blend(Core.Theme.InvertedBaseColor, 0.8f, bgNeutralAlpha / 2);
        var borderControl = bgColor.Blend(Core.Theme.InvertedBaseColor, 0.5f, bgNeutralAlpha / 2);

        Resx.Set(ResxId.IG_BackgroundNeutralBrush, bgNeutral.ToBrush());
        Resx.Set(ResxId.IG_BorderNeutralBrush, borderNeutral.ToBrush());
        Resx.Set(ResxId.IG_BorderControlBrush, borderControl.ToBrush());
        Resx.Set(ResxId.IG_MessageBackgroundBrush, bgColor.A(200).ToBrush());


        // update text color
        var textBrush = AppThemeColors.TextColorBrush.Color.ToBrush();
        var textPlaceholder = AppThemeColors.TextColorBrush.Color.Blend(Core.Theme.BaseColor, 0.7f, AppThemeColors.TextColorBrush.A);

        Resx.Set(ResxId.SystemControlForegroundBaseHighBrush, textBrush);
        Resx.Set(ResxId.TextControlForeground, textBrush);
        Resx.Set(ResxId.TextControlForegroundPointerOver, textBrush);
        Resx.Set(ResxId.TextControlForegroundFocused, textBrush);
        Resx.Set(ResxId.TextControlPlaceholderForeground, textPlaceholder.ToBrush());
        Resx.Set(ResxId.CheckBoxForegroundChecked, textBrush);
        Resx.Set(ResxId.CheckBoxForegroundCheckedPointerOver, textBrush);
        Resx.Set(ResxId.CheckBoxForegroundUnchecked, textBrush);
        Resx.Set(ResxId.CheckBoxForegroundUncheckedPointerOver, textBrush);

        // update border color
        Resx.Set(ResxId.TextControlBorderBrush, borderControl);
        Resx.Set(ResxId.TextControlBorderBrushDisabled, borderControl);
        Resx.Set(ResxId.ComboBoxBorderBrush, borderControl);
        Resx.Set(ResxId.CheckBoxCheckBackgroundStrokeUnchecked, borderControl);


        // update dropdown menu =======
        var menuBg = AppThemeColors.MenuBgBrush.Color.NoAlpha(); // no alpha support
        var menuBorder = Core.Theme.InvertedBaseColor.WithAlpha(30);
        var menuText = AppThemeColors.TextColorBrush.Color;
        var menuTextDisabled = menuText.Blend(Core.Theme.InvertedBaseColor, 0.8f, 100);

        Resx.Set(ResxId.MenuFlyoutPresenterBackground, menuBg);
        Resx.Set(ResxId.MenuFlyoutPresenterBorderBrush, menuBorder);
        Resx.Set(ResxId.IG_MenuSeparatorBackground, menuText.A(20));


        // menu text
        Resx.Set(ResxId.MenuFlyoutItemForeground, menuText);
        Resx.Set(ResxId.MenuFlyoutItemForegroundPointerOver, menuText);
        Resx.Set(ResxId.MenuFlyoutItemForegroundPressed, menuText);
        Resx.Set(ResxId.MenuFlyoutItemForegroundDisabled, menuTextDisabled);

        // menu hotkey text
        var hotkeyTextColor = menuText.A(200);
        Resx.Set(ResxId.MenuFlyoutItemKeyboardAcceleratorTextForeground, hotkeyTextColor);
        Resx.Set(ResxId.MenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver, hotkeyTextColor);
        Resx.Set(ResxId.MenuFlyoutItemKeyboardAcceleratorTextForegroundPressed, hotkeyTextColor);
        Resx.Set(ResxId.MenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled, menuTextDisabled);

        // menu chevron
        Resx.Set(ResxId.MenuFlyoutSubItemChevron, menuText);
        Resx.Set(ResxId.MenuFlyoutSubItemChevronPointerOver, menuText);
        Resx.Set(ResxId.MenuFlyoutSubItemChevronPressed, menuText);
        Resx.Set(ResxId.MenuFlyoutSubItemChevronDisabled, menuTextDisabled);
        Resx.Set(ResxId.MenuFlyoutSubItemChevronSubMenuOpened, menuText);

        // tooltip
        Resx.Set(ResxId.ToolTipForeground, menuText);
        Resx.Set(ResxId.ToolTipBackground, menuBg);
        Resx.Set(ResxId.ToolTipBorder, menuBorder);


        // combobox ===========
        Resx.Set(ResxId.ComboBoxForeground, textBrush);
        Resx.Set(ResxId.ComboBoxDropDownBackground, menuBg);
        Resx.Set(ResxId.ComboBoxDropDownBorderBrush, menuBorder);

        Resx.Set(ResxId.ComboBoxItemForeground, menuText);
        Resx.Set(ResxId.ComboBoxItemForegroundPointerOver, menuText);
        Resx.Set(ResxId.ComboBoxItemForegroundPressed, menuText);
        Resx.Set(ResxId.ComboBoxItemForegroundDisabled, menuTextDisabled);
    }


    /// <summary>
    /// Updates the viewer background resource according to app and slideshow settings.
    /// </summary>
    public static void UpdateViewerBackgroundBrushResource()
    {
        if (Application.Current is not App app) return;

        // 1. get background color from settings
        var bgColorText = Config.EnableSlideshow
            ? Config.SlideshowBackgroundColor
            : Config.BackgroundColor;
        var bgColor = BHelper.ColorFromHex(bgColorText, AccentColor);

        // 2. use default background color from theme pack
        if (bgColor.IsEmpty)
        {
            bgColor = AppThemeColors.BgBrush.Color;
        }

        // 3. set background color
        Resx.Set(ResxId.IG_ViewerBackgroundBrush, bgColor.ToBrush());
    }


    /// <summary>
    /// Updates the app resources according to current accent color
    /// </summary>
    public static void UpdateAccentColorResources()
    {
        if (Application.Current is not App app) return;

        // update app accent color
        var accent = Core.AccentColor;
        var accentLight1 = accent.WithBrightness(0.12f);
        var accentLight2 = accent.WithBrightness(0.28f);
        var accentLight3 = accent.WithBrightness(0.5f);

        var accentDark1 = accent.WithBrightness(-0.07f);
        var accentDark2 = accent.WithBrightness(-0.20f);
        var accentDark3 = accent.WithBrightness(-0.35f);


        // update all accent-related resources
        Resx.Set(ResxId.SystemAccentColor, accent);
        Resx.Set(ResxId.SystemAccentColorLight1, accentLight1);
        Resx.Set(ResxId.SystemAccentColorLight2, accentLight2);
        Resx.Set(ResxId.SystemAccentColorLight3, accentLight3);
        Resx.Set(ResxId.SystemAccentColorDark1, accentDark1);
        Resx.Set(ResxId.SystemAccentColorDark2, accentDark2);
        Resx.Set(ResxId.SystemAccentColorDark3, accentDark3);

        // accent-colored text: the raw accent is too dark on a dark background,
        // so lighten it in dark mode and darken it in light mode for contrast
        var textAccent = Core.Theme.Settings.IsDarkMode ? accentLight3 : accentDark1;
        Resx.Set(ResxId.IG_TextAccentColor, textAccent);


        // accent button text: pick black/white based on the accent brightness so
        // it stays readable when the system accent is dark (fixes contrast issue)
        var accentText = accent.InvertBlackOrWhite();
        var accentTextBrush = accentText.ToBrush();
        Resx.Set(ResxId.AccentButtonForeground, accentTextBrush);
        Resx.Set(ResxId.AccentButtonForegroundPointerOver, accentTextBrush);
        Resx.Set(ResxId.AccentButtonForegroundPressed, accentTextBrush);
        Resx.Set(ResxId.AccentButtonForegroundDisabled, accentText.A(128).ToBrush());


        // border hover styles
        var borderHoverBrush = accentLight1.ToBrush();
        Resx.Set(ResxId.TextControlBorderBrushPointerOver, borderHoverBrush);
        Resx.Set(ResxId.ComboBoxBorderBrushPointerOver, borderHoverBrush);
        Resx.Set(ResxId.CheckBoxCheckBackgroundStrokeUncheckedPointerOver, borderHoverBrush);


        // tool buttons
        var btnBgAlphaGap = Core.Theme.Settings.IsDarkMode ? 0 : -50;
        var btnBg = accent.A(0);
        var btnBgHover = accent.A((byte)(90 + btnBgAlphaGap));
        var btnBgPressed = accent.A((byte)(130 + btnBgAlphaGap));
        var btnBgChecked = accent.A((byte)(150 + btnBgAlphaGap));

        Resx.Set(ResxId.IG_ToolButtonBackground, btnBg);
        Resx.Set(ResxId.IG_ToolButtonBackgroundHover, btnBgHover);
        Resx.Set(ResxId.IG_ToolButtonBackgroundPressed, btnBgPressed);
        Resx.Set(ResxId.IG_ToolButtonBackgroundChecked, btnBgChecked);

        // menu item background
        Resx.Set(ResxId.MenuFlyoutItemBackground, btnBg);
        Resx.Set(ResxId.MenuFlyoutItemBackgroundPointerOver, btnBgHover);
        Resx.Set(ResxId.MenuFlyoutItemBackgroundPressed, btnBgPressed);

        // combobox item background
        Resx.Set(ResxId.ComboBoxItemBackground, btnBg);
        Resx.Set(ResxId.ComboBoxItemBackgroundPointerOver, btnBgHover);
        Resx.Set(ResxId.ComboBoxItemBackgroundPressed, btnBgPressed);
        Resx.Set(ResxId.ComboBoxItemBackgroundSelected, btnBgChecked);
    }


    /// <summary>
    /// Sets dark mode.
    /// </summary>
    public static void SetAppDarkThemeVariant(bool enable)
    {
        if (enable)
        {
            Application.Current?.RequestedThemeVariant = ThemeVariant.Dark;
        }
        else
        {
            Application.Current?.RequestedThemeVariant = ThemeVariant.Light;
        }
    }


    /// <summary>
    /// Update input path from arguments.
    /// </summary>
    public static void UpdateInitImagePath(string? path = null)
    {
        var pathToLoad = path;

        if (string.IsNullOrWhiteSpace(pathToLoad) && Core.Args.Length >= 2)
        {
            // get path from params, skipping "-p:" config overrides and "--" flags
            // (e.g. --ig-startup-trace) so a flag is never taken as the image path
            var cmdPath = Core.Args
                .Skip(1)
                .FirstOrDefault(i => !i.StartsWith(Const.CONFIG_CMD_PREFIX, StringComparison.Ordinal)
                    && !i.StartsWith("--", StringComparison.Ordinal));

            if (!string.IsNullOrEmpty(cmdPath))
            {
                pathToLoad = cmdPath;
            }
        }

        // store resolved: a shortcut's own folder must never pass as the folder being opened
        _initImagePathFromArgs = BHelper.ResolvePath(pathToLoad);
    }


    /// <summary>
    /// Quickly save the viewing photo as a temporary file.
    /// </summary>
    public static async Task<string?> SavePhotoAsTempFileAsync(string ext = ".png")
    {
        // 1. check if we can use the current clipboard image path
        if (File.Exists(Core.TempImagePath))
        {
            var extension = Path.GetExtension(Core.TempImagePath);

            if (extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return Core.TempImagePath;
            }
        }


        // 2. create temp file path
        var tempFilePath = BHelper.ConfigDir(Dir.Temporary, $"ig_temp_{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}{ext}");


        // 3. save the photo to file
        var photo = Core.ClipboardImage ?? Core.Photos.Current;
        if (photo is not null)
        {
            try
            {
                // save photo as file
                await photo.SaveAsAsync(tempFilePath, Core.ImageTransform, 85);

                Core.TempImagePath = tempFilePath;
            }
            catch
            {
                Core.TempImagePath = null;
            }
        }

        return Core.TempImagePath;
    }


    /// <summary>
    /// Disposes the clipboard photo.
    /// </summary>
    public static void DisposeClipboardPhoto()
    {
        Core.ClipboardImage?.Dispose();
        Core.ClipboardImage = null;
        Core.TempImagePath = null;
    }


    /// <summary>
    /// Updates the current destination color space.
    /// </summary>
    /// <param name="requiresPhotoReload">
    /// Pass <see langword="false"/> when the profile changed only because the window moved to
    /// another monitor, so the on-screen photo is left alone instead of flashing through a reload.
    /// </param>
    public static void UpdateDestColorProfile(bool requiresPhotoReload = true)
    {
        var results = SkiaCodec.GetColorProfileByName(Core.Config.ColorProfile);
        Core.IsDestColorProfileSupported = results.IsSupported;

        // Codec selection depends on IsDestColorProfileSupported, so drop the cached winners
        // (a codec chosen while Skia was ineligible must not stay stuck decoding).
        Core.CodecRegistry.InvalidateSelectionCaches();

        // apply the new profile and notify
        Core.DestColorProfile?.Dispose();
        Core.DestColorProfile = results.ColorSpace;
        Core.OnColorProfileChanged(requiresPhotoReload);
    }


    /// <summary>
    /// Loads a photo as the clipboard image in the viewer.
    /// </summary>
    public static async Task LoadClipboardPhotoAsync(Photo? photo)
    {
        if (API is null) return;
        await AppAPIProvider.LoadClipboardPhotoAsync(photo);
    }


    #endregion // Public Methods



    #region Methods to raise Events


    /// <summary>
    /// Raises ColorProfileChanged event on UI thread.
    /// </summary>
    public static void OnColorProfileChanged(bool requiresPhotoReload = true)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ColorProfileChanged?.Invoke(null, new(requiresPhotoReload));
        });

        ToolRegistry.ExternalTools.BroadcastToAll(MessageTypes.COLOR_PROFILE_CHANGED);
    }


    /// <summary>
    /// Raises <see cref="ThemeChanged"/> event on UI thread.
    /// </summary>
    public static void OnThemeChanged(string propName = "")
    {
        Dispatcher.UIThread.Post(() =>
        {
            ThemeChanged?.Invoke(null, new ThemePackChangedEventArgs(propName));
        });

        ToolRegistry.ExternalTools.BroadcastToAll(MessageTypes.THEME_CHANGED, new ThemeInfo
        {
            IsDarkMode = Theme.Settings.IsDarkMode,
            AccentColor = AccentColor.ToString(),
            BackgroundColor = Config.BackgroundColor,
        });
    }


    /// <summary>
    /// Raises <see cref="ThemeChanged"/> event on UI thread.
    /// </summary>
    public static void OnLanguageChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            LanguageChanged?.Invoke(null, new());
        });

        ToolRegistry.ExternalTools.BroadcastToAll(MessageTypes.LANGUAGE_CHANGED, new LanguageChangedEventArgs
        {
            Code = Lang.Metadata.Code,
            EnglishName = Lang.Metadata.EnglishName,
            LocalName = Lang.Metadata.LocalName,
        });
    }


    /// <summary>
    /// Raises <see cref="PhotoUnloadedEventArgs"/> event on UI thread.
    /// </summary>
    public static void OnPhotoUnloaded(PhotoUnloadedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            PhotoUnloaded?.Invoke(null, e);
        });
    }


    /// <summary>
    /// Raises <see cref="PhotoSaved"/> event on UI thread.
    /// </summary>
    public static void OnPhotoSaved(PhotoSaveEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            PhotoSaved?.Invoke(null, e);
        });
    }


    /// <summary>
    /// Broadcasts a photo change event to all running external plugins.
    /// </summary>
    public static void BroadcastPhotoChanged(Photo? photo)
    {
        if (photo is null) return;

        ToolRegistry.ExternalTools.BroadcastToAll(MessageTypes.PHOTO_CHANGED, new PhotoChangedEventArgs
        {
            FilePath = photo.FilePath,
            Width = (int)(photo.Metadata?.OriginalWidth ?? 0),
            Height = (int)(photo.Metadata?.OriginalHeight ?? 0),
            Format = photo.Metadata?.FileExtension,
            FrameCount = (int)(photo.Metadata?.FrameCount ?? 1),
            CanAnimate = photo.Metadata?.CanAnimate ?? false,
        });
    }



    internal static void Viewer_PhotoLoadingForPlugins(ViewerControl sender, PhotoLoadingEventArgs e)
    {
        if (e.State == PhotoState.Loaded)
        {
            BroadcastPhotoChanged(e.Photo);
        }
    }

    internal static void Viewer_PointerMovedForPlugins(ViewerControl sender, ViewerPointerEventArgs e)
    {
        ToolRegistry.ExternalTools.BroadcastToSubscribed(
            MessageTypes.POINTER_MOVED,
            new PointerEventArgs
            {
                SourceX = e.SourcePoint.X,
                SourceY = e.SourcePoint.Y,
                ClientX = (float)e.Point.Position.X,
                ClientY = (float)e.Point.Position.Y,
            },
            s => s.PointerMoved);
    }

    internal static void Viewer_PointerPressedForPlugins(ViewerControl sender, ViewerPointerEventArgs e)
    {
        ToolRegistry.ExternalTools.BroadcastToSubscribed(
            MessageTypes.POINTER_PRESSED,
            new PointerEventArgs
            {
                SourceX = e.SourcePoint.X,
                SourceY = e.SourcePoint.Y,
                ClientX = (float)e.Point.Position.X,
                ClientY = (float)e.Point.Position.Y,
            },
            s => s.PointerPressed);
    }

    internal static void Viewer_SelectionChangedForPlugins(ViewerControl sender, ViewerSelectionChangedEventArgs e)
    {
        var src = e.SourceSelection;
        ToolRegistry.ExternalTools.BroadcastToSubscribed(
            MessageTypes.SELECTION_CHANGED,
            src == default ? null : new SelectionEventArgs
            {
                X = (float)src.X,
                Y = (float)src.Y,
                Width = (float)src.Width,
                Height = (float)src.Height,
            },
            s => s.SelectionChanged);
    }

    internal static void Viewer_FrameChangedForPlugins(ViewerControl sender, PhotoFrameChangedEventArgs e)
    {
        ToolRegistry.ExternalTools.BroadcastToSubscribed(
            MessageTypes.FRAME_CHANGED,
            new FrameChangedPayload
            {
                FrameIndex = (int)e.CurrentFrame,
            },
            s => s.FrameChanged);
    }


    #endregion // Methods to raise Events


}
