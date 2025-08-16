using SiGen.Data.Presets;
using SiGen.Localization;
using SiGen.Physics;

namespace SiGen.Services.InstrumentProfiles
{
    public abstract class GenericBassGuitarValuesProvider : IInstrumentValuesProvider
    {
        public virtual int StandardStringCount => 4;

        public virtual IReadOnlyList<int> GetCommonStringCounts()
        {
            return [4, 5, 6];
        }

        public abstract IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets();
        public abstract IReadOnlyList<SpacingPreset> GetNutSpacingPresets();
        public abstract IReadOnlyList<SpacingPreset> GetMarginPresets();
        public abstract IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets();

        public virtual IReadOnlyList<TuningPreset> GetTuningPresets()
        {
            return
            [
                //4 strings
                new TuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_E}",
                [
                    PitchInterval.FromNote(NoteName.E, 1),
                    PitchInterval.FromNote(NoteName.A, 1),
                    PitchInterval.FromNote(NoteName.D, 2),
                    PitchInterval.FromNote(NoteName.G, 2),
                ]),

                new TuningPreset($"{Texts.Tuning_Drop} {Texts.NoteName_D}",
                [
                    PitchInterval.FromNote(NoteName.D, 1),
                    PitchInterval.FromNote(NoteName.A, 1),
                    PitchInterval.FromNote(NoteName.D, 2),
                    PitchInterval.FromNote(NoteName.G, 2),
                ]),

                //5 strings
                new TuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_B}",
                [
                    PitchInterval.FromNote(NoteName.B, 0),
                    PitchInterval.FromNote(NoteName.E, 1),
                    PitchInterval.FromNote(NoteName.A, 1),
                    PitchInterval.FromNote(NoteName.D, 2),
                    PitchInterval.FromNote(NoteName.G, 2),
                ]),


                //6 strings
                new TuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_B}",
                [
                    PitchInterval.FromNote(NoteName.B, 0),
                    PitchInterval.FromNote(NoteName.E, 1),
                    PitchInterval.FromNote(NoteName.A, 1),
                    PitchInterval.FromNote(NoteName.D, 2),
                    PitchInterval.FromNote(NoteName.G, 2),
                    PitchInterval.FromNote(NoteName.C, 3),
                ]),

            ];
        }

        
    }
}
