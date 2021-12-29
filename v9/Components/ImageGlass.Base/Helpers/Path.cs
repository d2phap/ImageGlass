
namespace ImageGlass.Base;

public partial class Helpers
{
    private const string LONG_PATH_PREFIX = @"\\?\";


    /// <summary>
    /// Check if the given path (file or directory) is writable. 
    /// </summary>
    /// <param name="type">Indicates if the given path is either file or directory</param>
    /// <param name="path">Full path of file or directory</param>
    /// <returns></returns>
    public static bool CheckPathWritable(PathType type, string path)
    {
        try
        {
            // If path is file
            if (type == PathType.File)
            {
                using (File.OpenWrite(path)) { }
            }

            // if path is directory
            else
            {
                var isDirExist = Directory.Exists(path);

                if (!isDirExist)
                {
                    Directory.CreateDirectory(path);
                }

                var sampleFile = Path.Combine(path, "test_write_file.temp");

                using (File.Create(sampleFile)) { }
                File.Delete(sampleFile);

                if (!isDirExist)
                {
                    Directory.Delete(path, true);
                }
            }


            return true;
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// Fallout from Issue #530. To handle a long path name (i.e. a file path
    /// longer than MAX_PATH), a magic prefix is sometimes necessary.
    /// </summary>
    public static string PrefixLongPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;

        if (path.Length > 255 && !path.StartsWith(LONG_PATH_PREFIX))
            return LONG_PATH_PREFIX + path;

        return path;
    }

    /// <summary>
    /// Fallout from Issue #530. Specific functions (currently FileWatch)
    /// fail if provided a prefixed file path. In this case, strip the prefix
    /// (see PrefixLongPath above).
    /// </summary>
    public static string DePrefixLongPath(string path)
    {
        if (path.StartsWith(LONG_PATH_PREFIX))
            return path[LONG_PATH_PREFIX.Length..];
        return path;
    }

}
