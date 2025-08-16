using SiGen.Measuring;

namespace SiGen.Data.Presets
{
    public class SpacingPreset
    {
        public string Name { get; set; }
        public Measure Spacing { get; set; }

        public SpacingPreset()
        {
            Name = string.Empty;
            Spacing = Measure.Empty;
        }

        public SpacingPreset(string name, Measure spacing)
        {
            Name = name;
            Spacing = spacing;
        }

        public SpacingPreset(Measure spacing)
        {
            Name = spacing.ToStringFormatted();
            Spacing = spacing;
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
