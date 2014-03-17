using System;

namespace ImageGlass
{
  // Cyotek ImageBox
  // Copyright (c) 2010-2014 Cyotek.
  // http://cyotek.com
  // http://cyotek.com/blog/tag/imagebox

  // Licensed under the MIT License. See imagebox-license.txt for the full text.

  // If you use this control in your applications, attribution, donations or contributions are welcome.

  /// <summary>
  /// Specifies the source of an action being performed.
  /// </summary>
  [Flags]
  public enum ImageBoxActionSources
  {
    /// <summary>
    /// Unknown source.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// A user initialized the action.
    /// </summary>
    User = 1
  }
}
