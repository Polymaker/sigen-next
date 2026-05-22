using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SiGen.UI.LayoutViewer
{
    [Flags]
    public enum LayoutViewerVisibleItems
    {
        None = 0,
        Frets = 1 << 0,
        FretNumbers = 1 << 1,
        Strings = 1 << 2,
        Medians = 1 << 3,
        Grid = 1 << 4,
        Fingerboard = 1 << 5,
        All = Frets | FretNumbers | Strings | Medians | Grid | Fingerboard
    }
}
