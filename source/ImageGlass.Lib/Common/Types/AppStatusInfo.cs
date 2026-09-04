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
using Avalonia.Threading;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using ImageGlass.UI.Viewer;
using ImageGlass.UI.Viewer.ZoomAndPan;
using System;
using System.ComponentModel;
using System.Text;

namespace ImageGlass.Common;

public partial class AppStatusInfo : PhDisposable
{
    private ViewerControl _viewer;
    private string? _filePath = null;

    public event EventHandler? Changed;


    #region Image Info Tags

    internal string? AppName
    {
        get
        {
            if (Core.Config.ImageInfoTags.Contains(nameof(AppName)))
            {
                return BHelper.AppDisplayName;
            }

            return null;
        }
    }


    internal string? Name
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if (Core.Config.ImageInfoTags.Contains(nameof(Name)))
            {
                var askterisk = Core.ImageTransform.HasChanges ? "*" : string.Empty;
                return $"{System.IO.Path.GetFileName(_filePath)}{askterisk}";
            }

            return null;
        }
    }


    internal string? Path
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if (Core.Config.ImageInfoTags.Contains(nameof(Path)))
            {
                var askterisk = Core.ImageTransform.HasChanges ? "*" : string.Empty;
                return $"{_filePath}{askterisk}";
            }

            return null;
        }
    }


    internal string? FileSize
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if (Core.Config.ImageInfoTags.Contains(nameof(FileSize))
                && Core.Photos.CurrentMetadata != null)
            {
                return Core.Photos.CurrentMetadata.FileSizeFormatted;
            }

            return null;
        }
    }


    internal string? ModifiedDateTime
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if ((Core.Config.ImageInfoTags.Contains(nameof(ModifiedDateTime))
                || Core.Config.ImageInfoTags.Contains(nameof(DateTimeAuto)))
                && Core.Photos.CurrentMetadata != null)
            {
                return Core.Photos.CurrentMetadata.FileLastWriteTimeFormatted + " (m)";
            }

            return null;
        }
    }


    internal string? Dimension
    {
        get
        {
            if (Core.Config.ImageInfoTags.Contains(nameof(Dimension)))
            {
                if (Core.ClipboardImage is not null && !Core.ClipboardImage.Size.IsEmpty)
                {
                    return $"{Core.ClipboardImage.Width:n0}×{Core.ClipboardImage.Height:n0}";
                }
                else if (!_viewer.BitmapSize.IsEmpty)
                {
                    var dimension = $"{_viewer.BitmapSize.Width:n0}×{_viewer.BitmapSize.Height:n0}";

                    // the file is bigger than what could be decoded
                    var scale = _viewer.Photo?.DecodeScale ?? 1;
                    if (scale < 1 && Core.Photos.CurrentMetadata is { } meta)
                    {
                        dimension += $" ({meta.Width:n0}×{meta.Height:n0}) – {Math.Round(scale * 100):n0}%";
                    }

                    return dimension;
                }
            }

            return null;
        }
    }


    internal string? FrameCount
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if (Core.Config.ImageInfoTags.Contains(nameof(FrameCount))
                && Core.Photos.CurrentMetadata != null
                && Core.Photos.CurrentMetadata.FrameCount > 1)
            {
                var frameInfo = new StringBuilder();
                frameInfo.Append((_viewer.Photo?.FrameIndex ?? 0) + 1);
                frameInfo.Append('/');
                frameInfo.Append(Core.Photos.CurrentMetadata.FrameCount);

                return Core.Lang[LangId._ImageInfo_FrameCount, frameInfo];
            }

            return null;
        }
    }


    internal string? ListCount
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if (Core.Config.ImageInfoTags.Contains(nameof(ListCount))
                && Core.Photos.Count > 0)
            {
                var listInfo = new StringBuilder();
                listInfo.Append(Core.Photos.CurrentIndex + 1);
                listInfo.Append('/');
                listInfo.Append(Core.Photos.Count);

                return Core.Lang[LangId._ImageInfo_ListCount, listInfo.ToString()];
            }

            return null;
        }
    }


    internal string? Zoom
    {
        get
        {
            if (Core.Config.ImageInfoTags.Contains(nameof(Zoom)) && Core.Photos.Count > 0)
            {
                return $"{Math.Round(_viewer.ZoomFactor * 100, 2):n2}%";
            }

            return null;
        }
    }


    internal string? ExifRating
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if (Core.Config.ImageInfoTags.Contains(nameof(ExifRating))
                && Core.Photos.CurrentMetadata != null)
            {
                return Core.Photos.CurrentMetadata.ExifRatingFormatted;
            }

            return null;
        }
    }


    internal string? ExifDateTime
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if ((Core.Config.ImageInfoTags.Contains(nameof(ExifDateTime))
                || Core.Config.ImageInfoTags.Contains(nameof(DateTimeAuto)))
                && Core.Photos.CurrentMetadata != null
                && Core.Photos.CurrentMetadata.ExifDateTime != null)
            {
                return BHelper.FormatDateTime(Core.Photos.CurrentMetadata.ExifDateTime) + " (e)";
            }

            return null;
        }
    }


    internal string? ExifDateTimeOriginal
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if ((Core.Config.ImageInfoTags.Contains(nameof(ExifDateTimeOriginal))
                || Core.Config.ImageInfoTags.Contains(nameof(DateTimeAuto)))
                && Core.Photos.CurrentMetadata != null
                && Core.Photos.CurrentMetadata.ExifDateTimeOriginal != null)
            {
                return BHelper.FormatDateTime(Core.Photos.CurrentMetadata.ExifDateTimeOriginal) + " (o)";
            }

            return null;
        }
    }


    internal string? DateTimeAuto
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if (Core.Config.ImageInfoTags.Contains(nameof(DateTimeAuto))
                && Core.Photos.CurrentMetadata != null)
            {
                if (Core.Photos.CurrentMetadata.ExifDateTimeOriginal != null)
                {
                    return ExifDateTimeOriginal;
                }

                if (Core.Photos.CurrentMetadata.ExifDateTime != null)
                {
                    return ExifDateTime;
                }

                return ModifiedDateTime;
            }

            return null;
        }
    }


    internal string? HdrInfo
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if (Core.Config.ImageInfoTags.Contains(nameof(HdrInfo))
                && Core.Photos.CurrentMetadata is { } meta
                && (meta.IsHdr || meta.IsWideGamut))
            {
                var parts = new System.Collections.Generic.List<string>(4);

                if (meta.IsHdr)
                {
                    var fn = meta.HdrTransferFn switch
                    {
                        Photoing.HdrTransferFunction.PQ => "HDR PQ",
                        Photoing.HdrTransferFunction.HLG => "HDR HLG",
                        Photoing.HdrTransferFunction.GainMap => "HDR Gain Map",
                        Photoing.HdrTransferFunction.ScRgb => "HDR scRGB",
                        _ => "HDR",
                    };
                    parts.Add(fn);

                    if (meta.ContentPeakNits > 0)
                    {
                        parts.Add($"{meta.ContentPeakNits:0} nits");
                    }
                }
                else if (meta.IsWideGamut)
                {
                    parts.Add("Wide Gamut");
                }

                if (meta.BitsPerChannel > 8)
                {
                    parts.Add($"{meta.BitsPerChannel}-bit");
                }

                return string.Join(", ", parts);
            }

            return null;
        }
    }


    internal string? ColorSpace
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if (Core.Config.ImageInfoTags.Contains(nameof(ColorSpace))
                && Core.Photos.CurrentMetadata != null
                && Core.Photos.CurrentMetadata.ColorSpace != ImageMagick.ColorSpace.Undefined)
            {
                var colorSpace = Core.Photos.CurrentMetadata.ColorSpace.ToString();
                var colorProfile = !string.IsNullOrEmpty(Core.Photos.CurrentMetadata.ColorProfileName)
                    ? Core.Photos.CurrentMetadata.ColorProfileName
                    : "-";

                if (colorSpace.Equals(colorProfile, StringComparison.OrdinalIgnoreCase))
                {
                    return colorSpace;
                }

                return $"{colorSpace}/{colorProfile}";
            }

            return null;
        }
    }


    internal string? DPI
    {
        get
        {
            // skip for clipboard image
            if (Core.ClipboardImage is not null) return null;

            if (Core.Config.ImageInfoTags.Contains(nameof(DPI))
                && Core.Photos.CurrentMetadata != null
                && Core.Photos.CurrentMetadata.DpiX > 0
                && Core.Photos.CurrentMetadata.DpiY > 0)
            {
                return $"{Core.Photos.CurrentMetadata.DpiX:n0}×{Core.Photos.CurrentMetadata.DpiY:n0} DPI";
            }

            return null;
        }
    }

    #endregion // Image Info Tags



    /// <summary>
    /// Gets the status text.
    /// </summary>
    public string Text
    {
        get
        {
            var strBuilder = new StringBuilder();
            int count = 0;

            if (Core.ClipboardImage is not null)
            {
                strBuilder.Append(Core.Lang[LangId._ClipboardImage]);
                count++;
            }

            foreach (var tag in Core.Config.ImageInfoTags)
            {
                var tagValue = tag switch
                {
                    nameof(AppName) => AppName,
                    nameof(Name) => Name,
                    nameof(Path) => Path,
                    nameof(FileSize) => FileSize,
                    nameof(ModifiedDateTime) => ModifiedDateTime,

                    nameof(Dimension) => Dimension,
                    nameof(FrameCount) => FrameCount,
                    nameof(ListCount) => ListCount,
                    nameof(Zoom) => Zoom,

                    nameof(ExifRating) => ExifRating,
                    nameof(ExifDateTime) => ExifDateTime,
                    nameof(ExifDateTimeOriginal) => ExifDateTimeOriginal,
                    nameof(DateTimeAuto) => DateTimeAuto,
                    nameof(HdrInfo) => HdrInfo,
                    nameof(ColorSpace) => ColorSpace,
                    nameof(DPI) => DPI,
                    _ => null,
                };

                if (!string.IsNullOrWhiteSpace(tagValue))
                {
                    if (count > 0)
                    {
                        strBuilder.Append("  ︱  ");
                    }

                    strBuilder.Append(tagValue);
                    count++;
                }
            }

            return strBuilder.ToString();
        }
    }


    public AppStatusInfo(ViewerControl viewer)
    {
        _viewer = viewer;

        Core.Photos.PropertyChanged += Photos_PropertyChanged;
        Core.ImageTransform.Changed += ImageTransform_Changed;
        Core.Config.PropertyChanged += Config_PropertyChanged;
        _viewer.ZoomChanged += Viewer_ZoomChanged;
        _viewer.PhotoFrameChanged += Viewer_PhotoFrameChanged;
    }


    protected override void OnDisposing()
    {
        base.OnDisposing();

        Core.Photos.PropertyChanged -= Photos_PropertyChanged;
        Core.ImageTransform.Changed -= ImageTransform_Changed;
        Core.Config.PropertyChanged -= Config_PropertyChanged;
        _viewer.ZoomChanged -= Viewer_ZoomChanged;
        _viewer.PhotoFrameChanged -= Viewer_PhotoFrameChanged;
    }


    private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // the status text is built from these tags, so refresh it when they change
        if (e.PropertyName != nameof(Core.Config.ImageInfoTags)) return;

        Dispatcher.UIThread.Post(() =>
        {
            Changed?.Invoke(this, EventArgs.Empty);
        });
    }


    private void Photos_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Core.Photos.CurrentFilePath))
        {
            _filePath = string.IsNullOrEmpty(Core.Photos.CurrentFilePath)
                ? Core.Photos.GetFilePath(Core.Photos.CurrentIndex)
                : BHelper.ResolvePath(Core.Photos.CurrentFilePath);
        }


        Dispatcher.UIThread.Post(() =>
        {
            Changed?.Invoke(this, EventArgs.Empty);
        });
    }


    private void ImageTransform_Changed(PhotoTransform sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Changed?.Invoke(this, EventArgs.Empty);
        });
    }


    private void Viewer_ZoomChanged(ViewerControl sender, ViewerZoomEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Changed?.Invoke(this, EventArgs.Empty);
        });
    }


    private void Viewer_PhotoFrameChanged(ViewerControl sender, PhotoFrameChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Changed?.Invoke(this, EventArgs.Empty);
        });
    }


}
