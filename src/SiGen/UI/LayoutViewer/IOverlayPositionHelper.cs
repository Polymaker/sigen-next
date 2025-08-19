using Avalonia;
using SiGen.Layouts;
using SiGen.Maths;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer
{
    public interface IOverlayPositionHelper
    {
        StringedInstrumentLayout Layout { get; }

        double Zoom { get; }

        Point PointToScreen(PointM point) => VectorToScreen(point.ToVector());

        Point VectorToScreen(VectorD vector);

        LayoutOrientation ViewerOrientation { get; }
    }
}
