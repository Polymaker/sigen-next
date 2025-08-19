using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Settings
{
    public class ThemeRenderSettings
    {
        public Color BackgroundColor { get; set; } = Colors.White;
        public Color GridColor { get; set; } = Colors.LightGray;
        public Color MajorAxisColor { get; set; } = Colors.Black;
        public Color OverlayTextColor { get; set; } = Colors.Black;

        #region Layout Elements

        public Color StringColor { get; set; } = Colors.Silver;
        public Color FretColor { get; set; } = Colors.Gray;
        public Color FingerBoardEdgeColor { get; set; } = Colors.SaddleBrown;
        public Color NutColor { get; set; } = Colors.Sienna;
        public Color BridgeColor { get; set; } = Colors.Sienna;
        public Color GuideLineColor { get; set; } = Colors.DarkGray;

        #endregion
    }
}
