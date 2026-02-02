using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Configuration.Builders;
using SiGen.Localization;
using SiGen.Measuring;
using SiGen.Physics;

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

        public override InstrumentLayoutConfiguration GetDefaultConfiguration()
        {
            return GetSingleScaleConfiguration(Measure.In(25.4), Measure.Mm(7.1), Measure.Mm(10.7), Measure.Mm(3.5), 20);
        }

        public override IReadOnlyList<LayoutTemplate> GetLayoutTemplates()
        {
            return [
                new LayoutTemplate("Dreadnought/Concert",
                    GetSingleScaleConfiguration(Measure.In(25.4), Measure.Mm(7.0), Measure.Mm(11.0), Measure.Mm(4), 20)),
                new LayoutTemplate("Martin",
                    GetSingleScaleConfiguration(Measure.In(25.4), Measure.Mm(7.0), Measure.Mm(10.4), Measure.Mm(4.75), 20)),
                new LayoutTemplate($"12 {Texts.Preset_Strings}", GetTwelveStringsConfig())
            ];
        }

        static InstrumentLayoutConfiguration GetTwelveStringsConfig()
        {
            return new LayoutConfigurationBuilder()
                .WithInstrumentType(InstrumentType.AcousticGuitar)
                .WithNumberOfFrets(20)
                .AddStringCourse(c =>
                    c
                    .WithSpacing(Measure.Mm(2.4))
                    .AddString(new NoteAndOctave(Physics.NoteName.E, 2), Measure.In(0.047))
                    .AddString(new NoteAndOctave(NoteName.E, 3), Measure.In(0.027))
                )
                .AddStringCourse(c =>
                    c
                    .WithSpacing(Measure.Mm(2.3))
                    .AddString(new NoteAndOctave(Physics.NoteName.A, 2), Measure.In(0.039))
                    .AddString(new NoteAndOctave(NoteName.A, 3), Measure.In(0.018))
                )
                .AddStringCourse(c =>
                    c
                    .WithSpacing(Measure.Mm(2.2))
                    .AddString(new NoteAndOctave(Physics.NoteName.D, 3), Measure.In(0.030))
                    .AddString(new NoteAndOctave(NoteName.D, 4), Measure.In(0.012))
                )
                .AddStringCourse( c =>
                    c
                    .WithSpacing(Measure.Mm(2.0))
                    .AddString(new NoteAndOctave(Physics.NoteName.G, 3), Measure.In(0.023))
                    .AddString(new NoteAndOctave(NoteName.G, 4), Measure.In(0.008))
                )
                .AddStringCourse(c =>
                    c
                    .WithSpacing(Measure.Mm(1.8))
                    .AddString(new NoteAndOctave(Physics.NoteName.B, 3), Measure.In(0.014))
                    .AddString(new NoteAndOctave(NoteName.B, 3), Measure.In(0.014))
                )
                .AddStringCourse(c =>
                    c
                    .WithSpacing(Measure.Mm(1.6))
                    .AddString(new NoteAndOctave(Physics.NoteName.E, 4), Measure.In(0.010))
                    .AddString(new NoteAndOctave(NoteName.E, 4), Measure.In(0.010))
                )
                .WithScaleLength(Measure.In(25.5))
                .WithMargins(Measure.Mm(2), true)
                .WithNutSpacing(Measure.Mm(8))
                .WithBridgeSpacing(Measure.Mm(11))
                .Build();
}
    }
}
