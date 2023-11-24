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
along with this program.  If not, see <https://www.gnu.org/licenses/>.

---------------------
This class is based on Bumpkit library of DataDink
Url: https://github.com/DataDink/Bumpkit/blob/c20d49a67c6c7c72a1cc5c77771b3210cfe670e4/BumpKit/BumpKit/GifEncoder.cs
License: Unlicense license, https://github.com/DataDink/Bumpkit/blob/c20d49a67c6c7c72a1cc5c77771b3210cfe670e4/LICENSE
---------------------
*/
using System.Drawing.Imaging;

namespace ImageGlass.Base.Photoing.Codecs;


/// <summary>
/// Encodes multiple images as an animated gif to a stream using default
/// .NET GIF encoding with animation headers.<br/>
/// <c>Disposing the encoder will complete the file.</c>
/// </summary>
public class GifEncoder : IDisposable
{
    #region IDisposable Disposing

    public bool IsDisposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
        {
            // Free any other managed objects here.

            // Complete File
            WriteByte(FileTrailer);

            // Pushing data
            _stream.Flush();
        }

        // Free any unmanaged objects here.
        IsDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~GifEncoder()
    {
        Dispose(false);
    }

    #endregion


    // Header Constants
    #region Header Constants
    private const string FileType = "GIF";
    private const string FileVersion = "89a";
    private const byte FileTrailer = 0x3b;

    private const int ApplicationExtensionBlockIdentifier = 0xff21;
    private const byte ApplicationBlockSize = 0x0b;
    private const string ApplicationIdentification = "NETSCAPE2.0";

    private const int GraphicControlExtensionBlockIdentifier = 0xf921;
    private const byte GraphicControlExtensionBlockSize = 0x04;

    private const long SourceGlobalColorInfoPosition = 10;
    private const long SourceGraphicControlExtensionPosition = 781;
    private const long SourceGraphicControlExtensionLength = 8;
    private const long SourceImageBlockPosition = 789;
    private const long SourceImageBlockHeaderLength = 11;
    private const long SourceColorBlockPosition = 13;
    private const long SourceColorBlockLength = 768;
    #endregion // Header Constants


    private bool _isFirstImage = true;
    private int? _width;
    private int? _height;
    private int? _repeatCount;
    private readonly Stream _stream;


    // Public Accessors
    public TimeSpan FrameDelay { get; set; }


    /// <summary>
    /// Encodes multiple images as an animated gif to a stream using default .net GIF
    /// encoding with animation headers.<br/>
    /// <c>Disposing the encoder will complete the file.</c>
    /// </summary>
    /// <param name="destStream">The stream that will be written to.</param>
    /// <param name="width">Sets the width for this gif or null to use the first frame's width.</param>
    /// <param name="height">Sets the height for this gif or null to use the first frame's height.</param>
    public GifEncoder(Stream destStream, int? width = null, int? height = null, int? repeatCount = null)
    {
        _stream = destStream;
        _width = width;
        _height = height;
        _repeatCount = repeatCount;
    }


    /// <summary>
    /// Adds a frame to this animation.
    /// </summary>
    /// <param name="img">The image to add</param>
    /// <param name="x">The positioning x offset this image should be displayed at.</param>
    /// <param name="y">The positioning y offset this image should be displayed at.</param>
    public void AddFrame(Image img, int x = 0, int y = 0, TimeSpan? frameDelay = null)
    {
        using (var gifStream = new MemoryStream())
        {
            img.Save(gifStream, ImageFormat.Gif);
            if (_isFirstImage) // Steal the global color table info
            {
                InitHeader(gifStream, img.Width, img.Height);
            }
            WriteGraphicControlBlock(gifStream, frameDelay.GetValueOrDefault(FrameDelay));
            WriteImageBlock(gifStream, !_isFirstImage, x, y, img.Width, img.Height);
        }
        _isFirstImage = false;
    }


    // Private methods
    #region Private methods

