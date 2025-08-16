using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;

namespace SiGen.Layouts.Builders
{
    public class FingerBoardEdgesBuilder : LayoutBuilderBase
    {
        public FingerBoardEdgesBuilder(StringedInstrumentLayout layout, InstrumentLayoutConfiguration configuration) : base(layout, configuration)
        {
        }

        public override void BuildLayoutCore()
        {
            var bassEdge = CreateSideElement(FingerboardSide.Bass);
            var trebEdge = CreateSideElement(FingerboardSide.Treble);

            Layout.AddElement(bassEdge);
            Layout.AddElement(trebEdge);

            var vertLine = new LineD(0, bassEdge.Path.Start.Y);
            if (trebEdge.Path.GetEquation().Intersects(vertLine, out var bassIntersection))
            {
                var center = (bassEdge.Path.Start + bassIntersection) / 2d;
            }
        }

        private FingerboardSideElement CreateSideElement(FingerboardSide side)
        {
            var @string = Layout.GetStringElement(side);

            var nutMargin = Configuration.Margin.GetMargin(Data.FingerboardEnd.Nut, side);
            var bridgeMargin = Configuration.Margin.GetMargin(Data.FingerboardEnd.Bridge, side);

            var startPt = @string.StartPoint;
            var endPt = @string.BridgePoint;

            var nutPerpLine = @string.Path.GetEquation().GetPerpendicular(@string.NutPoint.ToVector());
            var bridgePerpLine = @string.Path.GetEquation().GetPerpendicular(@string.BridgePoint.ToVector());


            if (side == FingerboardSide.Bass)
            {
                PreciseDouble offset = 0;
                var stringWidth = Configuration.StringConfigurations[0].GetTotalWidth();
                if (Configuration.Margin.CompensateForStrings && !Measure.IsNullOrEmpty(stringWidth))
                    offset = stringWidth.NormalizedValue / 2d;
                startPt -= PointM.FromVector(nutPerpLine.Vector * (nutMargin.NormalizedValue + offset));
                endPt -= PointM.FromVector(bridgePerpLine.Vector * (bridgeMargin.NormalizedValue + offset));
            }
            else
            {
                PreciseDouble offset = 0;
                var stringWidth = Configuration.StringConfigurations[^1].GetTotalWidth();
                if (Configuration.Margin.CompensateForStrings && !Measure.IsNullOrEmpty(stringWidth))
                    offset = stringWidth.NormalizedValue / 2d;
                startPt += PointM.FromVector(nutPerpLine.Vector * (nutMargin.NormalizedValue + offset));
                endPt += PointM.FromVector(bridgePerpLine.Vector * (bridgeMargin.NormalizedValue + offset));
            }

            //if (side == FingerboardSide.Bass)
            //{
            //    startPt -= new PointM(nutMargin, Measure.Zero);
            //    endPt -= new PointM(bridgeMargin, Measure.Zero);
            //}
            //else
            //{
            //    startPt += new PointM(nutMargin, Measure.Zero);
            //    endPt += new PointM(bridgeMargin, Measure.Zero);
            //}

            var edgePath = new Paths.LinearPath(startPt.ToVector(), endPt.ToVector());

            if (Configuration.NumberOfStrings > 1)
            {
                

                /*
                var secondString = Layout.GetStringElement(side, 1);
                var nutLine = new LinearPath(@string.NutPoint.ToVector(), secondString.NutPoint.ToVector());
                var bridgeLine = new LinearPath(@string.BridgePoint.ToVector(), secondString.BridgePoint.ToVector());
                if (edgePath.Intersects(nutLine, out var inter, true))
                    edgePath.Start = inter;
                if (edgePath.Intersects(bridgeLine, out inter, true))
                    edgePath.End = inter;
                */
            }

            return new FingerboardSideElement(side, edgePath);
        }
    }
}
