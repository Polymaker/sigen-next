using SiGen.Data.Presets;
using SiGen.Localization;
using SiGen.Physics;

namespace SiGen.Services.InstrumentProfiles
{
    public abstract class GenericGuitarValuesProvider : IInstrumentValuesProvider
    {
        public virtual int StandardStringCount => 6;

        public virtual IReadOnlyList<int> GetCommonStringCounts()
        {
            return [6, 7, 8];
        }

        public abstract IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets();
        public abstract IReadOnlyList<SpacingPreset> GetMarginPresets();
        public abstract IReadOnlyList<SpacingPreset> GetNutSpacingPresets();
        public abstract IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets();

        public virtual IReadOnlyList<TuningPreset> GetTuningPresets()
        {
            return
            [
                //6 strings
                new TuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_E}",
                [
                    PitchInterval.FromNote(NoteName.E, 2),
                    PitchInterval.FromNote(NoteName.A, 2),
                    PitchInterval.FromNote(NoteName.D, 3),
                    PitchInterval.FromNote(NoteName.G, 3),
                    PitchInterval.FromNote(NoteName.B, 3),
                    PitchInterval.FromNote(NoteName.E, 4),
                ]),

                new TuningPreset($"{Texts.Tuning_Drop} {Texts.NoteName_D}",
                [
                    PitchInterval.FromNote(NoteName.D, 2),
                    PitchInterval.FromNote(NoteName.A, 2),
                    PitchInterval.FromNote(NoteName.D, 3),
                    PitchInterval.FromNote(NoteName.G, 3),
                    PitchInterval.FromNote(NoteName.B, 3),
                    PitchInterval.FromNote(NoteName.E, 4),
                ]),

                new TuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_D}",
                [
                    PitchInterval.FromNote(NoteName.D, 2),
                    PitchInterval.FromNote(NoteName.G, 2),
                    PitchInterval.FromNote(NoteName.C, 3),
                    PitchInterval.FromNote(NoteName.F, 3),
                    PitchInterval.FromNote(NoteName.A, 3),
                    PitchInterval.FromNote(NoteName.D, 4),
                ]),

                //7 strings
                new TuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_B}",
                [
                    PitchInterval.FromNote(NoteName.B, 1),
                    PitchInterval.FromNote(NoteName.E, 2),
                    PitchInterval.FromNote(NoteName.A, 2),
                    PitchInterval.FromNote(NoteName.D, 3),
                    PitchInterval.FromNote(NoteName.G, 3),
                    PitchInterval.FromNote(NoteName.B, 3),
                    PitchInterval.FromNote(NoteName.E, 4),
                ]),

                new TuningPreset($"{Texts.Tuning_Drop} {Texts.NoteName_A}",
                [
                    PitchInterval.FromNote(NoteName.A, 1),
                    PitchInterval.FromNote(NoteName.E, 2),
                    PitchInterval.FromNote(NoteName.A, 2),
                    PitchInterval.FromNote(NoteName.D, 3),
                    PitchInterval.FromNote(NoteName.G, 3),
                    PitchInterval.FromNote(NoteName.B, 3),
                    PitchInterval.FromNote(NoteName.E, 4),
                ]),

                new TuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_C}",
                [
                    PitchInterval.FromNote(NoteName.C, 2),
                    PitchInterval.FromNote(NoteName.F, 2),
                    PitchInterval.FromNote(NoteName.Bb, 2),
                    PitchInterval.FromNote(NoteName.Eb, 3),
                    PitchInterval.FromNote(NoteName.G, 3),
                    PitchInterval.FromNote(NoteName.C, 4),
                    PitchInterval.FromNote(NoteName.F, 4),
                ]),

                //8 strings
                new TuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_Gb}",
                [
                    PitchInterval.FromNote(NoteName.Gb, 1),
                    PitchInterval.FromNote(NoteName.B, 1),
                    PitchInterval.FromNote(NoteName.E, 3),
                    PitchInterval.FromNote(NoteName.A, 3),
                    PitchInterval.FromNote(NoteName.D, 4),
                    PitchInterval.FromNote(NoteName.G, 4),
                    PitchInterval.FromNote(NoteName.B, 4),
                    PitchInterval.FromNote(NoteName.E, 5),
                ]),


            ];
        }

    }
}
