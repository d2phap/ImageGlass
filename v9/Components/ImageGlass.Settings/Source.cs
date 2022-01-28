using ImageGlass.Base;
using Microsoft.Extensions.Configuration;

namespace ImageGlass.Settings;

public class Source
{
    #region Public properties

    /// <summary>
    /// Gets the user config file name.
    /// </summary>
    public static string UserFilename => "igconfig.json";


    /// <summary>
    /// Gets the default config file located.
    /// </summary>
    public static string DefaultFilename => "igconfig.default.json";


    /// <summary>
    /// Gets the admin config file name.
    /// </summary>
    public static string AdminFilename => "igconfig.admin.json";


    /// <summary>
    /// Config file description
    /// </summary>
    public string Description { get; set; } = "ImageGlass configuration file";


    /// <summary>
    /// Config file version
    /// </summary>
    public string Version { get; set; } = "9.0";


    /// <summary>
    /// Gets, sets value indicates that the config file is compatible with this ImageGlass version or not
    /// </summary>
    public bool IsCompatible { get; set; } = true;


    #endregion



    #region Public methods

    /// <summary>
    /// Loads all config files: default, user, command-lines, admin;
    /// then unify configs.
    /// </summary>
    public IConfigurationRoot LoadUserConfigs()
    {
        // filter the command lines begin with '-'
        // example: ImageGlass.exe -FrmMainWidth=900
        var args = Environment.GetCommandLineArgs()
            .Where(cmd => cmd.StartsWith(Constants.CONFIG_CMD_PREFIX))
            .Select(cmd => cmd[1..]) // trim '-' from the command
            .ToArray();

        var userConfig = new ConfigurationBuilder()
          .SetBasePath(App.ConfigDir(PathType.Dir))
          .AddJsonFile(DefaultFilename, optional: true)
          .AddJsonFile(UserFilename, optional: true)
          .AddCommandLine(args)
          .AddJsonFile(AdminFilename, optional: true)
          .Build();

        return userConfig;
    }


    #endregion


}