using SiGen.Paths;

namespace SiGen.Layouts.Elements
{
    public class GuideLineElement : LayoutElement
    {
        public LinearPath Path { get; }

        public GuideLineType Type { get; set; }

        public GuideLineElement(GuideLineType type, LinearPath path)
        {
            Path = path;
            Type = type;
        }

        protected override void FlipHorizontalCore()
        {
            Path.FlipHorizontal();
        }
    }

    public enum GuideLineType
    {
        CenterLine,
        StringMedian,
        FretboardProjection
    }
}