using SiGen.Data.Presets;
using SiGen.Measuring;

namespace SiGen.Services.InstrumentProfiles
{
    public class ElectricBassGuitarValuesProvider : GenericBassGuitarValuesProvider
    {

        public override IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [
                new SpacingPreset("Narrow   (7.5 mm)", Measure.Mm(7.5)),
                new SpacingPreset("Standard (8.0 mm)", Measure.Mm(8.0)),
                new SpacingPreset("Wide     (8.5 mm)", Measure.Mm(8.5)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [
                new SpacingPreset("Narrow   (16 mm)", Measure.Mm(16)),
                new SpacingPreset("Standard (18 mm)", Measure.Mm(18)),
                new SpacingPreset("Wide     (19 mm)", Measure.Mm(19)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset("Narrow   (3.5 mm)", Measure.Mm(3.5)),
                new SpacingPreset("Standard (4 mm)", Measure.Mm(4)),
                new SpacingPreset("Wide     (4.5 mm)", Measure.Mm(4.5)),
            ];
        }

        public override IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return
            [
                new ScaleLengthPreset("Short Scale (30\")", SiGen.Measuring.Measure.In(30)),
                new ScaleLengthPreset("Medium Scale (32\")", SiGen.Measuring.Measure.In(32)),
                new ScaleLengthPreset("Long Scale (34\")", SiGen.Measuring.Measure.In(34)),
                new ScaleLengthPreset("Extra Long Scale (36\")", SiGen.Measuring.Measure.In(36)),
            ];
        }
    }
}
