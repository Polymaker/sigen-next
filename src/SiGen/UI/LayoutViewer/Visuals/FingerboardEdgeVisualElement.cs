using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using SiGen.Layouts;
using SiGen.Layouts.Elements;
using SiGen.Paths;
using SiGen.Settings;
using SiGen.Utilities;
using System.Collections.Generic;

namespace SiGen.UI.LayoutViewer.Visuals
{
    /// <summary>
    /// Visual element for rendering a fingerboard side (edge) in the layout viewer.
    /// </summary>
    public class FingerboardEdgeVisualElement : VisualElementBase<FingerboardEdgeElement>
    {
        private Shape? _edgeLine;

        public FingerboardEdgeVisualElement(FingerboardEdgeElement element, LayoutViewerColorScheme themeRenderSettings)
            : base(element, themeRenderSettings)
        {
        }

        /// <summary>
        /// Generates the visuals for the fingerboard side.
        /// </summary>
        protected override void GenerateVisuals()
        {
            Children.Clear();
            if (Element.Path is LinearPath linearPath)
            {
                _edgeLine = new Line
                {
                    Stroke = new SolidColorBrush(ThemeRenderSettings.FingerBoardEdgeColor),
                    StrokeThickness = 1.5,
                    StartPoint = linearPath.Start.ToAvalonia(),
                    EndPoint = linearPath.End.ToAvalonia()
                };
                Children.Add(_edgeLine);
            }
            else if (Element.Path is PolyLinePath polyPath)
            {
                var points = new List<Point>();
                foreach (var pt in polyPath.Points)
                {
                    points.Add(pt.ToAvalonia());
                }
                _edgeLine = new Polyline
                {
                    Stroke = new SolidColorBrush(ThemeRenderSettings.FingerBoardEdgeColor),
                    StrokeThickness = 1.5,
                    Points = points
                };
                Children.Add(_edgeLine);
            }
        }

        /// <summary>
        /// Updates the theme for the fingerboard side visual.
        /// </summary>
        public override void UpdateColorScheme(LayoutViewerColorScheme theme)
        {
            base.UpdateColorScheme(theme);
            if (_edgeLine != null)
                _edgeLine.Stroke = new SolidColorBrush(ThemeRenderSettings.FingerBoardEdgeColor);
        }
    }
}