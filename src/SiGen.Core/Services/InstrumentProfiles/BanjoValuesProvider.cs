using SiGen.Data.Common;
using SiGen.Data.Presets;
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
                new ScaleLengthPreset("Short", Measure.In(25.5)),
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
    }
}
