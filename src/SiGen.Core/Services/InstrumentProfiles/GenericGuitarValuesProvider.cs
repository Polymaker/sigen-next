using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Localization;
using SiGen.Measuring;
using SiGen.Physics;

namespace SiGen.Services.InstrumentProfiles
{
    public abstract class GenericGuitarValuesProvider : IInstrumentValuesProvider
    {
        public virtual int StandardStringCount => 6;

        public abstract InstrumentType InstrumentType { get; }

        public virtual IReadOnlyList<int> GetCommonStringsCount()
        {
            return [6];
        }

        public virtual IReadOnlyList<int> GetCommonFretsCount()
        {
            return [19, 20, 21, 22, 24];
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

        public virtual InstrumentLayoutConfiguration GetDefaultConfiguration()
        {
            var config = new InstrumentLayoutConfiguration
            {
                InstrumentType = InstrumentType,
                NumberOfStrings = 6,
                NumberOfFrets = 22,
                LeftHanded = false,
            };

            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.E, 2),
                Gauge = Measure.In(0.042),
            });
            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.A, 2),
                Gauge = Measure.In(0.032),
            });
            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.D, 3),
                Gauge = Measure.In(0.024),
            });
            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.G, 3),
                Gauge = Measure.In(0.016),
            });
            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.B, 3),
                Gauge = Measure.In(0.012),
            });
            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.E, 4),
                Gauge = Measure.In(0.010),
            });

            config.NutSpacing.CenterAlignment = LayoutCenterAlignment.Fingerboard;
            config.NutSpacing.SpacingMode = StringSpacingMode.Proportional;
            config.NutSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(7.3m));

            config.BridgeSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            config.BridgeSpacing.SpacingMode = StringSpacingMode.CenterToCenter;
            config.BridgeSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(10.5m));

            config.ScaleLength.CalculationMethod = ScaleLengthCalculationMethod.AlongFingerboard;
            config.ScaleLength.Mode = ScaleLengthMode.Single;
            config.ScaleLength.SingleScale = SiGen.Measuring.Measure.In(25.5m);

            config.Fingerboard.SetAllMargins(SiGen.Measuring.Measure.Mm(3.25m));
            config.Fingerboard.CompensateMarginsForStrings = true;
            config.Fingerboard.ExtensionAfterLastFret = Measuring.Measure.Mm(10);

            return config;
        }

        protected InstrumentLayoutConfiguration GetSingleScaleConfiguration(Measure scaleLength, Measure nutSpacing, Measure bridgeSpacing, Measure margin, int frets = 22)
        {
            var config = new InstrumentLayoutConfiguration
            {
                InstrumentType = InstrumentType,
                NumberOfStrings = 6,
                NumberOfFrets = frets,
                LeftHanded = false,
            };

            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.E, 2),
                Gauge = Measure.In(0.042),
            });
            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.A, 2),
                Gauge = Measure.In(0.032),
            });
            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.D, 3),
                Gauge = Measure.In(0.024),
            });
            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.G, 3),
                Gauge = Measure.In(0.016),
            });
            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.B, 3),
                Gauge = Measure.In(0.012),
            });
            config.StringConfigurations.Add(new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.E, 4),
                Gauge = Measure.In(0.010),
            });

            config.NutSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            config.NutSpacing.SpacingMode = StringSpacingMode.Proportional;
            config.NutSpacing.StringDistances.Add(nutSpacing);

            config.BridgeSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            config.BridgeSpacing.SpacingMode = StringSpacingMode.CenterToCenter;
            config.BridgeSpacing.StringDistances.Add(bridgeSpacing);

            config.ScaleLength.CalculationMethod = ScaleLengthCalculationMethod.AlongFingerboard;
            config.ScaleLength.Mode = ScaleLengthMode.Single;
            config.ScaleLength.SingleScale = scaleLength;

            config.Fingerboard.SetAllMargins(margin);
            config.Fingerboard.CompensateMarginsForStrings = false;
            config.Fingerboard.ExtensionAfterLastFret = Measuring.Measure.Mm(10);

            return config;
        }

        public abstract IReadOnlyList<LayoutTemplate> GetLayoutTemplates();
    }
}
