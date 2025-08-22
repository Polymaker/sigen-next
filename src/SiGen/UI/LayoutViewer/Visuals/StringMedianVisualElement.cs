using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Settings;
using SiGen.Utilities;

namespace SiGen.UI.LayoutViewer.Visuals
{
    public class StringMedianVisualElement : VisualElementBase<StringMedianElement>
    {
        private Line? _line;

        public StringMedianVisualElement(StringMedianElement element, ThemeRenderSettings theme)
            : base(element, theme)
        {
            
        }

        protected override void GenerateVisuals()
        {
            Children.Clear();
            _line = new Line
            {
                Stroke = new SolidColorBrush(ThemeRenderSettings.GuideLineColor),
                StrokeThickness = 1,
                StartPoint = Element.Path.Start.ToAvalonia(),
                EndPoint = Element.Path.End.ToAvalonia(),
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 8, 4, 2, 4 }
            };
            Children.Add(_line);
        }

        public override void UpdateTheme(ThemeRenderSettings theme)
        {
            base.UpdateTheme(theme);
            if (_line != null)
                _line.Stroke = new SolidColorBrush(theme.GuideLineColor);
        }
    }
}