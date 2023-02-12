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

using ImageGlass.Base.DirectoryComparer;

namespace ImageGlass.Base;

public partial class BHelper
{
    /// <summary>
    /// Convert string to int array, where numbers are separated by semicolons
    /// </summary>
    /// <param name="str">Input string. E.g. "12; -40; 50"</param>
    /// <param name="unsignedOnly">whether negative numbers are allowed</param>
    /// <param name="distinct">whether repitition of values is allowed</param>
    public static int[] StringToIntArray(string str, bool unsignedOnly = false, bool distinct = false)
    {
        var args = str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        var numbers = new List<int>();

        foreach (var item in args)
        {
            // Issue #677 : don't throw exception if we encounter invalid number, e.g. the comma-separated zoom values from pre-V7.5
            if (!int.TryParse(item, System.Globalization.NumberStyles.Integer, Constants.NumberFormat, out var num))
                continue;

            if (unsignedOnly && num < 0)
            {
                continue;
            }

            numbers.Add(num);
        }

        if (distinct)
        {
            numbers = numbers.Distinct().ToList();
        }

        return numbers.ToArray();
    }


    /// <summary>
    /// Get all controls by type
    /// </summary>
    public static IEnumerable<Control> GetAllControls(Control control, Type type)
    {
        var controls = control.Controls.Cast<Control>();

        return controls.SelectMany(ctrl => GetAllControls(ctrl, type))
                                  .Concat(controls)
                                  .Where(c => c.GetType() == type);
    }


    /// <summary>
    /// Checks if the given Windows version is matched.
    /// </summary>
    public static bool IsOS(WindowsOS ver)
    {
        if (ver == WindowsOS.Win11_22H2_OrLater)
        {
            return Environment.OSVersion.Version.Major >= 10
                && Environment.OSVersion.Version.Build >= 22621;
        }

        if (ver == WindowsOS.Win11OrLater)
        {
            return Environment.OSVersion.Version.Major >= 10
                && Environment.OSVersion.Version.Build >= 22000;
        }

        if (ver == WindowsOS.Win10)
        {
            return Environment.OSVersion.Version.Major == 10
                && Environment.OSVersion.Version.Build < 22000;
        }

        if (ver == WindowsOS.Win10OrLater)
        {
            return Environment.OSVersion.Version.Major >= 10;
        }

        if (ver == WindowsOS.Win7)
        {
            return Environment.OSVersion.Version.Major == 6
                && Environment.OSVersion.Version.Minor == 1;
        }

        return false;
    }


    /// <summary>
    /// Checks if the OS is Windows 10 or greater or equals the given build number.
    /// </summary>
    /// <param name="build">Build number of Windows.</param>
    public static bool IsOSBuildOrGreater(int build = -1)
    {
        return Environment.OSVersion.Version.Major >= 10
            && Environment.OSVersion.Version.Build >= build;
    }


