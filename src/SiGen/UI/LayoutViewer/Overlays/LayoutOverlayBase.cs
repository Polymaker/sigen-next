using Avalonia.Controls;
using SiGen.Settings;
using System;

namespace SiGen.UI.LayoutViewer.Overlays
{
    public abstract class LayoutOverlayBase : Panel, ILayoutOverlay
    {
        protected ThemeRenderSettings ThemeRenderSettings { get; private set; }
        protected LayoutOverlayBase(ThemeRenderSettings themeRenderSettings)
        {
            ThemeRenderSettings = themeRenderSettings ?? throw new ArgumentNullException(nameof(themeRenderSettings));
        }
        public abstract void Reposition(IOverlayPositionHelper positionHelper);
        public virtual void UpdateTheme(ThemeRenderSettings theme)
        {
            ThemeRenderSettings = theme;
            // Derived overlays should update brushes/colors here
        }
    }
}
