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
        public bool IsSide => Side.HasValue;

        public FingerboardEdgeElement(PathBase path, FingerboardSide? side)
        {
            Path = path;
            Side = side;
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
