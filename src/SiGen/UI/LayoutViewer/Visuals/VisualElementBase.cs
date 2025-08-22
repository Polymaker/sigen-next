using Avalonia.Controls;
using SiGen.Layouts;
using SiGen.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer.Visuals
{
    public abstract class VisualElementBase<T> : Panel where T : LayoutElement
    {
        public T Element { get; private set; }
        protected StringedInstrumentLayout Layout => Element.Layout!;
        protected ThemeRenderSettings ThemeRenderSettings { get; private set; }

        protected VisualElementBase(T element, ThemeRenderSettings themeRenderSettings)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            ThemeRenderSettings = themeRenderSettings ?? throw new ArgumentNullException(nameof(themeRenderSettings));
            GenerateVisuals();
        }

        protected abstract void GenerateVisuals();

        public virtual void UpdateTheme(ThemeRenderSettings theme)
        {
            ThemeRenderSettings = theme;
            // Derived classes should update brushes/colors here
        }
    }
}
