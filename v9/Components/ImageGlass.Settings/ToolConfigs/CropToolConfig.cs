/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
public class CropToolConfig: IToolConfig
{
    public string ToolId { get; init; }


    /// <summary>
    /// Gets, sets the aspect ratio type.
    /// </summary>
    public SelectionAspectRatio AspectRatio { get; set; } = SelectionAspectRatio.FreeRatio;


    /// <summary>
    /// Gets, sets the aspect ratio values.
    /// </summary>
    public int[] AspectRatioValues { get; set; } = new int[2] { 0, 0 };


    /// <summary>
    /// Gets, sets the option to close the Crop tool after the selected area is saved.
    /// </summary>
    public bool CloseToolAfterSaving { get; set; } = false;


    /// <summary>
    /// Gets, sets the default selection type.
    /// </summary>
    public DefaultSelectionType DefaultSelection { get; set; } = DefaultSelectionType.SelectNone;


    /// <summary>
    /// Gets, sets the custom selection area is used for <see cref="DefaultSelectionType.Custom"/>.
    /// </summary>
    public Rectangle SelectionArea {  get; set; } = Rectangle.Empty;


    /// <summary>
    /// Gets, sets the option to center the <see cref="SelectionArea"/>.
    /// </summary>
    public bool CenterSelectionArea { get; set; } = false;


    /// <summary>
    /// Initializes new instance of <see cref="CropToolConfig"/>.
    /// </summary>
    public CropToolConfig(string toolId)
    {
        ToolId = toolId;
    }


    public void LoadFromAppConfig()
    {
        var toolConfig = Config.Tools.GetValue(ToolId);
        if (toolConfig is not ExpandoObject config) return;


        // Bool configs
        CloseToolAfterSaving = config.GetValue(nameof(CloseToolAfterSaving), CloseToolAfterSaving);
        CenterSelectionArea = config.GetValue(nameof(CenterSelectionArea), CenterSelectionArea);

        // Enum configs
        AspectRatio = config.GetValue(nameof(AspectRatio), AspectRatio);
        DefaultSelection = config.GetValue(nameof(DefaultSelection), DefaultSelection);

        #region Array configs

        // load AspectRatioValues
        var ratioValues = config.GetValue(nameof(AspectRatioValues)) as dynamic as IEnumerable<object>;
        if (ratioValues != null)
        {
            var numArr = ratioValues.Select(i => int.Parse((string)i)).ToArray();
            if (numArr.Length == 2)
            {
                AspectRatioValues = new int[2] { numArr[0], numArr[1] };
            }
        }


        // load SelectionArea
        var bounds = config.GetValue(nameof(SelectionArea)) as dynamic as IEnumerable<object>;
        if (bounds != null)
        {
            var numArr = bounds.Select(i => int.Parse((string)i)).ToArray();
            if (numArr.Length == 4)
            {
                SelectionArea = new Rectangle(numArr[0], numArr[1], numArr[2], numArr[3]);
            }
        }

        #endregion // Array config

    }


    public void SaveToAppConfig()
    {
        var settings = new ExpandoObject();

        // Bool configs
        settings.TryAdd(nameof(CloseToolAfterSaving), CloseToolAfterSaving);
        settings.TryAdd(nameof(CenterSelectionArea), CenterSelectionArea);

        // Enum configs
        settings.TryAdd(nameof(AspectRatio), AspectRatio);
        settings.TryAdd(nameof(DefaultSelection), DefaultSelection);

        // Array configs
        settings.TryAdd(nameof(AspectRatioValues), AspectRatioValues);
        settings.TryAdd(nameof(SelectionArea), new int[]
        {
            SelectionArea.Left,
            SelectionArea.Top,
            SelectionArea.Width,
            SelectionArea.Height,
        });


        // save to app config
        Config.Tools.Set(ToolId, settings);
    }

}


/// <summary>
/// Options for Crop tool's default selection
/// </summary>
public enum DefaultSelectionType
{
    SelectNone,
    SelectAll,
    Custom,
}
