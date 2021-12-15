using System.Reflection;

namespace ImageGlass.Heart;

public class CodecManager
{
    public List<IIgCodec> Items { get; set; } = new();


    /// <summary>
    /// Gets a codec from the list
    /// </summary>
    /// <param name="filename">Filename of codec. Example: MyCodec.dll</param>
    /// <returns></returns>
    public IIgCodec? Get(string filename)
    {
        var codec = Items.Find(item => string.Compare(item.Filename, filename, true) == 0);

        return codec;
    }


    /// <summary>
    /// Loads all *.IgCodec.dll files and all codecs inside.
    /// </summary>
    /// <param name="codecDir"></param>
    public void LoadAllCodecs(string codecDir)
    {
        Directory.CreateDirectory(codecDir);
        var files = Directory.GetFiles(codecDir, "*.IgCodec.dll", SearchOption.TopDirectoryOnly);


        Items = files.SelectMany(path =>
        {
            var pluginAssembly = LoadPlugin(path);

            return CreateCodecs(pluginAssembly);
        }).ToList();
    }


    /// <summary>
    /// Loads all codecs in the assembly file
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    private static IEnumerable<IIgCodec> CreateCodecs(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(IIgCodec).IsAssignableFrom(type))
            {
                if (Activator.CreateInstance(type) is IIgCodec result)
                {
                    yield return result;
                }
            }
        }
    }

    private static Assembly LoadPlugin(string relativePath)
    {
        // Navigate up to the solution root
        var root = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(typeof(CodecManager).Assembly.Location)))))));

        var pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));

        var loadContext = new DllLoadContext(pluginLocation);

        return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
    }
}

