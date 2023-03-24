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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace ImageGlass.Library.Image {
    public struct ExifTagItem {
        public string Group;
        public string Name;
        public string Value;
    }


    public class ExifToolWrapper: List<ExifTagItem> {
        /// <summary>
        /// Gets, sets the path of Exiftool executable file.
        /// </summary>
        public string ToolPath { get; set; } = "exiftool";

        /// <summary>
        /// Gets default commands to pass to Exiftool.
        /// </summary>
        public static string DefaultCommands => "-fast -G -t -m -q -H";


        /// <summary>
        /// Initialize new instance of <see cref="ExifTool"/>.
        /// </summary>
        public ExifToolWrapper(string toolPath = "") {
            ToolPath = toolPath;
        }


        // Public methods
        #region Public methods

        /// <summary>
        /// Check if Exif tool exists
        /// </summary>
        public async Task<bool> CheckExistAsync() {
            try {
                var cmd = Cli.Wrap(ToolPath);
                var cmdResult = await cmd
                        .WithArguments("-ver")
                        .ExecuteBufferedAsync(Encoding.UTF8);

                // check the output
                if (cmdResult.StandardOutput.Length < 4 || cmdResult.StandardError.Length > 0)
                    return false;

                // (could check version number here if you care)
                return true;
            }
            catch {
                return false;
            }
        }


        /// <summary>
        /// Reads file's metadata.
        /// </summary>
        /// <param name="filePath">Path of file to read.</param>
        /// <param name="exifToolCmd">Additional commands for Exiftool.</param>
        public async Task ReadAsync(
            string filePath,
            CancellationToken cancelToken = default,
            params string[] exifToolCmd) {
            var cmdOutput = string.Empty;
            var pathContainsUnicode = CheckAndPurifyUnicodePath(filePath, out var cleanPath);

            try {
                var cmd = Cli.Wrap(ToolPath);
                var cmdResult = await cmd
                    .WithArguments($"{DefaultCommands} {string.Join(" ", exifToolCmd)} \"{cleanPath}\"")
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteBufferedAsync(Encoding.UTF8, cancelToken);

                cmdOutput = cmdResult.StandardOutput;
            }
            finally {
                // delete temporary file
                if (pathContainsUnicode) {
                    try {
                        File.Delete(cleanPath);
                    }
                    catch { }
                }
            }

            ParseExifTags(cmdOutput, Path.GetFileName(filePath));
        }



        /// <summary>
        /// Write exif data to file
        /// </summary>
        /// <param name="destFilename"></param>
        /// <returns></returns>
        public async Task ExportToFileAsync(string destFilename) {
            using var sw = new StreamWriter(destFilename);

            // find the longest Property in exif list
            var propMaxLength = this.Max(item => item.Name.Length);
            var currentGroup = "";

            foreach (var item in this) {
                var itemLine = item.Name.PadRight(propMaxLength + 5) + ":".PadRight(4) + item.Value;

                // write group heading
                if (item.Group != currentGroup) {
                    var groupLine = item.Group.PadRight(propMaxLength + 5, '-') + ":";
                    if (currentGroup.Length > 0) {
                        groupLine = "\n" + groupLine;
                    }

                    await sw.WriteLineAsync(groupLine);

                    currentGroup = item.Group;
                }

                // write exif item
                await sw.WriteLineAsync(itemLine);
            }

            await sw.FlushAsync();
            sw.Close();
        }

        #endregion // Public methods


        // Private methods
        #region Private methods

        /// <summary>
        /// Parses Exiftool's command-line output.
        /// </summary>
        private void ParseExifTags(string cmdOutput, string originalFileName) {
            var index = 0;
            Clear();

            while (cmdOutput.Length > 0) {
                var epos = cmdOutput.IndexOf('\r');
                if (epos < 0) epos = cmdOutput.Length;

                var tmp = cmdOutput.Substring(0, epos);
                var tpos1 = tmp.IndexOf('\t');
                var tpos2 = tmp.IndexOf('\t', tpos1 + 1);
                var tpos3 = tmp.IndexOf('\t', tpos2 + 1);

                if (tpos1 > 0 && tpos2 > 0) {
                    var tagGroup = tmp.Substring(0, tpos1);
                    ++tpos1;

                    var tagId = tmp.Substring(tpos1, tpos2 - tpos1);
                    ++tpos2;

                    var tagName = tmp.Substring(tpos2, tpos3 - tpos2);
                    ++tpos3;

                    var tagValue = tmp.Substring(tpos3, tmp.Length - tpos3);


                    // special processing for tags with binary data 
                    tpos1 = tagValue.IndexOf(", use -b option to extract");
                    if (tpos1 >= 0)
                        _ = tagValue.Remove(tpos1, 26);

                    // 
                    if (tagName.Equals("File Name")) tagValue = originalFileName;

                    Add(new ExifTagItem() {
                        Name = tagName,
                        Value = tagValue,
                        Group = tagGroup,
                    });

                    index++;
                }

                // is \r followed by \n ?
                if (epos < cmdOutput.Length)
                    epos += (cmdOutput[epos + 1] == '\n') ? 2 : 1;

                cmdOutput = cmdOutput.Substring(epos, cmdOutput.Length - epos);
            }
        }


        /// <summary>
        /// Purifies <paramref name="filePath"/> if it contains unicode character.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the <paramref name="filePath"/> contains unicode and is purified.
        /// </returns>
        private static bool CheckAndPurifyUnicodePath(string filePath, out string cleanPath) {
            const int MAX_ANSICODE = 255;


            // exiftool does not support unicode filename
            var dirPath = Path.GetDirectoryName(filePath) ?? "";
            var fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
            var ext = Path.GetExtension(filePath);


            // directory has unicode char
            if (filePath.Any(c => c > MAX_ANSICODE)) {
                // copy and rename it
                try {
                    cleanPath = Path.GetTempFileName() + ext;
                    File.Copy(filePath, cleanPath, true);

                    return true;
                }
                catch (Exception) { }
            }

            cleanPath = filePath;
            return false;
        }

        #endregion // Private methods

    }
}
