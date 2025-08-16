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
        public int StandardStringCount => 5;

        public IReadOnlyList<int> GetCommonStringCounts()
        {
            return [4,5,6];
        }

        public IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [
                new SpacingPreset("Narrow   (6.5 mm)", Measure.Mm(6.5)),
                new SpacingPreset("Standard (7.0 mm)", Measure.Mm(7.0)),
                new SpacingPreset("Wide     (7.5 mm)", Measure.Mm(7.5)),
            ];
        }

        public IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [
                new SpacingPreset("Narrow   (9.5 mm)", Measure.Mm(9.5)),
                new SpacingPreset("Standard (10.0 mm)", Measure.Mm(10.0)),
                new SpacingPreset("Wide     (10.5 mm)", Measure.Mm(10.5)),
            ];
        }

        public IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset("Narrow   (3.0 mm)", Measure.Mm(3.0)),
                new SpacingPreset("Standard (3.5 mm)", Measure.Mm(3.5)),
                new SpacingPreset("Wide     (4.0 mm)", Measure.Mm(4.0)),
            ];
        }

        public IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return [
                new ScaleLengthPreset("Standard", Measure.In(26.25)),
                new ScaleLengthPreset("Short", Measure.In(25.5)),
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
