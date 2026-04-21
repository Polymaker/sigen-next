using SiGen.Layouts.Configuration;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;

namespace SiGen.Layouts.Elements
{
    public class StringElement : LayoutElement
    {
        public const string ELEMENT_TYPE_ID = "STRING";
        private BaseStringConfiguration? _configuration;

        public int CourseIndex { get; set; }
        public int? SubIndex { get; set; }
        public PointM StartPoint { get; set; }
        public PointM NutPoint { get; set; }
        public PointM BridgePoint { get; set; }

        public LinearPath Path { get; set; }

        public BaseStringConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                    _configuration = GetConfiguration();
                return _configuration!;
            }
        }

        //public StringProperties StringProperties => Configuration?.string(StringIndex) ?? new StringProperties();

        public StringElement(int stringIndex, PointM p1, PointM p2)
        {
            CourseIndex = stringIndex;
            StartPoint = NutPoint = p1;
            BridgePoint = p2;
            Path = new LinearPath(p1.ToVector(), p2.ToVector());
        }

        public StringElement(int stringIndex, LinearPath linePath)
        {
            CourseIndex = stringIndex;
            StartPoint = NutPoint = PointM.FromVector(linePath.Start);
            BridgePoint = PointM.FromVector(linePath.End);
            Path = linePath;
        }

        public StringElement(int stringIndex, int subIndex, LinearPath linePath)
        {
            CourseIndex = stringIndex;
            SubIndex = subIndex;
            StartPoint = NutPoint = PointM.FromVector(linePath.Start);
            BridgePoint = PointM.FromVector(linePath.End);
            Path = linePath;
        }

        public BaseStringConfiguration? GetConfiguration()
        {
            return Layout?.Configuration?.GetString(CourseIndex);
        }

        protected override RectangleM? CalculateBoundsCore()
        {
            return RectangleM.BoundingRectangle(NutPoint, BridgePoint);
        }

        public Measure? GetGauge()
        {
            if (Configuration is SingleStringConfiguration stringConfiguration)
                return stringConfiguration.Gauge;
            else if (Configuration is StringGroupConfiguration groupedStringConfiguration)
                return groupedStringConfiguration.GetGauge(SubIndex ?? 0);
            else
                return null;    
        }

        public void GeneratePath()
        {
            Path = new LinearPath(NutPoint.ToVector(), BridgePoint.ToVector());
        }

        protected override void FlipHorizontalCore()
        {
            Path.FlipHorizontal();
            StartPoint *= new VectorD(-1, 1);
            NutPoint *= new VectorD(-1, 1);
            BridgePoint *= new VectorD(-1, 1);
        }
    }
}
