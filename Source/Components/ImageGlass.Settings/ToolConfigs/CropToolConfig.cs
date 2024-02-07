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
using ImageGlass.Base;
using ImageGlass.Settings;
using ImageGlass.UI;
using Microsoft.Extensions.Configuration;
using System.Dynamic;

namespace ImageGlass;

/// <summary>
/// Provides settings for Crop tool.
/// </summary>
public class CropToolConfig(string toolId) : IToolConfig
{
    public string ToolId { get; init; } = toolId;


    /// <summary>
    /// Gets, sets the aspect ratio type.
    /// </summary>
    public SelectionAspectRatio AspectRatio { get; set; } = SelectionAspectRatio.FreeRatio;


    /// <summary>
    /// Gets, sets the aspect ratio values.
    /// </summary>
    public int[] AspectRatioValues { get; set; } = [0, 0];


    /// <summary>
    /// Gets, sets the option to close the Crop tool after the selected area is saved.
    /// </summary>
    public bool CloseToolAfterSaving { get; set; } = false;


    /// <summary>
    /// Gets, sets the default selection type.
    /// </summary>
    public DefaultSelectionType InitSelectionType { get; set; } = DefaultSelectionType.Select50Percent;


    /// <summary>
    /// Gets, sets the custom selection area is used for <see cref="DefaultSelectionType.CustomArea"/>.
    /// </summary>
    public Rectangle InitSelectedArea { get; set; } = Rectangle.Empty;


    /// <summary>
    /// Gets, sets the option to center the <see cref="InitSelectedArea"/>.
    /// </summary>
    public bool AutoCenterSelection { get; set; } = true;


    public void LoadFromAppConfig()
    {
        var toolConfig = Config.ToolSettings.GetValue(ToolId);
        if (toolConfig is not ExpandoObject config) return;


        // Bool configs
        CloseToolAfterSaving = config.GetValue(nameof(CloseToolAfterSaving), CloseToolAfterSaving);
        AutoCenterSelection = config.GetValue(nameof(AutoCenterSelection), AutoCenterSelection);

        // Enum configs
        AspectRatio = config.GetValue(nameof(AspectRatio), AspectRatio);
        InitSelectionType = config.GetValue(nameof(InitSelectionType), InitSelectionType);

        #region Array configs

        // load AspectRatioValues
        var ratioValues = config.GetValue(nameof(AspectRatioValues)) as dynamic as IEnumerable<object>;
        if (ratioValues != null)
        {
            var numArr = ratioValues.Select(i => int.Parse((string)i)).ToArray();
            if (numArr.Length == 2)
            {
                AspectRatioValues = [numArr[0], numArr[1]];
            }
        }


        // load SelectionArea
        var bounds = config.GetValue(nameof(InitSelectedArea)) as dynamic as IEnumerable<object>;
        if (bounds != null)
        {
            var numArr = bounds.Select(i => int.Parse((string)i)).ToArray();
            if (numArr.Length == 4)
            {
                InitSelectedArea = new Rectangle(numArr[0], numArr[1], numArr[2], numArr[3]);
            }
        }

        #endregion // Array config

    }


    public void SaveToAppConfig()
    {
        var settings = new ExpandoObject();

        // Bool configs
        _ = settings.TryAdd(nameof(CloseToolAfterSaving), CloseToolAfterSaving);
        _ = settings.TryAdd(nameof(AutoCenterSelection), AutoCenterSelection);

        // Enum configs
        _ = settings.TryAdd(nameof(AspectRatio), AspectRatio);
        _ = settings.TryAdd(nameof(InitSelectionType), InitSelectionType);

        // Array configs
        _ = settings.TryAdd(nameof(AspectRatioValues), AspectRatioValues);
        _ = settings.TryAdd(nameof(InitSelectedArea), new int[]
        {
            InitSelectedArea.Left,
            InitSelectedArea.Top,
            InitSelectedArea.Width,
            InitSelectedArea.Height,
        });


        // save to app config
        Config.ToolSettings.Set(ToolId, settings);
    }

}


/// <summary>
/// Options for Crop tool's default selection
/// </summary>
public enum DefaultSelectionType
{
    UseTheLastSelection,
    CustomArea,
    SelectAll,
    SelectNone,
    Select10Percent,
    Select20Percent,
    Select25Percent,
    Select30Percent,
    SelectOneThird,
    Select40Percent,
    Select50Percent,
    Select60Percent,
    SelectTwoThirds,
    Select70Percent,
    Select75Percent,
    Select80Percent,
    Select90Percent,
}
