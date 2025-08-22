using Avalonia;
using SiGen.Layouts;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer
{
    public interface ILayoutViewerContext
    {
        StringedInstrumentLayout? Layout { get; }
        double Zoom { get; }
        Point Translation { get; }
        LayoutOrientation Orientation { get; }
        ThemeRenderSettings RenderSettings { get; }

        event EventHandler? RenderSettingsChanged;

        Point PointToScreen(PointM point) => VectorToScreen(point.ToVector());

        Point VectorToScreen(VectorD vector);
    }
}
