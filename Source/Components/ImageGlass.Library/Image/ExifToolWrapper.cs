using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

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
        /// Execute Exif tool to retrieve data
        /// </summary>
        /// <param name="imageFilename"></param>
        /// <param name="removeWhiteSpaceInTagNames"></param>
        public void LoadExifData(string imageFilename, bool removeWhiteSpaceInTagNames = false) {
            // exiftool command
            var toolPath = this.ToolPath + " ";

            if (removeWhiteSpaceInTagNames)
                toolPath += "-s ";
            toolPath += "-fast -G -t -m -q -q ";
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
        /// This method saves EXIF data to an external file (<file>.exif). Only tags with group EXIF are saved.
        /// </summary>
        /// <param name="sourceImage">Source Image file path</param>
        /// <param name="destinationExifFile">Destination .exif file path</param>
        /// <returns>True if no error</returns>
        public bool SaveExifData(string sourceImage, string destinationExifFile) {
            // exiftool command
            var toolPath = this.ToolPath + " ";
            toolPath += "-fast -m -q -q -tagsfromfile ";
            toolPath += "\"" + sourceImage + "\" -exif ";
            toolPath += "\"" + destinationExifFile + "\"";

            var (_, stdErr) = Open(toolPath);

            if (stdErr.Contains("Error"))
                return false;

            return true;
        }

        /// <summary>
        /// This method writes EXIF data to the given destination image file (must exist beforehand).
        /// </summary>
        /// <param name="sourceExifFile">Source .exif file</param>
        /// <param name="destinationImage">Destination image path (file must exist)</param>
        /// <returns></returns>
        public bool WriteExifData(string sourceExifFile, string destinationImage) {
            // exiftool command
            var toolPath = this.ToolPath + " ";
            toolPath += "-fast -m -q -q -TagsFromFile ";
            toolPath += "\"" + sourceExifFile + "\"";
            toolPath += " -all:all ";
            toolPath += "\"" + destinationImage + "\"";

            var (_, stdErr) = Open(toolPath);

            if (stdErr.Contains("Error"))
                return false;

            return true;
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

        private (string, string) Open(string cmd) {
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

        #endregion

    }
}
