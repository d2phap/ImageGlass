using ImageGlass.Base;
using System.ComponentModel;

namespace ImageGlass.UI;

public class ModernLabel : Label
{

    private bool _autoUpdateHeight;
    private bool _isGrowing;
    private bool _darkMode = false;

    private ModernControlState _controlState = ModernControlState.Normal;
    private IColors ColorPalatte => ThemeUtils.GetThemeColorPalatte(_darkMode);


    #region Property Region

    /// <summary>
    /// Toggles dark mode for this <see cref="ModernButton"/> control.
    /// </summary>
    public bool DarkMode
    {
        get => _darkMode;
        set
        {
            _darkMode = value;

            Invalidate();
        }
    }


    [Category("Layout")]
    [Description("Enables automatic height sizing based on the contents of the label.")]
    [DefaultValue(false)]
    public bool AutoUpdateHeight
    {
        get { return _autoUpdateHeight; }
        set
        {
            _autoUpdateHeight = value;

            if (_autoUpdateHeight)
            {
                AutoSize = false;
                ResizeLabel();
            }
        }
    }

    #endregion


    public ModernLabel()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

        var textColor = ColorPalatte.LightText;

        if (!Enabled)
        {
            textColor = ColorPalatte.DisabledText;
        }

        using (var b = new SolidBrush(BackColor))
        {
            g.FillRectangle(b, rect);
        }

        using (var b = new SolidBrush(textColor))
        {
            var stringFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Near
            };
            var modRect = new Rectangle(0, 0, rect.Width + (int)g.MeasureString("E", Font).Width, rect.Height);
            g.DrawString(Text, Font, b, modRect, stringFormat);
        }
    }


    private void ResizeLabel()
    {
        if (!_autoUpdateHeight || _isGrowing)
            return;

        try
        {
            _isGrowing = true;
            var sz = new Size(Width, int.MaxValue);
            sz = TextRenderer.MeasureText(Text, Font, sz, TextFormatFlags.WordBreak);
            Height = sz.Height;
        }
        finally
        {
            _isGrowing = false;
        }
    }

}
