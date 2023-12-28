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
    public static IConfigurationRoot LoadUserConfigs()
    {
        // filter the command lines begin with '/'
        // example: ImageGlass.exe /FrmMainWidth=900
        var args = Environment.GetCommandLineArgs()
            .Where(cmd => cmd.StartsWith(Const.CONFIG_CMD_PREFIX))
            .Select(cmd => cmd[1..]) // trim '/' from the command
            .ToArray();

        var startUpDir = App.StartUpDir();
        var configDir = App.ConfigDir(PathType.Dir);

        try
        {
            // igconfig.default.json
            var defaultConfig = new ConfigurationBuilder()
                .SetBasePath(startUpDir)
                .AddJsonFile(DefaultFilename, optional: true)
                .Build();

            // admin.igconfig.json
            var adminConfig = new ConfigurationBuilder()
                .SetBasePath(startUpDir)
                .AddJsonFile(AdminFilename, optional: true)
                .Build();

            // final config
            var userConfig = new ConfigurationBuilder()
                .AddConfiguration(defaultConfig)

                // igconfig.json
                .SetBasePath(configDir)
                .AddJsonFile(UserFilename, optional: true)

                // command line
                .AddCommandLine(args)

                .AddConfiguration(adminConfig)
                .Build();

            return userConfig;
        }
        catch { }


        // fall back to default config if user config is invalid
        var fallBackConfig = new ConfigurationBuilder()
            .SetBasePath(startUpDir)
            .AddJsonFile(DefaultFilename, optional: true) // igconfig.default.json
            .AddCommandLine(args) // command line
            .AddJsonFile(AdminFilename, optional: true) // igconfig.admin.json
            .Build();

        return fallBackConfig;
    }


    #endregion


}


public record ConfigMetadata
{
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
