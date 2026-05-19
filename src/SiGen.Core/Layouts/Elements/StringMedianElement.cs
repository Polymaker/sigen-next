using SiGen.Layouts;
using SiGen.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts.Elements
{
    public class StringMedianElement : GuideLineElement
    {
        public int MedianIndex { get; }

        public int BassStringIndex => MedianIndex;
        public int TrebleStringIndex => MedianIndex + 1;

        public StringMedianElement(int medianIndex, LinearPath path) : base(GuideLineType.StringMedian, path: path)
        {
            MedianIndex = medianIndex;
        }
    }
}
