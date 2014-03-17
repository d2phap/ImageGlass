using System.ComponentModel;

// Original ScrollControl code by Scott Crawford (http://sukiware.com/)

namespace ImageGlass
{
  partial class ScrollControl
  {
    #region Nested Types

    /// <summary>
    /// Provides basic properties for the horizontal scroll bar in a <see cref="ScrollControl"/>.
    /// </summary>
    public class HScrollProperties : ScrollProperties
    {
      #region Public Constructors

      /// <summary>
      /// Initializes a new instance of the <see cref="ScrollProperties" /> class.
      /// </summary>
      /// <param name="container">The <see cref="ScrollControl" /> whose scrolling properties this object describes.</param>
      public HScrollProperties(ScrollControl container)
        : base(container)
      { }

      #endregion
    }

    /// <summary>
    /// Encapsulates properties related to scrolling.
    /// </summary>
    public abstract class ScrollProperties
    {
      #region Instance Fields

      private readonly ScrollControl _container;

      #endregion

      #region Protected Constructors

      /// <summary>
      /// Initializes a new instance of the <see cref="ScrollProperties"/> class.
      /// </summary>
      /// <param name="container">The <see cref="ScrollControl"/> whose scrolling properties this object describes.</param>
      protected ScrollProperties(ScrollControl container)
      {
        //System.Windows.Forms.ScrollProperties
        _container = container;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Gets or sets whether the scroll bar can be used on the container.
      /// </summary>
      /// <value><c>true</c> if the scroll bar can be used; otherwise, <c>false</c>.</value>
      [DefaultValue(true)]
      public bool Enabled { get; set; }

      /// <summary>
      /// Gets or sets the distance to move a scroll bar in response to a large scroll command.
      /// </summary>
      /// <value>An <see cref="int"/> describing how far, in pixels, to move the scroll bar in response to a large change.</value>
      [DefaultValue(10)]
      public int LargeChange { get; set; }

      /// <summary>
      /// Gets or sets the upper limit of the scrollable range.
      /// </summary>
      /// <value>An <see cref="int"/> representing the maximum range of the scroll bar.</value>
      [DefaultValue(100)]
      public int Maximum { get; set; }

      /// <summary>
      /// Gets or sets the lower limit of the scrollable range.
      /// </summary>
      /// <value>An <see cref="int"/> representing the lower range of the scroll bar.</value>
      [DefaultValue(0)]
      public int Minimum { get; set; }

      /// <summary>
      /// Gets the control to which this scroll information applies.
      /// </summary>
      /// <value>A <see cref="ScrollControl"/>.</value>
      public ScrollControl ParentControl
      {
        get { return _container; }
      }

      /// <summary>
      /// Gets or sets the distance to move a scroll bar in response to a small scroll command.
      /// </summary>
      /// <value>An <see cref="int"/> representing how far, in pixels, to move the scroll bar.</value>
      [DefaultValue(1)]
      public int SmallChange { get; set; }

      /// <summary>
      /// Gets or sets a numeric value that represents the current position of the scroll bar box.
      /// </summary>
      /// <value>An <see cref="int"/> representing the position of the scroll bar box, in pixels. </value>
      [Bindable(true)]
      [DefaultValue(0)]
      public int Value { get; set; }

      /// <summary>
      /// Gets or sets whether the scroll bar can be seen by the user.
      /// </summary>
      /// <value><c>true</c> if it can be seen; otherwise, <c>false</c>.</value>
      [DefaultValue(false)]
      public bool Visible { get; set; }

      #endregion
    }

    /// <summary>
    /// Provides basic properties for the vertical scroll bar in a <see cref="ScrollControl"/>.
    /// </summary>
    public class VScrollProperties : ScrollProperties
    {
      #region Public Constructors

      /// <summary>
      /// Initializes a new instance of the <see cref="ScrollProperties" /> class.
      /// </summary>
      /// <param name="container">The <see cref="ScrollControl" /> whose scrolling properties this object describes.</param>
      public VScrollProperties(ScrollControl container)
        : base(container)
      { }

      #endregion
    }

    #endregion
  }
}
