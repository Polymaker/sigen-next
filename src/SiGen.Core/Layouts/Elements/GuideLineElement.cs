using SiGen.Paths;

namespace SiGen.Layouts.Elements
{
    public class GuideLineElement : LayoutElement
    {
        public LinearPath Path { get; }

        public GuideLineElement(LinearPath path)
        {
            Path = path;
        }

        protected override void FlipHorizontalCore()
        {
            Path.FlipHorizontal();
        }
    }

}
