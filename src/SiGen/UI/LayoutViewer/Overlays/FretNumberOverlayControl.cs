using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using SiGen.Layouts;
using SiGen.Layouts.Data;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Settings;
using SiGen.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer.Overlays
{
    public class FretNumberOverlayControl : Control
    {
        private ILayoutViewerContext ViewerContext { get; }
        private StringedInstrumentLayout? Layout => ViewerContext.Layout;
        public ThemeRenderSettings RenderSettings => ViewerContext.RenderSettings;

        public FretNumberOverlayControl(ILayoutViewerContext context)
        {
            ViewerContext = context;
            context.RenderSettingsChanged += (s, e) =>
            {
                InvalidateVisual();
            };
        }

        public override void Render(DrawingContext context)
        {
            if (Layout == null) return;

            int numberOfFrets = Layout.Configuration!.GetMaxFrets();
            var textBrush = new SolidColorBrush(RenderSettings.OverlayTextColor);

            //var textMargin = new Point(
            //    (double)MathD.Map(0.4, 3, 2, 10, ViewerContext.Zoom),
            //    (double)MathD.Map(0.4, 3, 4, 12, ViewerContext.Zoom)
            //);
            double margin = (double)MathD.Map(0.4, 3, 2, 15, ViewerContext.Zoom);

            double fontSize = (double)MathD.Map(0.5, 3, 0.6, 1.6, ViewerContext.Zoom) * 14;

            void DrawFretNumber(int fretIndex, Point position, FingerboardSide side)
            {
                var formattedText = new FormattedText(
                    fretIndex.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection,
                    Typeface.Default,
                    fontSize,
                    textBrush
                );
                //position -= new Point(formattedText.Width * 0.5, formattedText.Height * 0.5);
                position += GetTextOffset(formattedText, side);
                context.DrawText(formattedText, position);
            }

            for (int i = 1; i <= numberOfFrets; i++)
            {
                var bassSegment = Layout.Elements.OfType<FretSegmentElement>()
                    .Where(f => f.FretIndex == i /*&& f.ContainsString(0)*/)
                    .OrderBy(x => x.BassStringIndex)
                    .FirstOrDefault();

                var trebleSegment = Layout.Elements.OfType<FretSegmentElement>()
                    .Where(f => f.FretIndex == i /*&& f.ContainsString(0)*/)
                    .OrderByDescending(x => x.TrebleStringIndex)
                    .FirstOrDefault();

                if (bassSegment?.FretShape != null)
                {
                    var fretPos = bassSegment.FretShape.GetFirstPoint();

                    var screenPos = ViewerContext.VectorToScreen(fretPos);
                    var offsetVector = CorrectVectorForView(bassSegment.GetVector(FingerboardSide.Bass));
                    screenPos += (offsetVector * margin).ToAvalonia(1);
                    DrawFretNumber(i, screenPos, FingerboardSide.Bass);
                }

                if (trebleSegment?.FretShape != null && Layout.NumberOfStrings > 1)
                {
                    var fretPos = trebleSegment.FretShape.GetLastPoint();

                    var screenPos = ViewerContext.VectorToScreen(fretPos);
                    var offsetVector = CorrectVectorForView(trebleSegment.GetVector(FingerboardSide.Treble));
                    screenPos += (offsetVector * margin).ToAvalonia(1);
                    DrawFretNumber(i, screenPos, FingerboardSide.Treble);
                }
            }
        }

        private VectorD CorrectVectorForView(VectorD vector)
        {
            if (ViewerContext.Orientation == LayoutOrientation.HorizontalNutRight)
            {
                return new VectorD(vector.Y, vector.X );
            }
            else if (ViewerContext.Orientation == LayoutOrientation.HorizontalNutLeft)
            {
                return new VectorD(-vector.Y, -vector.X);
            }
            else
            {
                return new VectorD(vector.X, vector.Y * -1d);
            }
        }

        private Point GetTextOffset(FormattedText text, FingerboardSide side)
        {
            bool isLeftHanded = Layout?.Configuration?.LeftHanded ?? false;
            //if (side == FingerboardSide.Treble)
            //    isLeftHanded = !isLeftHanded;

            if (ViewerContext.Orientation == LayoutOrientation.Vertical)
            {
                bool showTextToTheRight = isLeftHanded && side == FingerboardSide.Bass ||
                                    !isLeftHanded && side == FingerboardSide.Treble;
                return new Point(showTextToTheRight ? 0 : -text.Width, text.Height * -0.5d);
            }

            bool showTextAbove = false;

            if (ViewerContext.Orientation == LayoutOrientation.HorizontalNutRight)
            {
                showTextAbove = (!isLeftHanded && side == FingerboardSide.Bass) || (isLeftHanded && side == FingerboardSide.Treble);
            }
            else if (ViewerContext.Orientation == LayoutOrientation.HorizontalNutLeft)
            {
                showTextAbove = (isLeftHanded && side == FingerboardSide.Bass) || (!isLeftHanded && side == FingerboardSide.Treble);
            }
            return new Point(text.Width * -0.5, showTextAbove ? -text.Height : 0);
            //if ((ViewerContext.Orientation == LayoutOrientation.HorizontalNutRight && isLeftHanded) ||
            //    (ViewerContext.Orientation == LayoutOrientation.HorizontalNutLeft && !isLeftHanded))
            //{
            //    return new Point(text.Width * -0.5, margin.Y);
            //}
            //else if (ViewerContext.Orientation != LayoutOrientation.Vertical)
            //{
            //    return new Point(text.Width * -0.5, (text.Height + margin.Y) * -1d);
            //}
            //else
            //{
            //    //bool showToTheRight = isLeftHanded && side == FingerboardSide.Bass ||
            //    //                     !isLeftHanded && side == FingerboardSide.Treble;
            //    return new Point(isLeftHanded ? margin.X : (text.Width + margin.X) * -1d, text.Height * -0.5d);
            //}
        }
        //private Point GetTextOffset(FormattedText text, Point margin, FingerboardSide side)
        //{
        //    bool isLeftHanded = Layout?.Configuration?.LeftHanded ?? false;
        //    if (side == FingerboardSide.Treble)
        //        isLeftHanded = !isLeftHanded;

        //    if ((ViewerContext.Orientation == LayoutOrientation.HorizontalNutRight && isLeftHanded) ||
        //        (ViewerContext.Orientation == LayoutOrientation.HorizontalNutLeft && !isLeftHanded))
        //    {
        //        return new Point(text.Width * -0.5, margin.Y);
        //    }
        //    else if (ViewerContext.Orientation != LayoutOrientation.Vertical)
        //    {
        //        return new Point(text.Width * -0.5, (text.Height + margin.Y) * -1d);
        //    }
        //    else
        //    {
        //        //bool showToTheRight = isLeftHanded && side == FingerboardSide.Bass ||
        //        //                     !isLeftHanded && side == FingerboardSide.Treble;
        //        return new Point(isLeftHanded ? margin.X : (text.Width + margin.X) * -1d, text.Height * -0.5d);
        //    }
        //}
    }
}
