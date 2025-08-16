using SiGen.Layouts.Elements;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
using SiGen.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Layouts
{
    public class StringedInstrumentLayout
    {
        public LayoutElementCollection Elements { get; }

        public IEnumerable<StringElement> Strings => Elements.OfType<StringElement>();

        public InstrumentLayoutConfiguration? Configuration { get; set; }

        public RectangleM? Bounds { get; set; }

        public int NumberOfStrings => Configuration?.NumberOfStrings ?? Strings.Count();

        public StringedInstrumentLayout()
        {
            Elements = new LayoutElementCollection(this);
        }

        public T AddElement<T>(T element) where T : LayoutElement
        {
            Elements.Add(element);
            return element;
        }

        #region Get Element Helpers

        public FingerboardSideElement GetFingerboardEdge(FingerboardSide side)
        {
            return Elements.OfType<FingerboardSideElement>().First(x => x.Side == side);
        }

        public StringMedianElement GetStringMedian(int medianIndex)
        {
            return Elements.OfType<StringMedianElement>().First(x => x.MedianIndex == medianIndex);
        }

        public StringElement GetStringElement(int stringIndex)
        {
            return Elements.OfType<StringElement>().First(x => x.StringIndex == stringIndex);
        }

        public StringElement GetStringElement(FingerboardSide side, int offset = 0)
        {
            int index = side == FingerboardSide.Bass ? (0 + offset) : ((Configuration?.NumberOfStrings ?? Strings.Count()) - 1 - offset);
            return Elements.OfType<StringElement>().First(x => x.StringIndex == index);
        }

        #endregion


        //public LinearPath GetStringMidLine(int str1, int str2)
        //{
        //    var string1 = GetStringElement(str1);
        //    var string2 = GetStringElement(str2);
        //    return new LinearPath(
        //        (string1.NutPoint.ToVector() + string2.NutPoint.ToVector()) / 2d,
        //        (string1.BridgePoint.ToVector() + string2.BridgePoint.ToVector()) / 2d
        //    );
        //}

        public void CalculateBounds()
        {
            var bounds = new RectangleM();
            foreach (var element in Elements)
            {
                if (element.Bounds != null)
                    bounds = RectangleM.Combine(bounds, element.Bounds);
            }
            Bounds = bounds;
        }
    }
}
