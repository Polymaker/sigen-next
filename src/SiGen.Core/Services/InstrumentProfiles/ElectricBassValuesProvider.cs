using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Measuring;
using SiGen.Localization;

namespace SiGen.Services.InstrumentProfiles
{
    public class ElectricBassValuesProvider : GenericBassGuitarValuesProvider
    {
        public override InstrumentType InstrumentType => InstrumentType.ElectricBass;

        public override IReadOnlyList<int> GetCommonStringsCount()
        {
            return [4, 5, 6];
        }

        public override IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(7.5)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(8.0)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(8.5)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(16)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(18)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(19)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(3.5)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(4)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(4.5)),
            ];
        }

        public override IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return
            [
                new ScaleLengthPreset("Short Scale", SiGen.Measuring.Measure.In(30)),
                new ScaleLengthPreset("Medium Scale", SiGen.Measuring.Measure.In(32)),
                new ScaleLengthPreset("Long Scale", SiGen.Measuring.Measure.In(34)),
                new ScaleLengthPreset("Extra Long Scale", SiGen.Measuring.Measure.In(36)),
            ];
        }
    }
}
