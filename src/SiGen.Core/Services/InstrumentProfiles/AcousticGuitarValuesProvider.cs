using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Measuring;
using SiGen.Localization;

namespace SiGen.Services.InstrumentProfiles
{
    public class AcousticGuitarValuesProvider : GenericGuitarValuesProvider
    {
        public override InstrumentType InstrumentType => InstrumentType.AcousticGuitar;

        public override IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(6.8)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(7.1)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(7.4)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(10.3)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(10.7)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(11.2)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(3.2)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(3.5)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(3.8)),
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
