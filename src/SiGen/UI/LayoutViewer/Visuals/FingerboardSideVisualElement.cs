using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using SiGen.Layouts;
using SiGen.Layouts.Elements;
using SiGen.Settings;
using SiGen.Utilities;

namespace SiGen.UI.LayoutViewer.Visuals
{
    /// <summary>
    /// Visual element for rendering a fingerboard side (edge) in the layout viewer.
    /// </summary>
    public class FingerboardSideVisualElement : VisualElementBase<FingerboardSideElement>
    {
        private Line? _edgeLine;

        public FingerboardSideVisualElement(FingerboardSideElement element, ThemeRenderSettings themeRenderSettings)
            : base(element, themeRenderSettings)
        {
        }

        /// <summary>
        /// Generates the visuals for the fingerboard side.
        /// </summary>
        protected override void GenerateVisuals()
        {
            Children.Clear();
            _edgeLine = new Line
            {
                Stroke = new SolidColorBrush(ThemeRenderSettings.FingerBoardEdgeColor),
                StrokeThickness = 1.5,
                StartPoint = Element.Path.Start.ToAvalonia(),
                EndPoint = Element.Path.End.ToAvalonia()
            };
            Children.Add(_edgeLine);
        }

        /// <summary>
        /// Updates the theme for the fingerboard side visual.
        /// </summary>
        public override void UpdateTheme(ThemeRenderSettings theme)
        {
            base.UpdateTheme(theme);
            if (_edgeLine != null)
                _edgeLine.Stroke = new SolidColorBrush(ThemeRenderSettings.FingerBoardEdgeColor);
        }
    }
}