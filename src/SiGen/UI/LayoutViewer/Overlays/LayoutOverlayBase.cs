using Avalonia.Controls;
using SiGen.Settings;
using System;

namespace SiGen.UI.LayoutViewer.Overlays
{
    public abstract class LayoutOverlayBase : Panel, ILayoutOverlay
    {
        protected LayoutViewerColorScheme ColorScheme { get; private set; }

        protected LayoutOverlayBase(LayoutViewerColorScheme colorScheme)
        {
            ColorScheme = colorScheme ?? throw new ArgumentNullException(nameof(colorScheme));
        }

        public abstract void Reposition(IOverlayPositionHelper positionHelper);

        public virtual void UpdateTheme(LayoutViewerColorScheme theme)
        {
            ColorScheme = theme;
            // Derived overlays should update brushes/colors here
        }
    }
}