    private void InitHeader(Stream sourceGif, int w, int h)
    {
        // File Header
        WriteString(FileType);
        WriteString(FileVersion);
        WriteShort(_width.GetValueOrDefault(w)); // Initial Logical Width
        WriteShort(_height.GetValueOrDefault(h)); // Initial Logical Height
        sourceGif.Position = SourceGlobalColorInfoPosition;
        WriteByte(sourceGif.ReadByte()); // Global Color Table Info
        WriteByte(0); // Background Color Index
        WriteByte(0); // Pixel aspect ratio
        WriteColorTable(sourceGif);

        // App Extension Header
        WriteShort(ApplicationExtensionBlockIdentifier);
        WriteByte(ApplicationBlockSize);
        WriteString(ApplicationIdentification);
        WriteByte(3); // Application block length
        WriteByte(1);
        WriteShort(_repeatCount.GetValueOrDefault(0)); // Repeat count for images.
        WriteByte(0); // terminator
    }


    private void WriteColorTable(Stream sourceGif)
    {
        sourceGif.Position = SourceColorBlockPosition; // Locating the image color table
        var colorTable = new byte[SourceColorBlockLength];
        sourceGif.Read(colorTable, 0, colorTable.Length);
        _stream.Write(colorTable, 0, colorTable.Length);
    }


    private void WriteGraphicControlBlock(Stream sourceGif, TimeSpan frameDelay)
    {
        sourceGif.Position = SourceGraphicControlExtensionPosition; // Locating the source GCE
        var blockhead = new byte[SourceGraphicControlExtensionLength];
        sourceGif.Read(blockhead, 0, blockhead.Length); // Reading source GCE

        WriteShort(GraphicControlExtensionBlockIdentifier); // Identifier
        WriteByte(GraphicControlExtensionBlockSize); // Block Size
        WriteByte(blockhead[3] & 0xf7 | 0x08); // Setting disposal flag
        WriteShort(Convert.ToInt32(frameDelay.TotalMilliseconds / 10)); // Setting frame delay
        WriteByte(blockhead[6]); // Transparent color index
        WriteByte(0); // Terminator
    }


    private void WriteImageBlock(Stream sourceGif, bool includeColorTable, int x, int y, int h, int w)
    {
        sourceGif.Position = SourceImageBlockPosition; // Locating the image block
        var header = new byte[SourceImageBlockHeaderLength];
        sourceGif.Read(header, 0, header.Length);
        WriteByte(header[0]); // Separator
        WriteShort(x); // Position X
        WriteShort(y); // Position Y
        WriteShort(h); // Height
        WriteShort(w); // Width

        if (includeColorTable) // If first frame, use global color table - else use local
        {
            sourceGif.Position = SourceGlobalColorInfoPosition;
            WriteByte(sourceGif.ReadByte() & 0x3f | 0x80); // Enabling local color table
            WriteColorTable(sourceGif);
        }
        else
        {
            WriteByte(header[9] & 0x07 | 0x07); // Disabling local color table
        }

        WriteByte(header[10]); // LZW Min Code Size

        // Read/Write image data
        sourceGif.Position = SourceImageBlockPosition + SourceImageBlockHeaderLength;

        var dataLength = sourceGif.ReadByte();
        while (dataLength > 0)
        {
            var imgData = new byte[dataLength];
            sourceGif.Read(imgData, 0, dataLength);

            _stream.WriteByte(Convert.ToByte(dataLength));
            _stream.Write(imgData, 0, dataLength);
            dataLength = sourceGif.ReadByte();
        }

        _stream.WriteByte(0); // Terminator

    }

    private void WriteByte(int value)
    {
        _stream.WriteByte(Convert.ToByte(value));
    }


    private void WriteShort(int value)
    {
        _stream.WriteByte(Convert.ToByte(value & 0xff));
        _stream.WriteByte(Convert.ToByte((value >> 8) & 0xff));
    }


    private void WriteString(string value)
    {
        _stream.Write(value.ToArray().Select(c => (byte)c).ToArray(), 0, value.Length);
    }

    #endregion // Private methods

}
