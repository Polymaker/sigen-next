using SiGen.Data.Presets;
using SiGen.Measuring;

namespace SiGen.Services.InstrumentProfiles
{
    public class ClassicalGuitarValuesProvider : GenericGuitarValuesProvider
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
                new SpacingPreset("Narrow   (10.5 mm)", Measure.Mm(10.5)),
                new SpacingPreset("Standard (11.0 mm)", Measure.Mm(11.0)),
                new SpacingPreset("Wide     (11.5 mm)", Measure.Mm(11.5)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset("Narrow   (3.5 mm)", Measure.Mm(3.5)),
                new SpacingPreset("Standard (4.0 mm)", Measure.Mm(4.0)),
                new SpacingPreset("Wide     (4.5 mm)", Measure.Mm(4.5)),
            ];
        }

        public override IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return [
                new ScaleLengthPreset("Standard", Measure.In(25.6)),
                new ScaleLengthPreset("Short", Measure.In(24.8))
            ];
        }
    }
}
