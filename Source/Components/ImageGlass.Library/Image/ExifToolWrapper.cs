/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageGlass.Base;

namespace ImageGlass.Library.Image {
    public struct ExifTagItem {
        public string Group;
        public string Name;
        public string Value;
    }

    public class ExifToolWrapper: List<ExifTagItem> {

        /// <summary>
        /// Gets, sets full path of Exif tool executable file
        /// </summary>
        public string ToolPath { get; set; } = string.Empty;


        /// <summary>
        /// Initialize the instance
        /// </summary>
        /// <param name="toolPath">Full path of Exif tool executable file</param>
        public ExifToolWrapper(string toolPath) {
            this.ToolPath = toolPath;
        }


        #region Public methods

        /// <summary>
        /// Check if Exif tool exists
        /// </summary>
        /// <returns></returns>
        public bool CheckExists() {
            var output = "";
            var toolPath = this.ToolPath + " -ver";

            if (!File.Exists(this.ToolPath)) {
                return false;
            }
            else {
                try {
                    (output, stdErr) = Open(toolPath);
                }
                catch (Exception) {
                }
            }

            // check the output
            if (output.Length < 4 || stdErr.Length > 0)
                return false;

            // (could check version number here if you care)
            return true;
        }


        /// <summary>
        /// Checks if the image has exif data
        /// </summary>
        /// <returns></returns>
        public bool HasExifData() {
            return (this.Count > 0);
        }


        /// <summary>
        /// Finds Exif tag
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public ExifTagItem Find(string tagName) {
            var itemsQuery = from tagItem in this
                             where tagItem.Name == tagName
                             select tagItem;

            return itemsQuery.First();
        }


        /// <summary>
        /// Preprocess unicode filename and load exif data
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task LoadAndProcessExifDataAsync(string filename) {
            const int MAX_ANSICODE = 255;
            const string DATE_FORMAT = "yyyy:MM:dd hh:mm:sszzz";


            await Task.Run(() => {
                var nonUnicodeFilename = filename;
                var containsUnicodeName = filename.Any(c => c > MAX_ANSICODE);

                // if filename contains unicode char
                if (containsUnicodeName) {
                    nonUnicodeFilename = ProcessUnicodeFilename(filename);
                }

                // load exif data
                LoadExifData(nonUnicodeFilename);


                if (containsUnicodeName) {
                    // process exif data
                    var replacements = new Dictionary<string, Func<string>> {
                        { "File Name", () => Path.GetFileName(filename) },
                        { "Directory", () => Path.GetDirectoryName(filename) },
                        { "File Modification Date/Time", () => ImageInfo.GetWriteTime(filename).ToString(DATE_FORMAT) },
                        { "File Access Date/Time", () => ImageInfo.GetLastAccess(filename).ToString(DATE_FORMAT) },
                        { "File Creation Date/Time", () => ImageInfo.GetCreateTime(filename).ToString(DATE_FORMAT) },
                    };

                    for (var i = 0; i < this.Count; i++) {
                        var item = this[i];

                        if (replacements.ContainsKey(item.Name)) {
                            item.Value = replacements[item.Name].Invoke();
                            this[i] = item;
                        }
                    }
                }
            });
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

        #endregion


        #region Private methods

        private string stdOut = null;
        private string stdErr = null;
        private ProcessStartInfo psi = null;
        private Process activeProcess = null;

        private void Thread_ReadStandardError() {
            if (activeProcess != null) {
                stdErr = activeProcess.StandardError.ReadToEnd();
            }
        }

        private void Thread_ReadStandardOut() {
            if (activeProcess != null) {
                stdOut = activeProcess.StandardOutput.ReadToEnd();
            }
        }


        /// <summary>
        /// Execute exif tool command line
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private (string stdOut, string stdErr) Open(string cmd) {
            var program = "\"%COMSPEC%\"";
            var args = "/c [command]";

            this.psi = new ProcessStartInfo(
                Environment.ExpandEnvironmentVariables(program),
                args.Replace("[command]", cmd)
            ) {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var thread_ReadStandardError = new Thread(new ThreadStart(Thread_ReadStandardError));
            var thread_ReadStandardOut = new Thread(new ThreadStart(Thread_ReadStandardOut));

            activeProcess = Process.Start(psi);
            if (psi.RedirectStandardError) {
                thread_ReadStandardError.Start();
            }
            if (psi.RedirectStandardOutput) {
                thread_ReadStandardOut.Start();
            }
            activeProcess.WaitForExit();

            thread_ReadStandardError.Join();
            thread_ReadStandardOut.Join();

            return (stdOut, stdErr);
        }


        /// <summary>
        /// Copy and rename unicode file path to non-unicode path
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string ProcessUnicodeFilename(string filename) {
            // exiftool does not support unicode filename
            var ext = Path.GetExtension(filename);
            var nonUnicodeDir = App.ConfigDir(PathType.Dir, Dir.Temporary);
            var nonUnicodeFilename = Path.Combine(nonUnicodeDir, Guid.NewGuid().ToString("N") + ext);

            try {
                Directory.CreateDirectory(nonUnicodeDir);
                File.Copy(filename, nonUnicodeFilename, true);
            }
            catch (Exception) { }

            return nonUnicodeFilename;
        }


        /// <summary>
        /// Execute Exif tool to retrieve data
        /// </summary>
        /// <param name="imageFilename"></param>
        /// <param name="removeWhiteSpaceInTagNames"></param>
        private void LoadExifData(string imageFilename, bool removeWhiteSpaceInTagNames = false) {
            // exiftool command
            var toolPath = this.ToolPath + " ";

            if (removeWhiteSpaceInTagNames)
                toolPath += "-s ";
            toolPath += "-fast -G -t -m -q ";
            toolPath += "\"" + imageFilename + "\"";

            var (output, _) = Open(toolPath);


            // parse the output into tags
            this.Clear();
            while (output.Length > 0) {
                var epos = output.IndexOf('\r');

                if (epos < 0)
                    epos = output.Length;
                var tmp = output.Substring(0, epos);
                var tpos1 = tmp.IndexOf('\t');
                var tpos2 = tmp.IndexOf('\t', tpos1 + 1);

                if (tpos1 > 0 && tpos2 > 0) {
                    var taggroup = tmp.Substring(0, tpos1);
                    ++tpos1;
                    var tagname = tmp.Substring(tpos1, tpos2 - tpos1);
                    ++tpos2;
                    var tagvalue = tmp.Substring(tpos2, tmp.Length - tpos2);

                    // special processing for tags with binary data 
                    tpos1 = tagvalue.IndexOf(", use -b option to extract");
                    if (tpos1 >= 0)
                        tagvalue.Remove(tpos1, 26);

                    ExifTagItem itm;
                    itm.Name = tagname;
                    itm.Value = tagvalue;
                    itm.Group = taggroup;
                    this.Add(itm);
                }

                // is \r followed by \n ?
                if (epos < output.Length)
                    epos += (output[epos + 1] == '\n') ? 2 : 1;

                output = output.Substring(epos, output.Length - epos);
            }
        }


        #endregion

    }
}
