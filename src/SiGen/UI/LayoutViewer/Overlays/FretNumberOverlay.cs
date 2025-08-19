using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using SiGen.Layouts;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer.Overlays
{
    public class FretNumberOverlay : Panel, ILayoutOverlay
    {
        public int FretNumber { get; private set; }

        private Label fretNumberLabel;

        public FretNumberOverlay(int fretNumber)
        {
            FretNumber = fretNumber;
            fretNumberLabel = new Label()
            {
                Content = fretNumber.ToString(),
            };
            Children.Add(fretNumberLabel);
        }

        public void Reposition(IOverlayPositionHelper positionHelper)
        {
            var fretSegment = positionHelper.Layout.Elements.OfType<FretSegmentElement>()
                .Where(f => f.FretIndex == FretNumber)
                .OrderBy(x => x.BassStringIndex)
                .First();

            var fretPoint = fretSegment.FretShape?.GetFirstPoint();
            if (fretPoint == null)
                return;
            var fretPos = positionHelper.VectorToScreen(fretPoint.Value);

            //var formattedText = new FormattedText(FretNumber.ToString(),
            //    System.Globalization.CultureInfo.CurrentCulture,
            //    FlowDirection.LeftToRight,
            //    new Typeface("Segoe UI"),
            //    14,
            //    Brushes.White);
            
            fretNumberLabel.FontSize = (double)MathD.Map(0.5, 3, 0.6, 1.6, positionHelper.Zoom) * 14;

            fretNumberLabel.Measure(new Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            bool isLeftHanded = positionHelper.Layout.Configuration?.LeftHanded ?? false;

            if ((positionHelper.ViewerOrientation == LayoutOrientation.HorizontalNutRight && isLeftHanded) ||
                (positionHelper.ViewerOrientation == LayoutOrientation.HorizontalNutLeft && !isLeftHanded))
            {
                Canvas.SetTop(this, fretPos.Y + 5);
                Canvas.SetLeft(this, fretPos.X - (fretNumberLabel.DesiredSize.Width / 2));
            }
            else if (positionHelper.ViewerOrientation != LayoutOrientation.Vertical)
            {
                Canvas.SetTop(this, fretPos.Y - fretNumberLabel.DesiredSize.Height - 5);
                Canvas.SetLeft(this, fretPos.X - (fretNumberLabel.DesiredSize.Width / 2));
            }
            else
            {
                if (isLeftHanded)
                    Canvas.SetLeft(this, fretPos.X + 5);
                else
                    Canvas.SetLeft(this, fretPos.X - fretNumberLabel.DesiredSize.Width - 5);
                Canvas.SetTop(this, fretPos.Y - fretNumberLabel.DesiredSize.Height / 2d);
            }



        }

        public static void CreateElements(IOverlayPositionHelper positionHelper, Canvas canvas)
        {
            if (positionHelper?.Layout == null)
                return;

            for (int i = 1; i <= positionHelper.Layout.Configuration!.NumberOfFrets; i++)
            {
                var overlay = new FretNumberOverlay(i);
                canvas.Children.Add(overlay);
                overlay.Reposition(positionHelper);
            }
        }

    }
}
