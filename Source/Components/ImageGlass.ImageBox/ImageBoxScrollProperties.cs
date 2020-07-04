using System.ComponentModel;
using System.Windows.Forms;

namespace ImageGlass {
    /// <summary>
    /// Encapsulates properties related to scrolling.
    /// </summary>
    public sealed class ImageBoxScrollProperties {
        #region Constants

        private readonly ScrollBar _scrollBar;

        #endregion

        #region Fields

        private bool _enabled;

        private int _largeChange;

        private int _maximum;

        private int _minimum;

        private int _smallChange;

        private int _value;

        private bool _visible;

        #endregion

        #region Constructors

        internal ImageBoxScrollProperties(ScrollBar scrollBar) {
            _scrollBar = scrollBar;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether the scroll bar can be used on the container.
        /// </summary>
        /// <value><c>true</c> if the scroll bar can be used; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        public bool Enabled {
            get { return _enabled; }
            internal set {
                if (_enabled != value) {
                    _enabled = value;
                    _scrollBar.Enabled = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the distance to move a scroll bar in response to a large scroll command.
        /// </summary>
        /// <value>An <see cref="int"/> describing how far, in pixels, to move the scroll bar in response to a large change.</value>
        [DefaultValue(10)]
        public int LargeChange {
            get { return _largeChange; }
            internal set {
                if (_largeChange != value) {
                    _largeChange = value;
                    _scrollBar.LargeChange = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the upper limit of the scrollable range.
        /// </summary>
        /// <value>An <see cref="int"/> representing the maximum range of the scroll bar.</value>
        [DefaultValue(100)]
        public int Maximum {
            get { return _maximum; }
            internal set {
                if (_maximum != value) {
                    _maximum = value;
                    _scrollBar.Maximum = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the lower limit of the scrollable range.
        /// </summary>
        /// <value>An <see cref="int"/> representing the lower range of the scroll bar.</value>
        [DefaultValue(0)]
        public int Minimum {
            get { return _minimum; }
            internal set {
                if (_minimum != value) {
                    _minimum = value;
                    _scrollBar.Minimum = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the distance to move a scroll bar in response to a small scroll command.
        /// </summary>
        /// <value>An <see cref="int"/> representing how far, in pixels, to move the scroll bar.</value>
        [DefaultValue(1)]
        public int SmallChange {
            get { return _smallChange; }
            internal set {
                if (_smallChange != value) {
                    _smallChange = value;
                    _scrollBar.SmallChange = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a numeric value that represents the current position of the scroll bar box.
        /// </summary>
        /// <value>An <see cref="int"/> representing the position of the scroll bar box, in pixels. </value>
        [Bindable(true)]
        [DefaultValue(0)]
        public int Value {
            get { return _value; }
            internal set {
                if (value < Minimum) {
                    value = Minimum;
                }
                else if (value > Maximum) {
                    value = Maximum;
                }

                if (_value != value) {
                    _value = value;
                    _scrollBar.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the scroll bar can be seen by the user.
        /// </summary>
        /// <value><c>true</c> if it can be seen; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        public bool Visible {
            get { return _visible; }
            internal set {
                if (_visible != value) {
                    _visible = value;
                    _scrollBar.Visible = value;
                }
            }
        }

        // [IG_CHANGE] When setting the ImageBox cursor,
        // the scrollbars also get the cursor. I don't
        // believe this should EVER happen, but to work
        // around this fact, I've created this property.
        // In an ideal world, this cursor management
        // would be handled when the ImageBox.Cursor
        // property is changed.
        // ImageGlass Issue #618
        public Cursor Cursor {
            set {
                if (_scrollBar.Visible)
                    _scrollBar.Cursor = value;
            }
        }
        #endregion
    }
}
