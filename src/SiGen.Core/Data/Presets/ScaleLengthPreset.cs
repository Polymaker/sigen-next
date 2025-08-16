using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Data.Presets
{
    public class ScaleLengthPreset
    {
        public string Name { get; set; }
        public Measure ScaleLength { get; set; }

        public ScaleLengthPreset()
        {
            Name = string.Empty;
            ScaleLength = Measure.Empty;
        }

        public ScaleLengthPreset(string name, Measure scaleLength)
        {
            Name = name;
            ScaleLength = scaleLength;
        }
    }

    //public class MultiScalePreset
    //{
    //    public string Name { get; set; } = string.Empty;
    //    public Measure BassScale { get; set; }
    //    public Measure TrebleScale { get; set; }

    //    public MultiScalePreset()
    //    {
    //        Name = string.Empty;
    //        BassScale = Measure.Empty;
    //        TrebleScale = Measure.Empty;
    //    }

    //    public MultiScalePreset(string name, Measure bassScale, Measure trebleScale)
    //    {
    //        Name = name;
    //        BassScale = bassScale;
    //        TrebleScale = trebleScale;
    //    }
    //}
}
