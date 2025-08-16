using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer
{
    public enum LayoutViewMode
    {
        Schematic,
        Realistic,
        Blueprint
    }

    public enum LayoutOrientation
    {
        Vertical,             // Vertical view, nut at top
        HorizontalNutLeft,    // Horizontal view, nut at left
        HorizontalNutRight    // Horizontal view, nut at right
    }
}
