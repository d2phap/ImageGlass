/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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

---------------------
ImageGlass.Gallery is based on ImageListView v13.8.2:
Url: https://github.com/oozcitak/imagelistview
License: Apache License Version 2.0, http://www.apache.org/licenses/
---------------------
*/
using System.Reflection;

namespace ImageGlass.Gallery;


/// <summary>
/// Extracts thumbnails from images.
/// </summary>
internal static class Extractor
{
#if USEWIC
    private static bool useWIC = true;
#else
    private static bool useWIC = false;
#endif
    private static IExtractor? instance = null;

    public static IExtractor Instance
    {
        get
        {
            if (instance == null)
            {
                if (!useWIC)
                {
                    instance = new GDIExtractor();
                }
                else
                {
                    try
                    {
                        var programFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        var pluginFileName = Path.Combine(programFolder, "WPFThumbnailExtractor.dll");

                        instance = LoadFrom(pluginFileName);
                    }
                    catch
                    {
                        instance = new GDIExtractor();
                    }
                }
            }

            if (instance == null)
                instance = new GDIExtractor();

            return instance;
        }
    }

    private static IExtractor? LoadFrom(string pluginFileName)
    {
        var assembly = Assembly.LoadFrom(pluginFileName);
        foreach (Type type in assembly.GetTypes())
        {
            if (type.GetInterfaces().Contains(typeof(IExtractor))
                && !type.IsInterface
                && type.IsClass
                && !type.IsAbstract)
            {
                return (IExtractor?)Activator.CreateInstance(type, Array.Empty<object>());
            }
        }

        return null;
    }

    public static bool UseWIC
    {
        get => useWIC;
        set
        {
#if USEWIC
            useWIC = value;
            instance = null;
#else
            useWIC = false;
            if (value)
            {
                System.Diagnostics.Debug.WriteLine("Trying to set UseWIC option although the library was compiled without WPF/WIC support.");
            }
#endif
        }
    }
}

