using SiGen.Layouts.Data;
using SiGen.Layouts.Elements;
using SiGen.Measuring;
using SiGen.Paths;

namespace SiGen.Layouts
{
    public class FingerboardSideElement : LayoutElement
    {
        public FingerboardSide Side { get; set; }
        public LinearPath Path { get; set; }

        public FingerboardSideElement(FingerboardSide side, LinearPath path)
        {
            Side = side;
            Path = path;
        }

        protected override void FlipHorizontalCore()
        {
            Path.FlipHorizontal();
        }

        protected override RectangleM? CalculateBoundsCore()
        {
            return RectangleM.BoundingRectangle(PointM.FromVector(Path.Start), PointM.FromVector(Path.End));
        }
    }
}
