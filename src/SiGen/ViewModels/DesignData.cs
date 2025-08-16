using SiGen.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels
{
    public static class DesignData
    {
        public static StringCountViewModel StringControlViewModel { get; } =
            new StringCountViewModel(new StringCountControl { StringCount = 6, IsLeftHanded = false });
    }
}
