using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Localization;
using SiGen.Physics;

namespace SiGen.Services.InstrumentProfiles
{
    public abstract class GenericBassGuitarValuesProvider : IInstrumentValuesProvider
    {
        public virtual int StandardStringCount => 4;

        public abstract InstrumentType InstrumentType { get; }

        public virtual IReadOnlyList<int> GetCommonStringsCount()
        {
            return [4, 5];
        }

        public virtual IReadOnlyList<int> GetCommonFretsCount()
        {
            return [20, 22, 24];
        }

        public abstract IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets();
        public abstract IReadOnlyList<SpacingPreset> GetNutSpacingPresets();
        public abstract IReadOnlyList<SpacingPreset> GetMarginPresets();
        public abstract IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets();

        public virtual IReadOnlyList<InstrumentTuningPreset> GetTuningPresets()
        {
            return
            [
                //4 strings
                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_E}",
                [
                    TuningCourse.Single(NoteName.E, 1),
                    TuningCourse.Single(NoteName.A, 1),
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.G, 2),
                ]),

                new InstrumentTuningPreset($"{Texts.Tuning_Drop} {Texts.NoteName_D}",
                [
                    TuningCourse.Single(NoteName.D, 1),
                    TuningCourse.Single(NoteName.A, 1),
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.G, 2),
                ]),

                //5 strings
                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_B}",
                [
                    TuningCourse.Single(NoteName.B, 0),
                    TuningCourse.Single(NoteName.E, 1),
                    TuningCourse.Single(NoteName.A, 1),
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.G, 2),
                ]),

                new InstrumentTuningPreset($"{Texts.Tuning_Drop} {Texts.NoteName_A}",
                [
                    TuningCourse.Single(NoteName.A, 0),
                    TuningCourse.Single(NoteName.E, 1),
                    TuningCourse.Single(NoteName.A, 1),
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.G, 2),
                ]),

                //6 strings
                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_B}",
                [
                    TuningCourse.Single(NoteName.B, 0),
                    TuningCourse.Single(NoteName.E, 1),
                    TuningCourse.Single(NoteName.A, 1),
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.G, 2),
                    TuningCourse.Single(NoteName.C, 3),
                ]),

                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_B} (Alt)",
                [
                    TuningCourse.Single(NoteName.B, 0),
                    TuningCourse.Single(NoteName.E, 1),
                    TuningCourse.Single(NoteName.A, 1),
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.Gb, 2),
                    TuningCourse.Single(NoteName.B, 2),
                ]),
            ];
        }

        public abstract InstrumentLayoutConfiguration GetDefaultConfiguration();

        public virtual IReadOnlyList<LayoutTemplate> GetLayoutTemplates()
        {
            return [];
        }
    }
}
