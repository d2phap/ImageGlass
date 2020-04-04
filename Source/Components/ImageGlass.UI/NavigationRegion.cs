using System;
using System.Drawing;

namespace ImageGlass.UI {

    public enum NavigationRegionType {
        Undefined,
        Left,
        Right,
    };
    public enum NavigationRegionState {
        Hidden,
        Visible,
    };

    public class NavigationRegion {
        public NavigationRegionType Type { get; set; } = NavigationRegionType.Undefined;
        public Rectangle Region { get; set; } = new Rectangle();
        public NavigationRegionState State { get; set; } = NavigationRegionState.Hidden;
        public bool RequireToPaint { get; set; } = false;
    }
}