    /// <summary>
    /// Sort image list.
    /// </summary>
    public static IEnumerable<string> SortImageList(IEnumerable<string> fileList,
        ImageOrderBy orderBy, ImageOrderType orderType, bool groupByDir)
    {
        // NOTE: relies on LocalSetting.ActiveImageLoadingOrder been updated first!

        // KBR 20190605
        // Fix observed limitation: to more closely match the Windows Explorer's sort
        // order, we must sort by the target column, then by name.
        var naturalSortComparer = orderType == ImageOrderType.Desc
                                    ? (IComparer<string>)new ReverseWindowsNaturalSort()
                                    : new WindowsNaturalSort();

        // initiate directory sorter to a comparer that does nothing
        // if user wants to group by directory, we initiate the real comparer
        var directorySortComparer = (IComparer<string>)new IdentityComparer();
        if (groupByDir)
        {
            if (orderType == ImageOrderType.Desc)
            {
                directorySortComparer = new ReverseWindowsDirectoryNaturalSort();
            }
            else
            {
                directorySortComparer = new WindowsDirectoryNaturalSort();
            }
        }

        // KBR 20190605 Fix observed discrepancy: using UTC for create,
        // but not for write/access times

        // Sort image file
        if (orderBy == ImageOrderBy.FileSize)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).Length)
                    .ThenBy(f => f, naturalSortComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).Length)
                    .ThenBy(f => f, naturalSortComparer);
            }
        }

        // sort by CreationTime
        if (orderBy == ImageOrderBy.CreationTime)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).CreationTimeUtc)
                    .ThenBy(f => f, naturalSortComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).CreationTimeUtc)
                    .ThenBy(f => f, naturalSortComparer);
            }
        }

        // sort by Extension
        if (orderBy == ImageOrderBy.Extension)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).Extension)
                    .ThenBy(f => f, naturalSortComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).Extension)
                    .ThenBy(f => f, naturalSortComparer);
            }
        }

        // sort by LastAccessTime
        if (orderBy == ImageOrderBy.LastAccessTime)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).LastAccessTimeUtc)
                    .ThenBy(f => f, naturalSortComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).LastAccessTimeUtc)
                    .ThenBy(f => f, naturalSortComparer);
            }
        }

        // sort by LastWriteTime
        if (orderBy == ImageOrderBy.LastWriteTime)
        {
            if (orderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                    .ThenBy(f => f, naturalSortComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => f, naturalSortComparer)
                    .ThenBy(f => new FileInfo(f).LastWriteTimeUtc);
            }
        }

        // sort by Random
        if (orderBy == ImageOrderBy.Random)
        {
            // NOTE: ignoring the 'descending order' setting
            return fileList.AsParallel()
                .OrderBy(f => f, directorySortComparer)
                .ThenBy(_ => Guid.NewGuid());
        }

        // sort by Name (default)
        return fileList.AsParallel()
            .OrderBy(f => f, directorySortComparer)
            .ThenBy(f => f, naturalSortComparer);
    }



    /// <summary>
    /// Gets selection rectangle from 2 points.
    /// </summary>
    /// <param name="point1">The first point</param>
    /// <param name="point2">The second point</param>
    /// <param name="aspectRatio">Aspect ratio</param>
    /// <param name="limitRect">The rectangle to limit the selection</param>
    public static RectangleF GetSelection(PointF? point1, PointF? point2,
        SizeF aspectRatio, float srcWidth, float srcHeight,
        RectangleF limitRect)
    {
        var selectedArea = new RectangleF();
        var fromPoint = point1 ?? new PointF();
        var toPoint = point2 ?? new PointF();

        if (fromPoint.IsEmpty || toPoint.IsEmpty) return selectedArea;

        // swap fromPoint and toPoint value if toPoint is less than fromPoint
        if (toPoint.X < fromPoint.X)
        {
            var tempX = fromPoint.X;
            fromPoint.X = toPoint.X;
            toPoint.X = tempX;
        }
        if (toPoint.Y < fromPoint.Y)
        {
            var tempY = fromPoint.Y;
            fromPoint.Y = toPoint.Y;
            toPoint.Y = tempY;
        }

        float width = Math.Abs(fromPoint.X - toPoint.X);
        float height = Math.Abs(fromPoint.Y - toPoint.Y);

        selectedArea.X = fromPoint.X;
        selectedArea.Y = fromPoint.Y;
        selectedArea.Width = width;
        selectedArea.Height = height;

        // limit the selected area to the limitRect
        selectedArea.Intersect(limitRect);


        // free aspect ratio
        if (aspectRatio.Width <= 0 || aspectRatio.Height <= 0)
            return selectedArea;


        var wRatio = aspectRatio.Width / aspectRatio.Height;
        var hRatio = aspectRatio.Height / aspectRatio.Width;

        // update selection size according to the ratio
        if (wRatio > hRatio)
        {
            selectedArea.Height = selectedArea.Width / wRatio;

            if (selectedArea.Bottom >= limitRect.Bottom)
            {
                var maxHeight = limitRect.Bottom - selectedArea.Y;
                selectedArea.Width = maxHeight * wRatio;
                selectedArea.Height = maxHeight;
            }
        }
        else
        {
            selectedArea.Width = selectedArea.Height / hRatio;

            if (selectedArea.Right >= limitRect.Right)
            {
                var maxWidth = limitRect.Right - selectedArea.X; ;
                selectedArea.Width = maxWidth;
                selectedArea.Height = maxWidth * hRatio;
            }
        }


        return selectedArea;
    }


}