using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Configuration.Builders;
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

        public virtual IReadOnlyList<InstrumentTuningPreset> GetTuningPresets()
        {
            return
            [
                //6 strings
                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_E}",
                [
                    TuningCourse.Single(NoteName.E, 2),
                    TuningCourse.Single(NoteName.A, 2),
                    TuningCourse.Single(NoteName.D, 3),
                    TuningCourse.Single(NoteName.G, 3),
                    TuningCourse.Single(NoteName.B, 3),
                    TuningCourse.Single(NoteName.E, 4),
                ]),

                new InstrumentTuningPreset(Texts.Tuning_HalfStepDown,
                [
                    TuningCourse.Single(NoteName.Eb, 2),
                    TuningCourse.Single(NoteName.Ab, 2),
                    TuningCourse.Single(NoteName.Db, 3),
                    TuningCourse.Single(NoteName.Gb, 3),
                    TuningCourse.Single(NoteName.Bb, 3),
                    TuningCourse.Single(NoteName.Eb, 4),
                ]),

                new InstrumentTuningPreset($"{Texts.Tuning_Drop} {Texts.NoteName_D}",
                [
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.A, 2),
                    TuningCourse.Single(NoteName.D, 3),
                    TuningCourse.Single(NoteName.G, 3),
                    TuningCourse.Single(NoteName.B, 3),
                    TuningCourse.Single(NoteName.E, 4),
                ]),

                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_D} / {Texts.Tuning_WholeStepDown}",
                [
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.G, 2),
                    TuningCourse.Single(NoteName.C, 3),
                    TuningCourse.Single(NoteName.F, 3),
                    TuningCourse.Single(NoteName.A, 3),
                    TuningCourse.Single(NoteName.D, 4),
                ]),

                new InstrumentTuningPreset($"{Texts.Tuning_Open} {Texts.NoteName_G}",
                [
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.G, 2),
                    TuningCourse.Single(NoteName.D, 3),
                    TuningCourse.Single(NoteName.G, 3),
                    TuningCourse.Single(NoteName.B, 3),
                    TuningCourse.Single(NoteName.D, 4),
                ]),

                new InstrumentTuningPreset($"DADGAD",
                [
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.A, 2),
                    TuningCourse.Single(NoteName.D, 3),
                    TuningCourse.Single(NoteName.G, 3),
                    TuningCourse.Single(NoteName.A, 3),
                    TuningCourse.Single(NoteName.D, 4),
                ]),

                //12 Strings
                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_E}",
                [
                    new TuningCourse([new NoteAndOctave(NoteName.E, 2),new NoteAndOctave(NoteName.E, 3)]),
                    new TuningCourse([new NoteAndOctave(NoteName.A, 2),new NoteAndOctave(NoteName.A, 3)]),
                    new TuningCourse([new NoteAndOctave(NoteName.D, 3),new NoteAndOctave(NoteName.D, 4)]),
                    new TuningCourse([new NoteAndOctave(NoteName.G, 3),new NoteAndOctave(NoteName.G, 4)]),
                    new TuningCourse([new NoteAndOctave(NoteName.B, 3),new NoteAndOctave(NoteName.B, 3)]),
                    new TuningCourse([new NoteAndOctave(NoteName.E, 4),new NoteAndOctave(NoteName.E, 4)]),
                ]),

                //7 strings
                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_B}",
                [
                    TuningCourse.Single(NoteName.B, 1),
                    TuningCourse.Single(NoteName.E, 2),
                    TuningCourse.Single(NoteName.A, 2),
                    TuningCourse.Single(NoteName.D, 3),
                    TuningCourse.Single(NoteName.G, 3),
                    TuningCourse.Single(NoteName.B, 3),
                    TuningCourse.Single(NoteName.E, 4),
                ]),

                new InstrumentTuningPreset($"{Texts.Tuning_Drop} {Texts.NoteName_A}",
                [
                    TuningCourse.Single(NoteName.A, 1),
                    TuningCourse.Single(NoteName.E, 2),
                    TuningCourse.Single(NoteName.A, 2),
                    TuningCourse.Single(NoteName.D, 3),
                    TuningCourse.Single(NoteName.G, 3),
                    TuningCourse.Single(NoteName.B, 3),
                    TuningCourse.Single(NoteName.E, 4),
                ]),

                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_A} / {Texts.Tuning_WholeStepDown}",
                [
                    TuningCourse.Single(NoteName.A, 1),
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.G, 2),
                    TuningCourse.Single(NoteName.C, 3),
                    TuningCourse.Single(NoteName.F, 3),
                    TuningCourse.Single(NoteName.A, 3),
                    TuningCourse.Single(NoteName.D, 4),
                ]),

                //8 strings
                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_Gb}",
                [
                    TuningCourse.Single(NoteName.Gb, 1),
                    TuningCourse.Single(NoteName.B, 1),
                    TuningCourse.Single(NoteName.E, 2),
                    TuningCourse.Single(NoteName.A, 2),
                    TuningCourse.Single(NoteName.D, 3),
                    TuningCourse.Single(NoteName.G, 3),
                    TuningCourse.Single(NoteName.B, 3),
                    TuningCourse.Single(NoteName.E, 4),
                ]),

                new InstrumentTuningPreset($"{Texts.Tuning_Standard} {Texts.NoteName_E} / {Texts.Tuning_WholeStepDown}",
                [
                    TuningCourse.Single(NoteName.E, 1),
                    TuningCourse.Single(NoteName.A, 1),
                    TuningCourse.Single(NoteName.D, 2),
                    TuningCourse.Single(NoteName.G, 2),
                    TuningCourse.Single(NoteName.C, 3),
                    TuningCourse.Single(NoteName.F, 3),
                    TuningCourse.Single(NoteName.A, 3),
                    TuningCourse.Single(NoteName.D, 4),
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
            //var config = new InstrumentLayoutConfiguration
            //{
            //    InstrumentType = InstrumentType,
            //    NumberOfStrings = 6,
            //    NumberOfFrets = frets,
            //    LeftHanded = false,
            //};

            //config.StringConfigurations.Add(new SingleStringConfiguration
            //{
            //    Tuning = new NoteAndOctave(NoteName.E, 2),
            //    Gauge = Measure.In(0.042),
            //    MaterialType = StringMaterialType.NickelWound
            //});
            //config.StringConfigurations.Add(new SingleStringConfiguration
            //{
            //    Tuning = new NoteAndOctave(NoteName.A, 2),
            //    Gauge = Measure.In(0.032),
            //    MaterialType = StringMaterialType.NickelWound
            //});
            //config.StringConfigurations.Add(new SingleStringConfiguration
            //{
            //    Tuning = new NoteAndOctave(NoteName.D, 3),
            //    Gauge = Measure.In(0.024),
            //    MaterialType = StringMaterialType.NickelWound
            //});
            //config.StringConfigurations.Add(new SingleStringConfiguration
            //{
            //    Tuning = new NoteAndOctave(NoteName.G, 3),
            //    Gauge = Measure.In(0.016),
            //    MaterialType = StringMaterialType.SteelPlain
            //});
            //config.StringConfigurations.Add(new SingleStringConfiguration
            //{
            //    Tuning = new NoteAndOctave(NoteName.B, 3),
            //    Gauge = Measure.In(0.012),
            //    MaterialType = StringMaterialType.SteelPlain
            //});
            //config.StringConfigurations.Add(new SingleStringConfiguration
            //{
            //    Tuning = new NoteAndOctave(NoteName.E, 4),
            //    Gauge = Measure.In(0.010),
            //    MaterialType = StringMaterialType.SteelPlain
            //});

            //config.NutSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            //config.NutSpacing.SpacingMode = StringSpacingMode.Proportional;
            //config.NutSpacing.StringDistances.Add(nutSpacing);

            //config.BridgeSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            //config.BridgeSpacing.SpacingMode = StringSpacingMode.CenterToCenter;
            //config.BridgeSpacing.StringDistances.Add(bridgeSpacing);

            //config.ScaleLength.CalculationMethod = ScaleLengthCalculationMethod.AlongFingerboard;
            //config.ScaleLength.Mode = ScaleLengthMode.Single;
            //config.ScaleLength.SingleScale = scaleLength;

            //config.Fingerboard.SetAllMargins(margin);
            //config.Fingerboard.CompensateMarginsForStrings = false;
            //config.Fingerboard.ExtensionAfterLastFret = Measuring.Measure.Mm(10);

            var config = new LayoutConfigurationBuilder()
                .WithInstrumentType(InstrumentType)
                .WithNumberOfFrets(frets)
                .AddSingleString(sb =>
                    sb
                    .WithTuning(NoteName.E, 2)
                    .WithGauge(Measure.In(0.042))
                    .WithMaterialType(StringMaterialType.NickelWound)
                )
                .AddSingleString(sb =>
                    sb
                    .WithTuning(NoteName.A, 2)
                    .WithGauge(Measure.In(0.032))
                    .WithMaterialType(StringMaterialType.NickelWound)
                )
                .AddSingleString(sb =>
                    sb
                    .WithTuning(NoteName.D, 3)
                    .WithGauge(Measure.In(0.024))
                    .WithMaterialType(StringMaterialType.NickelWound)
                )
                .AddSingleString(sb =>
                    sb
                    .WithTuning(NoteName.G, 3)
                    .WithGauge(Measure.In(0.016))
                    .WithMaterialType(StringMaterialType.SteelPlain)
                )
                .AddSingleString(sb =>
                    sb
                    .WithTuning(NoteName.B, 3)
                    .WithGauge(Measure.In(0.012))
                    .WithMaterialType(StringMaterialType.SteelPlain)
                )
                .AddSingleString(sb =>
                    sb
                    .WithTuning(NoteName.E, 4)
                    .WithGauge(Measure.In(0.010))
                    .WithMaterialType(StringMaterialType.SteelPlain)
                )
                .WithScaleLength(scaleLength)
                .WithMargins(margin, true)
                .WithNutSpacing(nutSpacing)
                .WithBridgeSpacing(bridgeSpacing)

                .Build();
            config.Fingerboard.ExtensionAfterLastFret = Measuring.Measure.Mm(10);
            return config;
        }

        public abstract IReadOnlyList<LayoutTemplate> GetLayoutTemplates();
    }
}
