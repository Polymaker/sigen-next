using SiGen.Data.Presets;
using SiGen.Measuring;

namespace SiGen.Services.InstrumentProfiles
{
    public class AcousticGuitarValuesProvider : GenericGuitarValuesProvider
    {
        public override IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [
                new SpacingPreset("Narrow   (6.8 mm)", Measure.Mm(6.8)),
                new SpacingPreset("Standard (7.1 mm)", Measure.Mm(7.1)),
                new SpacingPreset("Wide     (7.4 mm)", Measure.Mm(7.4)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [
                new SpacingPreset("Narrow   (10.3 mm)", Measure.Mm(10.3)),
                new SpacingPreset("Standard (10.7 mm)", Measure.Mm(10.7)),
                new SpacingPreset("Wide     (11.2 mm)", Measure.Mm(11.2)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset("Narrow   (3.2 mm)", Measure.Mm(3.2)),
                new SpacingPreset("Standard (3.5 mm)", Measure.Mm(3.5)),
                new SpacingPreset("Wide     (3.8 mm)", Measure.Mm(3.8)),
            ];
        }

        public override IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return
            [
                new ScaleLengthPreset("Martin", Measure.In(25.4)),
                new ScaleLengthPreset("Gibson", Measure.In(24.75)),
                new ScaleLengthPreset("Taylor", Measure.In(25.5)),
                new ScaleLengthPreset("Baritone", Measure.In(27))
            ];
        }
    }
}
