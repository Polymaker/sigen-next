using Avalonia.Media;

namespace SiGen.UI.LayoutViewer
{
    public class LayoutViewerColorScheme
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

        public static LayoutViewerColorScheme Blueprint
        {
            get
            {
                return new LayoutViewerColorScheme
                {
                    BackgroundColor = Color.Parse("#234475"),
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

        public static LayoutViewerColorScheme LightBlueprint
        {
            get
            {
                return new LayoutViewerColorScheme
                {
                    BackgroundColor = Color.Parse("#234475"),//234475  2E5489
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

        public static LayoutViewerColorScheme DarkMode
        {
            get
            {
                return new LayoutViewerColorScheme
                {
                    BackgroundColor = Color.Parse("#242438"),
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

        public static LayoutViewerColorScheme LightMode
        {
            get
            {
                return new LayoutViewerColorScheme
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
