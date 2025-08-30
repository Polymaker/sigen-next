using netDxf.Entities;
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

        public IReadOnlyList<TuningPreset> GetTuningPresets()
        {
            return [
                //four strings
                new TuningPreset(Texts.Tuning_Standard, [
                        PitchInterval.FromNote(NoteName.G, 3),
                        PitchInterval.FromNote(NoteName.D, 4),
                        PitchInterval.FromNote(NoteName.A, 4),
                        PitchInterval.FromNote(NoteName.E, 5)
                ]),

                //five strings
                new TuningPreset(Texts.Tuning_Standard, [
                        PitchInterval.FromNote(NoteName.C, 3),
                        PitchInterval.FromNote(NoteName.G, 3),
                        PitchInterval.FromNote(NoteName.D, 4),
                        PitchInterval.FromNote(NoteName.A, 4),
                        PitchInterval.FromNote(NoteName.E, 5)
                ]),
            ];
        }

        
    }
}
