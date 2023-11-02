// ImageListView - A listview control for image files
// Copyright (C) 2009 Ozgur Ozcitak
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Ozgur Ozcitak (ozcitak@yahoo.com)
//
// WIC support coded by Jens

using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
#if USEWIC
using System.Windows.Media.Imaging;
#endif

namespace ImageGlass.ImageListView {
    /// <summary>
    /// Read metadata.
    /// Only EXIF data when using .NET 2.0 methods.
    /// Prioritized EXIF/XMP/ICC/etc. data when using WIC/WPF methods.
    /// </summary>
    internal class MetadataExtractor {
        #region Exif Tag IDs
        private const int TagImageDescription = 0x010E;
        private const int TagEquipmentModel = 0x0110;
        private const int TagDateTimeOriginal = 0x9003;
        private const int TagArtist = 0x013B;
        private const int TagCopyright = 0x8298;
        private const int TagExposureTime = 0x829A;
        private const int TagFNumber = 0x829D;
        private const int TagISOSpeed = 0x8827;
        private const int TagUserComment = 0x9286;
        private const int TagRating = 0x4746;
        private const int TagRatingPercent = 0x4749;
        private const int TagEquipmentManufacturer = 0x010F;
        private const int TagFocalLength = 0x920A;
        private const int TagSoftware = 0x0131;
        #endregion

#if USEWIC
        #region WIC Metadata Paths
        private static readonly string[] WICPathImageDescription = new string[] { "/app1/ifd/{ushort=40095}", "/app1/ifd/{ushort=270}" };
        private static readonly string[] WICPathCopyright = new string[] { "/app1/ifd/{ushort=33432}", "/app13/irb/8bimiptc/iptc/copyright notice", "/xmp/<xmpalt>dc:rights", "/xmp/dc:rights" };
        private static readonly string[] WICPathComment = new string[] { "/app1/ifd/{ushort=40092}", "/app1/ifd/{ushort=37510}", "/xmp/<xmpalt>exif:UserComment" };
        private static readonly string[] WICPathSoftware = new string[] { "/app1/ifd/{ushort=305}", "/xmp/xmp:CreatorTool", "/xmp/xmp:creatortool", "/xmp/tiff:Software", "/xmp/tiff:software", "/app13/irb/8bimiptc/iptc/Originating Program" };
        private static readonly string[] WICPathSimpleRating = new string[] { "/app1/ifd/{ushort=18246}", "/xmp/xmp:Rating" };
        private static readonly string[] WICPathRating = new string[] { "/app1/ifd/{ushort=18249}", "/xmp/MicrosoftPhoto:Rating" };
        private static readonly string[] WICPathArtist = new string[] { "/app1/ifd/{ushort=315}", "/app13/irb/8bimiptc/iptc/by-line", "/app1/ifd/{ushort=40093}", "/xmp/tiff:artist" };
        private static readonly string[] WICPathEquipmentManufacturer = new string[] { "/app1/ifd/{ushort=271}", "/xmp/tiff:Make", "/xmp/tiff:make" };
        private static readonly string[] WICPathEquipmentModel = new string[] { "/app1/ifd/{ushort=272}", "/xmp/tiff:Model", "/xmp/tiff:model" };
        private static readonly string[] WICPathDateTaken = new string[] { "/app1/ifd/exif/{ushort=36867}", "/app13/irb/8bimiptc/iptc/date created", "/xmp/xmp:CreateDate", "/app1/ifd/exif/{ushort=36868}", "/app13/irb/8bimiptc/iptc/date created", "/xmp/exif:DateTimeOriginal" };
        private static readonly string[] WICPathExposureTime = new string[] { "/app1/ifd/exif/{ushort=33434}", "/xmp/exif:ExposureTime" };
        private static readonly string[] WICPathFNumber = new string[] { "/app1/ifd/exif/{ushort=33437}", "/xmp/exif:FNumber" };
        private static readonly string[] WICPathISOSpeed = new string[] { "/app1/ifd/exif/{ushort=34855}", "/xmp/<xmpseq>exif:ISOSpeedRatings", "/xmp/exif:ISOSpeed" };
        private static readonly string[] WICPathFocalLength = new string[] { "/app1/ifd/exif/{ushort=37386}", "/xmp/exif:FocalLength" };
        #endregion
#endif

