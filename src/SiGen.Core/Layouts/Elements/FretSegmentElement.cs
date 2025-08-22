using SiGen.Layouts.Data;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;
using SiGen.Physics;
using System.Drawing;

namespace SiGen.Layouts.Elements
{
    public class FretSegmentElement : LayoutElement
    {
        public const string ELEMENT_TYPE_ID = "FRET";

        public int FretIndex { get; set; }
        //public PointM P1 { get; set; }
        //public PointM P2 { get; set; }
        public FretSegment Segment { get; set; }
        public PathBase? FretShape { get; set; }

        public bool IsNut => Segment.IsNut();
        public bool IsBridge => Segment.IsBridge();

        public int BassStringIndex => Segment.FretPoints.Where(x => !x.IsReference).Select(x => x.StringIndex).Min();
        public int TrebleStringIndex => Segment.FretPoints.Where(x => !x.IsReference).Select(x => x.StringIndex).Max();

        public FretSegmentElement(int fretIndex, FretSegment segment, PathBase path)
        {
            FretIndex = fretIndex;
            FretShape = path;
            Segment = segment;
        }

        protected override void FlipHorizontalCore()
        {
            if (FretShape != null)
                FretShape.FlipHorizontal();
        }

        public bool ContainsString(int index)
        {
            return Segment.ContainsString(index);
        }

        public bool HasFingerboardSide(FingerboardSide side)
        {
            if (side == FingerboardSide.Bass)
                return ContainsString(0);
            else
                return ContainsString((Layout?.NumberOfStrings ?? 1) - 1);
        }

        public bool IsFingerboardEnd(FingerboardEnd end)
        {
            if (end == FingerboardEnd.Nut)
                return IsNut;
            else
                return IsBridge;
        }

        public FretPoint? GetFretPoint(int stringIndex)
        {
            return Segment.FretPoints.FirstOrDefault(x => x.StringIndex == stringIndex);
        }

        protected override RectangleM? CalculateBoundsCore()
        {
            if (FretShape is LinearPath linear)
                return RectangleM.BoundingRectangle(PointM.FromVector(linear.GetFirstPoint()), PointM.FromVector(linear.GetLastPoint()));

            if (FretShape is PolyLinePath polyline)
                return RectangleM.BoundingRectangle(polyline.Points.Select(x => PointM.FromVector(x)));

            return null;
        }

        public LinearPath? GetEdgePath(FingerboardSide side)
        {
            if (Layout == null) return null;

            var firstPt = Segment.FretPoints.First(x => !x.IsReference);
            var lastPt = Segment.FretPoints.Last(x => !x.IsReference);

            if (side == FingerboardSide.Bass)
            {
                if (firstPt.StringIndex == 0)
                    return Layout.GetFingerboardEdge(FingerboardSide.Bass).Path;
                else
                    return Layout.GetStringMedian(firstPt.StringIndex - 1).Path;
            }
            else
            {
                if (lastPt.StringIndex == Layout.NumberOfStrings - 1)
                    return Layout.GetFingerboardEdge(FingerboardSide.Treble).Path;
                else
                    return Layout.GetStringMedian(lastPt.StringIndex).Path;
            }
        }
    
        public VectorD GetVector(FingerboardSide side)
        {
            if (FretShape is LinearPath linear)
            {
                if (side == FingerboardSide.Bass)
                    return linear.Direction * -1d;
                else
                    return linear.Direction;
            }
            else if (FretShape is PolyLinePath polyline)
            {

                if (side == FingerboardSide.Bass)
                    return (polyline.Points[0] - polyline.Points[1]).Normalized;
                else
                    return (polyline.Points[^1] - polyline.Points[^2]).Normalized;
            }
            return default;
        }
    }

    public class FretPoint
    {
        public int FretIndex { get; set; }
        public PointM Position { get; set; }
        public PitchInterval Interval { get; set; }
        
        public int StringIndex { get; set; }

        public bool IsReference { get; set; }
        public bool IsNut { get; set; }
        public bool IsBridge { get; set; }

        public FretPoint(int stringIndex, int fretIndex, PointM position, PitchInterval interval)
        {
            StringIndex = stringIndex;
            FretIndex = fretIndex;
            Position = position;
            Interval = interval;
        }

        public FretPoint Clone()
        {
            return new FretPoint(StringIndex, FretIndex, Position, Interval) { IsReference = true };
        }

        //public FretPoint(FingerboardSide side, int fretIndex, PointM position, NoteInterval interval)
        //{
        //    Side = side;
        //    FretIndex = fretIndex;
        //    Position = position;
        //    Interval = interval;
        //}
    }

    public class FretSegment
    {
        public List<FretPoint> FretPoints { get; set; } = new List<FretPoint>();
        public int Count => FretPoints.Count;

        public int FirstStringIndex => FretPoints.First(x => !x.IsReference).StringIndex;

        public int LastStringIndex => FretPoints.Last(x => !x.IsReference).StringIndex;

        public FretSegment() { }

        public void AddPoint(FretPoint point) { FretPoints.Add(point); }

        public LinearPath GetLineFromLastTwoPoints()
        {
            return new LinearPath(FretPoints[^1].Position.ToVector(), FretPoints[^2].Position.ToVector());
        }

        public void TrimReferencePoints()
        {
            for (int i = 0; i < FretPoints.Count - 1; i++)
            {
                if (!FretPoints[i].IsReference) break;
                if (FretPoints[i + 1].IsReference)
                {
                    FretPoints.RemoveAt(0);
                    i--;
                }
            }
            for (int i = FretPoints.Count - 1; i >= 0; i--)
            {
                if (!FretPoints[i].IsReference) break;
                if (FretPoints[i - 1].IsReference)
                {
                    FretPoints.RemoveAt(FretPoints.Count - 1);
                }
            }
        }

        public bool IsPartialNut()
        {
            var realpoints = FretPoints.Where(x => !x.IsReference);
            return realpoints.Any(x => x.IsNut) && !realpoints.All(x => x.IsNut);
        }

        public bool IsNut()
        {
            var realpoints = FretPoints.Where(x => !x.IsReference);
            return realpoints.All(x => x.IsNut);
        }

        public bool IsBridge()
        {
            var realpoints = FretPoints.Where(x => !x.IsReference);
            return realpoints.All(x => x.IsBridge);
        }

        public bool ContainsString(int index)
        {
            return FretPoints.Where(x => !x.IsReference).Any(x => x.StringIndex == index);
        }
    }
}
