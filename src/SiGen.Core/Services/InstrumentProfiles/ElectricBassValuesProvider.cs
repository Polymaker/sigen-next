using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Configuration.Builders;
using SiGen.Layouts.Data;
using SiGen.Localization;
using SiGen.Measuring;
using SiGen.Physics;

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

        public override InstrumentLayoutConfiguration GetDefaultConfiguration()
        {
            return GetBaseLayoutBuilder().Build();
        }

        private LayoutConfigurationBuilder GetBaseLayoutBuilder()
        {
            return new LayoutConfigurationBuilder()
                .WithInstrumentType(InstrumentType.ElectricBass)
                .WithNumberOfFrets(21)
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.105)).WithTuning(new NoteAndOctave(NoteName.E, 1)).WithMaterialType(StringMaterialType.NickelWound))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.085)).WithTuning(new NoteAndOctave(NoteName.A, 1)).WithMaterialType(StringMaterialType.NickelWound))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.065)).WithTuning(new NoteAndOctave(NoteName.D, 2)).WithMaterialType(StringMaterialType.NickelWound))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.045)).WithTuning(new NoteAndOctave(NoteName.G, 2)).WithMaterialType(StringMaterialType.NickelWound))
                .WithScaleLength(Measure.In(34))
                .WithNutSpacing(Measure.Mm(8))
                .WithBridgeSpacing(Measure.Mm(18))
                .WithMargins(Measure.Mm(4))
                .WithExtension(Measure.Mm(15));
        } 

        public override IReadOnlyList<LayoutTemplate> GetLayoutTemplates()
        {
            var fiveStringConfig = GetBaseLayoutBuilder()
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.130)).WithTuning(new NoteAndOctave(NoteName.B, 0)).WithMaterialType(StringMaterialType.NickelWound), FingerboardSide.Bass)
                .Build();

            var sixStringConfig = GetBaseLayoutBuilder()
                .WithNutSpacing(Measure.Mm(9))
                .WithBridgeSpacing(Measure.Mm(19))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.130)).WithTuning(new NoteAndOctave(NoteName.B, 0)).WithMaterialType(StringMaterialType.NickelWound), FingerboardSide.Bass)
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.032)).WithTuning(new NoteAndOctave(NoteName.C, 3)).WithMaterialType(StringMaterialType.NickelWound), FingerboardSide.Treble)
                .Build();
            return [
                new LayoutTemplate(Texts.Preset_Standard, GetDefaultConfiguration()),
                new LayoutTemplate($"5 {Texts.Preset_Strings}", fiveStringConfig),
                new LayoutTemplate($"6 {Texts.Preset_Strings}", sixStringConfig),
            ];
        }
    }
}