        #region Exif Format Conversion
        /// <summary>
        /// Converts the given Exif data to an ASCII encoded string.
        /// </summary>
        /// <param name="value">Exif data as a byte array.</param>
        private static string ExifAscii(byte[] value) {
            if (value == null || value.Length == 0)
                return string.Empty;

            string str = Encoding.ASCII.GetString(value);
            str = str.Trim(new char[] { '\0' });
            return str;
        }
        /// <summary>
        /// Converts the given Exif data to DateTime.
        /// </summary>
        /// <param name="value">Exif data as a byte array.</param>
        private static DateTime ExifDateTime(byte[] value) {
            return ExifDateTime(ExifAscii(value));
        }
        /// <summary>
        /// Converts the given Exif data to DateTime.
        /// Value must be formatted as yyyy:MM:dd HH:mm:ss.
        /// </summary>
        /// <param name="value">Exif data as a string.</param>
        private static DateTime ExifDateTime(string value) {
            try {
                // Don't throw unnecessary FormatExceptions
                DateTime dt;
                var converted = DateTime.TryParseExact(value,
                    "yyyy:MM:dd HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out dt);
                return converted ? dt : DateTime.MinValue;
                //return DateTime.ParseExact(value,
                //    "yyyy:MM:dd HH:mm:ss",
                //    System.Globalization.CultureInfo.InvariantCulture);
            }
            catch {
                return DateTime.MinValue;
            }
        }
        /// <summary>
        /// Converts the given Exif data to an 16-bit unsigned integer.
        /// The value must have 2 bytes.
        /// </summary>
        /// <param name="value">Exif data as a byte array.</param>
        private static ushort ExifUShort(byte[] value) {
            return BitConverter.ToUInt16(value, 0);
        }
        /// <summary>
        /// Converts the given Exif data to an 32-bit unsigned integer.
        /// The value must have 4 bytes.
        /// </summary>
        /// <param name="value">Exif data as a byte array.</param>
        private static uint ExifUInt(byte[] value) {
            return BitConverter.ToUInt32(value, 0);
        }
        /// <summary>
        /// Converts the given Exif data to an 32-bit signed integer.
        /// The value must have 4 bytes.
        /// </summary>
        /// <param name="value">Exif data as a byte array.</param>
        private static int ExifInt(byte[] value) {
            return BitConverter.ToInt32(value, 0);
        }
        /// <summary>
        /// Converts the given Exif data to an unsigned rational value
        /// represented as a string.
        /// The value must have 8 bytes.
        /// </summary>
        /// <param name="value">Exif data as a byte array.</param>
        private static string ExifURational(byte[] value) {
            return BitConverter.ToUInt32(value, 0).ToString() + "/" +
                    BitConverter.ToUInt32(value, 4).ToString();
        }
        /// <summary>
        /// Converts the given Exif data to a signed rational value
        /// represented as a string.
        /// The value must have 8 bytes.
        /// </summary>
        /// <param name="value">Exif data as a byte array.</param>
        private static string ExifRational(byte[] value) {
            return BitConverter.ToInt32(value, 0).ToString() + "/" +
                    BitConverter.ToInt32(value, 4).ToString();
        }
        /// <summary>
        /// Converts the given Exif data to a double number.
        /// The value must have 8 bytes.
        /// </summary>
        /// <param name="value">Exif data as a byte array.</param>
        private static double ExifDouble(byte[] value) {
            uint num = BitConverter.ToUInt32(value, 0);
            uint den = BitConverter.ToUInt32(value, 4);
            if (den == 0)
                return 0.0;
            else
                return num / (double)den;
        }
        #endregion

        #region Metadata properties
        /// <summary>
        /// Error.
        /// </summary>
        public Exception Error = null;
        /// <summary>
        /// Image width.
        /// </summary>
        public int Width = 0;
        /// <summary>
        /// Image height.
        /// </summary>
        public int Height = 0;
        /// <summary>
        /// Horizontal DPI.
        /// </summary>
        public double DPIX = 0.0;
        /// <summary>
        /// Vertical DPI.
        /// </summary>
        public double DPIY = 0.0;
        /// <summary>
        /// Date taken.
        /// </summary>
        public DateTime DateTaken = DateTime.MinValue;
        /// <summary>
        /// Image description (null = not available).
        /// </summary>
        public string ImageDescription = null;
        /// <summary>
        /// Camera manufacturer (null = not available).
        /// </summary>
        public string EquipmentManufacturer = null;
        /// <summary>
        /// Camera model (null = not available).
        /// </summary>
        public string EquipmentModel = null;
        /// <summary>
        /// Image creator (null = not available).
        /// </summary>
        public string Artist = null;
        /// <summary>
        /// Iso speed rating.
        /// </summary>
        public int ISOSpeed = 0;
        /// <summary>
        /// Exposure time.
        /// </summary>
        public double ExposureTime = 0.0;
        /// <summary>
        /// F number.
        /// </summary>
        public double FNumber = 0.0;
        /// <summary>
        /// Copyright information (null = not available).
        /// </summary>
        public string Copyright = null;
        /// <summary>
        /// Rating value between 0-99.
        /// </summary>
        public int Rating = 0;
        /// <summary>
        /// User comment (null = not available).
        /// </summary>
        public string Comment = null;
        /// <summary>
        /// Software used (null = not available).
        /// </summary>
        public string Software = null;
        /// <summary>
        /// Focal length.
        /// </summary>
        public double FocalLength = 0.0;
        #endregion

        #region Helper Methods
        /// <summary>
        /// Inits metadata via WIC/WPF (.NET 3.0).
        /// If WIC lacks a metadata reader for this image type then fall back to .NET 2.0 method. 
        /// </summary>
        /// <param name="path">Filepath of image</param>
        private void InitViaWpf(string path) {
            bool wicError = false;
#if USEWIC
            try {
                using (FileStream streamWpf = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    BitmapFrame frameWpf = BitmapFrame.Create
                            (streamWpf,
                             BitmapCreateOptions.IgnoreColorProfile,
                             BitmapCacheOption.None);
                    InitViaWpf(frameWpf);
                }
            }
            catch (Exception eWpf) {
                Error = eWpf;
                wicError = true;
            }
#else
            wicError = true;
#endif
            if (wicError) {
                try {
                    // Fall back to .NET 2.0 method.
                    InitViaBmp(path);
                }
                catch (Exception eBmp) {
                    Error = eBmp;
                }
            }
        }
#if USEWIC
        /// <summary>
        /// Inits metadata via WIC/WPF (.NET 3.0).
        /// </summary>
        /// <param name="frameWpf">Opened WPF image</param>
        private void InitViaWpf(BitmapFrame frameWpf) {
            Width = frameWpf.PixelWidth;
            Height = frameWpf.PixelHeight;
            DPIX = frameWpf.DpiX;
            DPIY = frameWpf.DpiY;

            BitmapMetadata data = frameWpf.Metadata as BitmapMetadata;
            if (data != null)
                InitViaWpf(data);
        }
#endif
        /// <summary>
        /// Open image and read metadata (.NET 2.0).
        /// </summary>
        /// <param name="path">Filepath of image</param>
        private void InitViaBmp(string path) {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                if (Utility.IsImage(stream)) {
                    using (Image img = Image.FromStream(stream, false, false)) {
                        if (img != null) {
                            InitViaBmp(img);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Read metadata using .NET 2.0 methods.
        /// </summary>
        /// <param name="img">Opened image</param>
        private void InitViaBmp(Image img) {
            Width = img.Width;
            Height = img.Height;
            DPIX = img.HorizontalResolution;
            DPIY = img.VerticalResolution;

            double dVal;
            int iVal;
            DateTime dateTime;
            string str;
            foreach (PropertyItem prop in img.PropertyItems) {
                if (prop.Value != null && prop.Value.Length != 0) {
                    switch (prop.Id) {
                        case TagImageDescription:
                            str = ExifAscii(prop.Value).Trim();
                            if (str != String.Empty) {
                                ImageDescription = str;
                            }
                            break;
                        case TagArtist:
                            str = ExifAscii(prop.Value).Trim();
                            if (str != String.Empty) {
                                Artist = str;
                            }
                            break;
                        case TagEquipmentManufacturer:
                            str = ExifAscii(prop.Value).Trim();
                            if (str != String.Empty) {
                                EquipmentManufacturer = str;
                            }
                            break;
                        case TagEquipmentModel:
                            str = ExifAscii(prop.Value).Trim();
                            if (str != String.Empty) {
                                EquipmentModel = str;
                            }
                            break;
                        case TagDateTimeOriginal:
                            dateTime = ExifDateTime(prop.Value);
                            if (dateTime != DateTime.MinValue) {
                                DateTaken = dateTime;
                            }
                            break;
                        case TagExposureTime:
                            if (prop.Value.Length == 8) {
                                dVal = ExifDouble(prop.Value);
                                if (dVal != 0.0) {
                                    ExposureTime = dVal;
                                }
                            }
                            break;
                        case TagFNumber:
                            if (prop.Value.Length == 8) {
                                dVal = ExifDouble(prop.Value);
                                if (dVal != 0.0) {
                                    FNumber = dVal;
                                }
                            }
                            break;
                        case TagISOSpeed:
                            if (prop.Value.Length == 2) {
                                iVal = ExifUShort(prop.Value);
                                if (iVal != 0) {
                                    ISOSpeed = iVal;
                                }
                            }
                            break;
                        case TagCopyright:
                            str = ExifAscii(prop.Value);
                            if (str != String.Empty) {
                                Copyright = str;
                            }
                            break;
                        case TagRating:
                            if (Rating == 0 && prop.Value.Length == 2) {
                                iVal = ExifUShort(prop.Value);
                                if (iVal == 1)
                                    Rating = 1;
                                else if (iVal == 2)
                                    Rating = 25;
                                else if (iVal == 3)
                                    Rating = 50;
                                else if (iVal == 4)
                                    Rating = 75;
                                else if (iVal == 5)
                                    Rating = 99;
                            }
                            break;
                        case TagRatingPercent:
                            if (prop.Value.Length == 2) {
                                iVal = ExifUShort(prop.Value);
                                Rating = iVal;
                            }
                            break;
                        case TagUserComment:
                            str = ExifAscii(prop.Value);
                            if (str != String.Empty) {
                                Comment = str;
                            }
                            break;
                        case TagSoftware:
                            str = ExifAscii(prop.Value).Trim();
                            if (str != String.Empty) {
                                Software = str;
                            }
                            break;
                        case TagFocalLength:
                            if (prop.Value.Length == 8) {
                                dVal = ExifDouble(prop.Value);
                                if (dVal != 0.0) {
                                    FocalLength = dVal;
                                }
                            }
                            break;
                    }
                }
            }
        }

#if USEWIC
        /// <summary>
        /// Read metadata via WIC/WPF.
        /// </summary>
        /// <param name="data">metadata</param>
        private void InitViaWpf(BitmapMetadata data) {
            Object val;

            // Subject
            val = GetMetadataObject(data, WICPathImageDescription);
            if (val != null)
                ImageDescription = val as string;
            // Copyright
            val = GetMetadataObject(data, WICPathCopyright);
            if (val != null)
                Copyright = val as string;
            // Comment
            val = GetMetadataObject(data, WICPathComment);
            if (val != null)
                Comment = val as string;
            // Software
            val = GetMetadataObject(data, WICPathSoftware);
            if (val != null)
                Software = val as string;
            // Simple rating
            val = GetMetadataObject(data, WICPathSimpleRating);
            if (val != null) {
                //ushort simpleRating = (ushort)val;
                ushort simpleRating = Convert.ToUInt16(val);

                if (simpleRating == 1)
                    Rating = 1;
                else if (simpleRating == 2)
                    Rating = 25;
                else if (simpleRating == 3)
                    Rating = 50;
                else if (simpleRating == 4)
                    Rating = 75;
                else if (simpleRating == 5)
                    Rating = 99;
            }
            // Rating
            val = GetMetadataObject(data, WICPathRating);
            if (val != null) {
                var a = val as Array;
                if (a != null)
                    Rating = Convert.ToInt32(a.GetValue(0));
                else
                    Rating = Convert.ToInt32(val);
                //Rating = (int)((ushort)val);
            }
            // Authors
            val = GetMetadataObject(data, WICPathArtist);
            if (val != null) {
                if (val is string)
                    Artist = (string)val;
                else if (val is System.Collections.Generic.IEnumerable<string>) {
                    int i = 0;
                    StringBuilder authors = new StringBuilder();
                    foreach (string author in (System.Collections.Generic.IEnumerable<string>)val) {
                        if (i != 0)
                            authors.Append(";");
                        authors.Append(authors);
                        i++;
                    }
                    Artist = authors.ToString();
                }
            }

            // Camera manufacturer
            val = GetMetadataObject(data, WICPathEquipmentManufacturer);
            if (val != null)
                EquipmentManufacturer = val as string;
            // Camera model
            val = GetMetadataObject(data, WICPathEquipmentModel);
            if (val != null)
                EquipmentModel = val as string;

            // Date taken
            val = GetMetadataObject(data, WICPathDateTaken);
            if (val != null)
                DateTaken = ExifDateTime((string)val);
            // Exposure time
            val = GetMetadataObject(data, WICPathExposureTime);
            if (val != null)
                ExposureTime = ExifDouble(BitConverter.GetBytes((ulong)val));
            // FNumber
            val = GetMetadataObject(data, WICPathFNumber);
            if (val != null)
                FNumber = ExifDouble(BitConverter.GetBytes((ulong)val));
            // ISOSpeed
            val = GetMetadataObject(data, WICPathISOSpeed);
            if (val != null) {
                var a = val as Array;
                if (a != null)
                    ISOSpeed = Convert.ToUInt16(a.GetValue(0));
                else
                    ISOSpeed = Convert.ToUInt16(val);
            }
            // FocalLength
            val = GetMetadataObject(data, WICPathFocalLength);
            if (val != null)
                FocalLength = ExifDouble(BitConverter.GetBytes((ulong)val));
        }
        /// <summary>
        /// [PHAP] Returns the metadata for the given query.
        /// </summary>
        /// <param name="metadata">The image metadata.</param>
        /// <param name="query">A list of query strings.</param>
        /// <returns>Metadata object or null if the metadata is not found.</returns>
        private object GetMetadataObject(BitmapMetadata metadata, params string[] query) {
            try {
                foreach (string q in query) {
                    object val = metadata.GetQuery(q);
                    if (val != null)
                        return val;
                }
            }
            catch (Exception) {
                return null;
            }
            return null;
        }
#endif
        /// <summary>
        /// Convert FileTime to DateTime.
        /// </summary>
        /// <param name="ft">FileTime</param>
        /// <returns>DateTime</returns>
        private DateTime ConvertFileTime(System.Runtime.InteropServices.ComTypes.FILETIME ft) {
            long longTime = (((long)ft.dwHighDateTime) << 32) | ((uint)ft.dwLowDateTime);
            return DateTime.FromFileTimeUtc(longTime); // using UTC???
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MetadataExtractor class.
        /// </summary>
        private MetadataExtractor() {
            ;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates an instance of the MetadataExtractor class.
        /// Reads metadata via WIC/WPF (.NET 3.0).
        /// If WIC lacks a metadata reader for this image type then fall back to .NET 2.0 method. 
        /// </summary>
        /// <param name="path">Filepath of image</param>
        public static MetadataExtractor FromFile(string path) {
            return MetadataExtractor.FromFile(path, true);
        }
        /// <summary>
        /// Creates an instance of the MetadataExtractor class.
        /// Reads metadata via WIC/WPF (.NET 3.0).
        /// If WIC lacks a metadata reader for this image type then fall back to .NET 2.0 method. 
        /// </summary>
        /// <param name="path">Filepath of image</param>
        /// <param name="useWic">true to use Windows Imaging Component; otherwise false.</param>
        public static MetadataExtractor FromFile(string path, bool useWic) {
            MetadataExtractor metadata = new MetadataExtractor();
#if USEWIC
            if (useWic)
                metadata.InitViaWpf(path);
            else
                metadata.InitViaBmp(path);
#else
            metadata.InitViaBmp(path);
#endif
            return metadata;
        }
#if USEWIC
        /// <summary>
        /// Creates an instance of the MetadataExtractor class.
        /// Reads metadata via WIC/WPF (.NET 3.0).
        /// If WIC lacks a metadata reader for this image type then fall back to .NET 2.0 method. 
        /// </summary>
        /// <param name="frameWpf">Opened WPF image</param>
        public static MetadataExtractor FromBitmap(BitmapFrame frameWpf) {
            MetadataExtractor metadata = new MetadataExtractor();
            metadata.InitViaWpf(frameWpf);
            return metadata;
        }
#endif
        #endregion
    }
}
