using SiGen.Layouts.Data;
using SiGen.Layouts.Elements;
using SiGen.Measuring;
using SiGen.Paths;

namespace SiGen.Layouts
{
    public class FingerboardEdgeElement : LayoutElement
    {
        public PathBase Path { get; set; }
        public FingerboardSide? Side { get; set; }
        public int? BesideStringIndex { get; set; }
        public bool IsSide => Side.HasValue;

        public FingerboardEdgeElement(PathBase path, FingerboardSide? side, int? besideStringIndex = null)
        {
            Path = path;
            Side = side;
            BesideStringIndex = besideStringIndex;
        }

        protected override void FlipHorizontalCore()
        {
            Path.FlipHorizontal();
        }

        protected override RectangleM? CalculateBoundsCore()
        {
            return RectangleM.BoundingRectangle(PointM.FromVector(Path.GetFirstPoint()), PointM.FromVector(Path.GetLastPoint()));
        }
    }
}
