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
    public class BanjoValuesProvider : IInstrumentValuesProvider
    {
        public InstrumentType InstrumentType => InstrumentType.Banjo;

        public int StandardStringCount => 5;

        public IReadOnlyList<int> GetCommonStringsCount()
        {
            return [4,5,6];
        }

        public IReadOnlyList<int> GetCommonFretsCount()
        {
            return [19, 22];
        }

        public IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(6.5)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(7.0)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(7.5)),
            ];
        }

        public IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(9.5)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(10.0)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(10.5)),
            ];
        }

        public IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(3.0)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(3.5)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(4.0)),
            ];
        }

        public IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return [
                new ScaleLengthPreset("Tenor", Measure.In(23)),
                new ScaleLengthPreset(Texts.Preset_Standard, Measure.In(26.25)),
                
            ];
        }

        public IReadOnlyList<TuningPreset> GetTuningPresets()
        {
            return [
                //4 strings
                new TuningPreset("Tenor Standard (Jazz)", [
                    PitchInterval.FromNote(NoteName.C, 3),
                    PitchInterval.FromNote(NoteName.G, 3),
                    PitchInterval.FromNote(NoteName.D, 4),
                    PitchInterval.FromNote(NoteName.A, 4),
                ]),

                new TuningPreset("Tenor Irish", [
                    PitchInterval.FromNote(NoteName.G, 2),
                    PitchInterval.FromNote(NoteName.D, 3),
                    PitchInterval.FromNote(NoteName.A, 3),
                    PitchInterval.FromNote(NoteName.E, 4),
                ]),

                new TuningPreset($"Plectrum",
                [
                    PitchInterval.FromNote(NoteName.C, 2),
                    PitchInterval.FromNote(NoteName.G, 2),
                    PitchInterval.FromNote(NoteName.B, 2),
                    PitchInterval.FromNote(NoteName.D, 3),
                ]),

                //5 strings
                new TuningPreset($"Bluegrass / {Texts.Tuning_Standard}",
                [
                    PitchInterval.FromNote(NoteName.G, 4),
                    PitchInterval.FromNote(NoteName.D, 3),
                    PitchInterval.FromNote(NoteName.G, 3),
                    PitchInterval.FromNote(NoteName.B, 3),
                    PitchInterval.FromNote(NoteName.D, 4),
                ]),
            ];
        }

        public InstrumentLayoutConfiguration GetDefaultConfiguration()
        {
            return new LayoutConfigurationBuilder()
                .WithInstrumentType(InstrumentType.Banjo)
                .WithNumberOfFrets(22)
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.010)).WithTuning(new NoteAndOctave(NoteName.G, 4)).WithStartingFret(5))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.023)).WithTuning(new NoteAndOctave(NoteName.D, 3)))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.016)).WithTuning(new NoteAndOctave(NoteName.G, 3)))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.012)).WithTuning(new NoteAndOctave(NoteName.B, 3)))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.010)).WithTuning(new NoteAndOctave(NoteName.D, 4)))
                .WithScaleLength(Measure.In(26.25))
                .WithNutSpacing(Measure.Mm(6))
                .WithBridgeSpacing(Measure.Mm(10.5))
                .WithMargins(Measure.Mm(3))
                .Build();
        }

        public IReadOnlyList<LayoutTemplate> GetLayoutTemplates()
        {
            var fiveStringBanjo = GetDefaultConfiguration();

            var fourStringBanjo = new LayoutConfigurationBuilder()
                .WithInstrumentType(InstrumentType.Banjo)
                .WithNumberOfFrets(19)
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.036)).WithTuning(new NoteAndOctave(NoteName.C, 3)))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.026)).WithTuning(new NoteAndOctave(NoteName.G, 3)))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.016)).WithTuning(new NoteAndOctave(NoteName.D, 4)))
                .AddSingleString(cfg => cfg.WithGauge(Measure.In(0.010)).WithTuning(new NoteAndOctave(NoteName.A, 4)))
                .WithScaleLength(Measure.In(23))
                .WithNutSpacing(Measure.Mm(6.5))
                .WithBridgeSpacing(Measure.Mm(11))
                .WithMargins(Measure.Mm(3))
                .Build();

            return [
                new LayoutTemplate($"5 {Texts.Preset_Strings}", fiveStringBanjo),
                new LayoutTemplate($"4 {Texts.Preset_Strings} Tenor", fourStringBanjo)
            ];
        }
    }
}
