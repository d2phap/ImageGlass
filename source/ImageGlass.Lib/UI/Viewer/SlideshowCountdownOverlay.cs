/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Types;
using System.Globalization;

namespace ImageGlass.UI.Viewer;

/// <summary>
/// An overlay that draws only the slideshow countdown text.
/// </summary>
public class SlideshowCountdownOverlay : PhControl
{

    public override void Render(DrawingContext c)
    {
        base.Render(c);

        if (!Core.Config.EnableSlideshow || !Core.Config.EnableSlideshowCountdown) return;
        if (Core.Slideshow is null || !Core.Slideshow.IsRunning) return;

        // 1. format countdown
        var totalSeconds = Core.Slideshow.CountdownSeconds;
        var minutes = (int)(totalSeconds / 60);
        var seconds = (int)(totalSeconds % 60);
        var tenths = (int)(totalSeconds * 10 % 10);

        var text = $"{minutes:D2}:{seconds:D2}.{tenths}";
        if (Core.Slideshow.IsPaused) text = $"▮▮ {text}";

        var typeface = new Typeface(Const.FONT_CODE);
        var fontSize = DpiScale(14d);
        var padding = 20d;


        // 2. draw countdown shadow
        var countDownShadow = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection,
            typeface,
            fontSize,
            Brushes.Black);

        var x = Bounds.Width - countDownShadow.Width - padding;
        var y = Bounds.Height - countDownShadow.Height - padding;
        c.DrawText(countDownShadow, new Point(x + DpiScale(1d), y + DpiScale(1d)));


        // 3. draw countdown
        var countDownText = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection,
            typeface,
            fontSize,
            Brushes.White);

        c.DrawText(countDownText, new Point(x, y));
    }
}
