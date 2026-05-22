using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Configuration.Builders;
using SiGen.Localization;
using SiGen.Measuring;
using SiGen.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services.InstrumentProfiles
{
    public class MandolinValuesProvider : IInstrumentValuesProvider
    {
        public InstrumentType InstrumentType => InstrumentType.Mandolin;

        public int StandardStringCount => 4; // 4 courses of 2 strings each
        
        public IReadOnlyList<int> GetCommonStringsCount()
        {
            return [4, 5];
        }

        public IReadOnlyList<int> GetCommonFretsCount()
        {
            return [20, 22, 23];
        }

        public IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset { Name = Texts.Preset_Standard, Spacing = Measure.Mm(2) },
            ];
        }

        public IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [
                new SpacingPreset { Name = Texts.Preset_Narrow, Spacing = Measure.In(0.75 / 4) },
                new SpacingPreset { Name = Texts.Preset_Standard, Spacing = Measure.Mm(7.5) },
                new SpacingPreset { Name = Texts.Preset_Wide, Spacing = Measure.In(0.95 / 4) },
            ];
        }

        public IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [];
        }

        public IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return [
                new ScaleLengthPreset { Name = Texts.Preset_Standard, ScaleLength = Measure.In(14) },
                new ScaleLengthPreset { Name = "Mandola", ScaleLength = Measure.In(16.5) },
                new ScaleLengthPreset { Name = "Octave", ScaleLength = Measure.In(21) },
            ];
        }

        public IReadOnlyList<InstrumentTuningPreset> GetTuningPresets()
        {
            return [
                //four strings
                new InstrumentTuningPreset(Texts.Tuning_Standard, [
                       TuningCourse.Single(NoteName.G, 3),
                       TuningCourse.Single(NoteName.D, 4),
                       TuningCourse.Single(NoteName.A, 4),
                       TuningCourse.Single(NoteName.E, 5)
                ]),

                //five strings
                new InstrumentTuningPreset(Texts.Tuning_Standard, [
                       TuningCourse.Single(NoteName.C, 3),
                       TuningCourse.Single(NoteName.G, 3),
                       TuningCourse.Single(NoteName.D, 4),
                       TuningCourse.Single(NoteName.A, 4),
                       TuningCourse.Single(NoteName.E, 5)
                ]),
            ];
        }

        public InstrumentLayoutConfiguration GetDefaultConfiguration()
        {
            var builder = new LayoutConfigurationBuilder();

            builder.WithMargins(Measure.Mm(2), true)
                   .WithInstrumentType(InstrumentType.Mandolin)
                   .WithNumberOfFrets(19)
                   .WithScaleLength(Measure.In(14))
                   .WithNutSpacing(Measure.Mm(7.5), centerAlignment: Layouts.Data.LayoutCenterAlignment.Fingerboard)
                   .WithBridgeSpacing(Measure.Mm(10.5))
                   .WithExtension(Measure.Mm(8))
                   .AddStringCourse(c =>
                        c.WithSpacing(Measure.Mm(2))
                        .AddString(s => s.WithGauge(Measure.In(0.036)).WithTuning(new NoteAndOctave(NoteName.G, 3)).WithMaterialType(StringMaterialType.BronzeWound))
                        .AddString(s => s.WithGauge(Measure.In(0.036)).WithTuning(new NoteAndOctave(NoteName.G, 3)).WithMaterialType(StringMaterialType.BronzeWound))
                   )
                   .AddStringCourse(c =>
                        c.WithSpacing(Measure.Mm(1.75))
                        .AddString(s => s.WithGauge(Measure.In(0.026)).WithTuning(new NoteAndOctave(NoteName.D, 4)).WithMaterialType(StringMaterialType.BronzeWound))
                        .AddString(s => s.WithGauge(Measure.In(0.026)).WithTuning(new NoteAndOctave(NoteName.D, 4)).WithMaterialType(StringMaterialType.BronzeWound))
                        .WithNumberOfFrets(20)
                   )
                   .AddStringCourse(c =>
                        c.WithSpacing(Measure.Mm(1.6))
                        .AddString(s => s.WithGauge(Measure.In(0.015)).WithTuning(new NoteAndOctave(NoteName.A, 4)).WithMaterialType(StringMaterialType.SteelPlain))
                        .AddString(s => s.WithGauge(Measure.In(0.015)).WithTuning(new NoteAndOctave(NoteName.A, 4)).WithMaterialType(StringMaterialType.SteelPlain))
                        .WithNumberOfFrets(22)
                   )
                   .AddStringCourse(c =>
                        c.WithSpacing(Measure.Mm(1.5))
                        .AddString(s => s.WithGauge(Measure.In(0.011)).WithTuning(new NoteAndOctave(NoteName.E, 4)).WithMaterialType(StringMaterialType.SteelPlain))
                        .AddString(s => s.WithGauge(Measure.In(0.011)).WithTuning(new NoteAndOctave(NoteName.E, 4)).WithMaterialType(StringMaterialType.SteelPlain))
                        .WithNumberOfFrets(22)
                   )
                   ;
            return builder.Build();
        }

        private InstrumentLayoutConfiguration GetFiveStringConfig()
        {
            var builder = new LayoutConfigurationBuilder();

            builder.WithMargins(Measure.Mm(2), true)
                   .WithInstrumentType(InstrumentType.Mandolin)
                   .WithNumberOfFrets(19)
                   .WithScaleLength(Measure.In(14))
                   .WithNutSpacing(Measure.Mm(7.5), centerAlignment: Layouts.Data.LayoutCenterAlignment.Fingerboard)
                   .WithBridgeSpacing(Measure.Mm(10.5))
                   .WithExtension(Measure.Mm(8))
                   .AddSingleString(c => 
                        c.WithGauge(Measure.In(0.049))
                        .WithTuning(new NoteAndOctave(NoteName.C, 3))
                        .WithMaterialType(StringMaterialType.BronzeWound)
                   )
                   .AddSingleString(c =>
                        c.WithGauge(Measure.In(0.036))
                        .WithTuning(new NoteAndOctave(NoteName.G, 3))
                        .WithMaterialType(StringMaterialType.BronzeWound)
                   )
                   .AddSingleString(c =>
                        c.WithGauge(Measure.In(0.026))
                        .WithTuning(new NoteAndOctave(NoteName.D, 4))
                        .WithMaterialType(StringMaterialType.BronzeWound)
                   )
                   .AddSingleString(c =>
                        c.WithGauge(Measure.In(0.015))
                        .WithTuning(new NoteAndOctave(NoteName.A, 4))
                        .WithMaterialType(StringMaterialType.SteelPlain)
                   )
                   .AddSingleString(c =>
                        c.WithGauge(Measure.In(0.011))
                        .WithTuning(new NoteAndOctave(NoteName.E, 5))
                        .WithMaterialType(StringMaterialType.SteelPlain)
                   )
                   ;
            return builder.Build();
        }

        public IReadOnlyList<LayoutTemplate> GetLayoutTemplates()
        {
            return [
                new LayoutTemplate("F5", GetDefaultConfiguration()),
                new LayoutTemplate($"5 {Texts.Preset_Strings}", GetFiveStringConfig()),
            ];
        }
    }
}
