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

