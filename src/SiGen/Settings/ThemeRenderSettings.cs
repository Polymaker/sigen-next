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

        public static ThemeRenderSettings Blueprint
        {
            get
            {
                return new ThemeRenderSettings
                {
                    BackgroundColor = Color.Parse("#1E3250"),
                    GridColor = Color.Parse("#EFEFEF"),
                    MajorAxisColor = Colors.White,
                    OverlayTextColor = Colors.White,
                    StringColor = Color.FromRgb(230, 230, 240),
                    FretColor = Colors.Silver,
                    FingerBoardEdgeColor = Colors.White,
                    NutColor = Colors.Sienna,
                    BridgeColor = Colors.Sienna,
                    GuideLineColor = Colors.DarkGray
                };
            }
        }

        public static ThemeRenderSettings DarkMode
        {
            get
            {
                return new ThemeRenderSettings
                {
                    BackgroundColor = Color.Parse("#262626"),
                    GridColor = Colors.White,
                    MajorAxisColor = Colors.White,
                    OverlayTextColor = Colors.White,
                    StringColor = Color.FromRgb(230, 230, 240),
                    FretColor = Colors.Silver,
                    FingerBoardEdgeColor = Colors.White,
                    NutColor = Colors.Sienna,
                    BridgeColor = Colors.Sienna,
                    GuideLineColor = Colors.DarkGray
                };
            }
        }

        public static ThemeRenderSettings LightMode
        {
            get
            {
                return new ThemeRenderSettings
                {
                    BackgroundColor = Colors.White,
                    GridColor = Colors.Gray,
                    MajorAxisColor = Colors.Gray,
                    OverlayTextColor = Colors.Black,
                    StringColor = Color.FromRgb(180, 180, 180),
                    FretColor = Colors.Gray,
                    FingerBoardEdgeColor = Colors.SaddleBrown,
                    NutColor = Colors.Sienna,
                    BridgeColor = Colors.Sienna,
                    GuideLineColor = Colors.DarkGray
                };
            }
        }
    }
}
