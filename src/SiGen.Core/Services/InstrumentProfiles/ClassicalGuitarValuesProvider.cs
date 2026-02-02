using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Localization;
using SiGen.Measuring;

namespace SiGen.Services.InstrumentProfiles
{
    public class ClassicalGuitarValuesProvider : GenericGuitarValuesProvider
    {
        public override InstrumentType InstrumentType => InstrumentType.ClassicalGuitar;

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
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(10.5)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(11.0)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(11.5)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(3.5)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(4.0)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(4.5)),
            ];
        }

        public override IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return [
                new ScaleLengthPreset("Standard", Measure.In(25.6)),
                new ScaleLengthPreset("Short", Measure.In(24.8))
            ];
        }

        public override InstrumentLayoutConfiguration GetDefaultConfiguration()
        {

            return GetSingleScaleConfiguration(Measure.In(25.6), Measure.Mm(8.0), Measure.Mm(11.0), Measure.Mm(4), 19);
        }

        public override IReadOnlyList<LayoutTemplate> GetLayoutTemplates()
        {
            return [
                new LayoutTemplate(Texts.Preset_Standard, GetSingleScaleConfiguration(Measure.In(25.6), Measure.Mm(8.0), Measure.Mm(11.0), Measure.Mm(4), 19)),
                new LayoutTemplate("Modern", GetSingleScaleConfiguration(Measure.In(25.6), Measure.Mm(9.0), Measure.Mm(12.0), Measure.Mm(4), 19)),
            ];
        }
    }
}
