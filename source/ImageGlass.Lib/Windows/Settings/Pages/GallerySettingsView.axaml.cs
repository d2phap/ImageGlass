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
using Avalonia.Controls;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.UI.Windowing;
using System;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class GallerySettingsView : SettingsPageView
{
    public GallerySettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public GallerySettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        // Appearance
        BindToggle(PART_ShowGalleryInFullscreen, ConfigId.ShowGalleryInFullscreen,
            LangId.Settings_ShowGalleryInFullscreen, LangId.Settings_Appearance);
        BindToggle(PART_ShowGalleryFileName, ConfigId.ShowGalleryFileName,
            LangId.Settings_ShowGalleryFileName, LangId.Settings_Appearance, true);
        BindToggle(PART_ShellThumbnail, ConfigId.EnableGalleryShellThumbnail,
            LangId.Settings_EnableGalleryShellThumbnail, LangId.Settings_Appearance, true);

        BindUIntSlider(PART_GalleryColumns, ConfigId.GalleryColumns,
            LangId.Settings_GalleryColumns, LangId.Settings_Appearance, 3u, PART_GalleryColumnsLabel);
        BindUIntSlider(PART_ThumbnailSize, ConfigId.ThumbnailSize,
            LangId.Settings_ThumbnailSize, LangId.Settings_Appearance, 70u, PART_ThumbnailSizeLabel);

        BindUIntInput(PART_GalleryCacheSize, ConfigId.GalleryCacheSizeInMb,
            LangId.Settings_GalleryCacheSizeInMb, LangId.Settings_Appearance, 100u);

        SetLocalizedText(PART_ClearCache, LangId._ClearCache);
        PART_ClearCache.Click += async (_, _) => await ClearThumbnailCacheAsync();
        RegisterSearchKey(PART_ClearCache, LangId._ClearCache, null, LangId.Settings_Appearance);
    }


    /// <summary>
    /// Deletes every cached thumbnail from disk and reports the outcome.
    /// </summary>
    private async Task ClearThumbnailCacheAsync()
    {
        var owner = TopLevel.GetTopLevel(this) as PhWindow;
        Exception? error;

        // the cache folder can hold thousands of files, so delete it off the UI thread
        PART_ClearCache.IsEnabled = false;
        try
        {
            error = await Task.Run(ThumbnailDiskCache.Clear);
        }
        finally
        {
            PART_ClearCache.IsEnabled = true;
        }

        if (error is null)
        {
            await ModalWindow.ShowInfoAsync(owner, new ModalWindowOptions
            {
                Title = Core.Lang[LangId._ClearCache],
                Heading = Core.Lang[LangId._ClearCache_Success],
            });
            return;
        }

        await ModalWindow.ShowErrorAsync(owner, new ModalWindowOptions
        {
            Title = Core.Lang[LangId._ClearCache],
            Heading = Core.Lang[LangId._ClearCache_Error],
            Details = error.Message,
        });
    }
}
