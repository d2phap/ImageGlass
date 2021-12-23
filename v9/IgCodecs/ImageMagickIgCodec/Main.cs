/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using ImageGlass.Heart;
using ImageMagick.Formats;

namespace ImageMagick.IgCodec;

public class Main : IIgCodec
{
    public string DisplayName => "Magick.NET codec";
    public string Description => "Use ImageMagick.NET to decode file formats";
    public string Author => "Duong Dieu Phap";
    public string Contact => "phap@imageglass.org";
    public Version Version => new(1, 0, 0, 0);
    public Version ApiVersion => new(0, 5);
    public List<string> SupportedExts => new()
    {
        ".3fr",
        ".ari",
        ".arw",
        ".avif",
        ".b64",
        ".bay",
        ".bmp",
        ".cap",
        ".cr2",
        ".cr3",
        ".crw",
        ".cur",
        ".cut",
        ".dcr",
        ".dcs",
        ".dds",
        ".dib",
        ".dng",
        ".drf",
        ".eip",
        ".emf",
        ".erf",
        ".exif",
        ".exr",
        ".fax",
        ".fff",
        ".gif",
        ".gpr",
        ".hdr",
        ".heic",
        ".heif",
        ".ico",
        ".iiq",
        ".jfif",
        ".jp2",
        ".jpe",
        ".jpeg",
        ".jpg",
        ".jxl",
        ".k25",
        ".kdc",
        ".mdc",
        ".mef",
        ".mos",
        ".mrw",
        ".nef",
        ".nrw",
        ".obm",
        ".orf",
        ".pbm",
        ".pcx",
        ".pef",
        "pdf",
        ".pgm",
        ".png",
        ".ppm",
        ".psb",
        ".psd",
        ".ptx",
        ".pxn",
        ".r3d",
        ".raf",
        ".raw",
        ".rw2",
        ".rwl",
        ".rwz",
        ".sr2",
        ".srf",
        ".srw",
        ".svg",
        ".tga",
        ".tif",
        ".tiff",
        ".webp",
        ".wmf",
        ".wpg",
        ".x3f",
        ".xbm",
        ".xpm",
    };


    private MagickReadSettings ParseSettings(CodecReadOptions options, string filename = "")
    {
        var ext = Path.GetExtension(filename).ToUpperInvariant();
        var settings = new MagickReadSettings();

        if (ext == ".SVG")
        {
            settings.BackgroundColor = MagickColors.Transparent;
            settings.SetDefine("svg:xml-parse-huge", "true");
        }

        if (options.Width > 0 && options.Height > 0)
        {
            settings.Width = options.Width;
            settings.Height = options.Height;
        }

        // Fixed #708: length and filesize do not match
        settings.SetDefines(new BmpReadDefines
        {
            IgnoreFileSize = true,
        });

        // Fix RAW color
        settings.SetDefines(new DngReadDefines()
        {
            UseCameraWhitebalance = true,
            OutputColor = DngOutputColor.AdobeRGB,
            ReadThumbnail = true,
        });


        return settings;
    }


    public IgMetadata LoadMetadata(string filename, CodecReadOptions options = default)
    {
        var settings = ParseSettings(options, filename);

        using var imgC = new MagickImageCollection();
        imgC.Ping(filename, settings);

        using var imgM = new MagickImage();
        imgM.Ping(filename, settings);

        return new()
        {
            Width = imgM.Width,
            Height = imgM.Height,
            FrameCount = imgC.Count,
            ActiveFrame = 0,
            AnimationDelay = imgM.AnimationDelay,
            AnimationIterations = imgM.AnimationIterations,
            AnimationTicksPerSecond = imgM.AnimationTicksPerSecond,
            CanAnimate = imgM.AnimationTicksPerSecond > 0,
        };
    }


    public IgPhoto Load(string filename, CodecReadOptions options = default)
    {
        var settings = ParseSettings(options, filename);
        var photo = new IgPhoto();

        if (options.LoadAllFrames)
        {
            using var imgC = new MagickImageCollection();
            imgC.Read(filename, settings);

            foreach (var imgM in imgC)
            {
                photo.AllFrames.Add(imgM.ToBitmap());
            }

            return photo;
        }
        else
        {
            using var imgM = new MagickImage();
            imgM.Read(filename, settings);

            photo.FirstFrame = imgM.ToBitmap();

            return photo;
        }
    }

    public async Task<IgPhoto> LoadAsync(string filename,
        CodecReadOptions options = default,
        CancellationToken token = default)
    {
        var settings = ParseSettings(options, filename);
        var photo = new IgPhoto();

        if (options.LoadAllFrames)
        {
            using var imgC = new MagickImageCollection();
            await imgC.ReadAsync(filename, settings, token);

            foreach (var imgM in imgC)
            {
                photo.AllFrames.Add(imgM.ToBitmap());
            }

            return photo;
        }
        else
        {
            using var imgM = new MagickImage();
            await imgM.ReadAsync(filename, settings, token);

            photo.FirstFrame = imgM.ToBitmap();

            return photo;
        }
    }



    public byte[] GetThumbnail(string filename, int width, int height)
    {
        byte[] result = Array.Empty<byte>();
        using var imgM = new MagickImage();


        try
        {
            imgM.Ping(filename);
        }
        // exit on invalid image
        catch { return result; }


        // try to get RAW thumbnail
        var rawProfile = imgM.GetProfile("dng:thumbnail");

        if (rawProfile is not null)
        {
            var imgBytes = rawProfile?.GetData();

            if (imgBytes is not null)
            {
                using var rawImgM = new MagickImage(imgBytes);

                if (imgM.BaseWidth > width || imgM.BaseHeight > height)
                {
                    rawImgM.Thumbnail(width, height);
                    result = rawImgM.ToByteArray(MagickFormat.WebP);
                }
            }
        }


        // cannot find raw thumbnail, try to create a thumbnail
        if (result.Length == 0)
        {
            // read entire file content
            imgM.Read(filename);

            try
            {
                var profile = imgM.GetExifProfile();
                using var thumbM = profile?.CreateThumbnail();

                if (thumbM is not null)
                {
                    result = thumbM.ToByteArray(MagickFormat.WebP);
                }
            }
            catch { }
        }


        // cannot create thumbnail, resize the image file
        if (result.Length == 0)
        {
            if (imgM.BaseWidth > width || imgM.BaseHeight > height)
            {
                imgM.Thumbnail(width, height);
            }

            result = imgM.ToByteArray(MagickFormat.WebP);
        }

        return result;
    }


    public string GetThumbnailBase64(string filename, int width, int height)
    {
        var bytes = GetThumbnail(filename, width, height);

        if (bytes.Length > 0)
        {
            using var imgM = new MagickImage(bytes);

            return "data:image/webp;base64," + imgM.ToBase64(MagickFormat.WebP);
        }

        return string.Empty;
    }

    
}
