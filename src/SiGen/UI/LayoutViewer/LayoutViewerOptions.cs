using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer
{
    public partial class LayoutViewerOptions : ObservableObject
    {
        [ObservableProperty]
        private bool showFrets = true;
        [ObservableProperty]
        private bool showStrings = true;
        [ObservableProperty]
        private bool showMedians = true;

    }
}
