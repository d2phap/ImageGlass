using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGlass.UI;

public interface IColors
{
    public Color GreyBackground { get; }
    public Color HeaderBackground { get; }
    public Color BlueBackground { get; }
    public Color DarkBlueBackground { get; }
    public Color DarkBackground { get; }
    public Color MediumBackground { get; }
    public Color LightBackground { get; }
    public Color LighterBackground { get; }
    public Color LightestBackground { get; }
    public Color LightBorder { get; }
    public Color DarkBorder { get; }
    public Color LightText { get; }
    public Color DisabledText { get; }
    public Color BlueHighlight { get; }
    public Color BlueSelection { get; }
    public Color GreyHighlight { get; }
    public Color GreySelection { get; }
    public Color DarkGreySelection { get; }
    public Color DarkBlueBorder { get; }
    public Color LightBlueBorder { get; }
    public Color ActiveControl { get; }
}
