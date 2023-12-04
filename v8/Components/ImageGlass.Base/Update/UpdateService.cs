/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 -2023 DUONG DIEU PHAP
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Win32;

namespace ImageGlass.Base.Update {
    public class UpdateService {

        /// <summary>
        /// Gets the update information
        /// </summary>
        public UpdateModel? UpdateInfo { get; private set; }


        /// <summary>
        /// Gets current release information
        /// </summary>
        public ReleaseModel? CurrentReleaseInfo {
            get {
                if (UpdateInfo is null) return null;

                if (UpdateInfo.Releases.ContainsKey(Constants.UPDATE_CHANNEL)) {
                    return UpdateInfo.Releases[Constants.UPDATE_CHANNEL];
                }

                return null;
            }
        }


        /// <summary>
        /// Gets the suitable download package information
        /// </summary>
        public DownloadModel? DownloadInfo {
            get {
                if (CurrentReleaseInfo is null) {
                    return null;
                }

                var architecture = Environment.Is64BitProcess ? "x64" : "x86";
                var extension = App.IsPortable ? "zip" : "msi";

                return CurrentReleaseInfo.Downloads.FirstOrDefault(i =>
                    i.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase)
                    && i.Architecture.Equals(architecture, StringComparison.OrdinalIgnoreCase));
            }
        }


        /// <summary>
        /// Gets the value indicates that the current app has a new update
        /// </summary>
        public bool HasNewUpdate {
            get {
                if (CurrentReleaseInfo == null) {
                    return false;
                }

                var newVersion = new Version(CurrentReleaseInfo.Version);
                var currentVersion = new Version(App.Version);

                return newVersion > currentVersion;
            }
        }


        /// <summary>
        /// Checks the requirements for the update.
        /// </summary>
        public async Task<Dictionary<string, bool>> CheckRequirementsAsync() {
            var list = new Dictionary<string, bool>();
            var newVersion = new Version(CurrentReleaseInfo.Version);
            if (newVersion.Major < 9) return list;

            // Windows 64-bit
            list.Add("64-bit Windows", Environment.Is64BitOperatingSystem);


            // .NET Desktop Runtime versions
            try {
                var cli = Cli.Wrap("dotnet");
                var cmdOutput = await cli.WithArguments("--list-runtimes")
                    .ExecuteBufferedAsync(Encoding.UTF8);

                if (cmdOutput.StandardOutput.Contains("Microsoft.WindowsDesktop.App 8")) {
                    list.Add(".NET Desktop Runtime 8.0", true);
                }
                else {
                    throw new Exception(".NET Desktop Runtime not found");
                }
            }
            catch {
                list.Add(".NET Desktop Runtime", false);
            }


            // WebView2 runtime
            try {
                using var lmKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}", false);
                var installedVersion = lmKey.GetValue("pv", string.Empty).ToString();

                if (string.IsNullOrWhiteSpace(installedVersion)) {
                    using var cuKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}", false);
                    installedVersion = cuKey.GetValue("pv", string.Empty).ToString();
                }

                if (string.IsNullOrWhiteSpace(installedVersion)) {
                    throw new Exception("WebView2 Runtime not found");
                }
                else {
                    list.Add("WebView2 Runtime", true);
                }
            }
            catch {
                list.Add("WebView2 Runtime", false);
            }

            return list;
        }



        /// <summary>
        /// Gets the latest updates
        /// </summary>
        /// <returns></returns>
        public async Task GetUpdatesAsync() {
            var url = $"https://imageglass.org/url/update?channel={Constants.UPDATE_CHANNEL}&version={App.Version}";


            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) {
                return;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            UpdateInfo = await Helpers.ParseJson<UpdateModel>(stream);
        }
    }
}
