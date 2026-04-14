using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Configuration.Builders;
using SiGen.Localization;
using SiGen.Measuring;
using SiGen.Physics;

namespace SiGen.Services.InstrumentProfiles
{
    public class AcousticGuitarValuesProvider : GenericGuitarValuesProvider
    {
        public override InstrumentType InstrumentType => InstrumentType.AcousticGuitar;

        public override IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(6.8)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(7.1)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(7.4)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(10.3)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(10.7)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(11.2)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(3.2)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(3.5)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(3.8)),
            ];
        }

        public override IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return
            [
                new ScaleLengthPreset("Martin", Measure.In(25.4)),
                new ScaleLengthPreset("Gibson", Measure.In(24.75)),
                new ScaleLengthPreset("Taylor", Measure.In(25.5)),
                new ScaleLengthPreset("Baritone", Measure.In(27))
            ];
        }

        public override InstrumentLayoutConfiguration GetDefaultConfiguration()
        {
            return GetSingleScaleConfiguration(Measure.In(25.4), Measure.Mm(7.1), Measure.Mm(10.7), Measure.Mm(3.5), 20);
        }

        public override IReadOnlyList<LayoutTemplate> GetLayoutTemplates()
        {

            return [
                new LayoutTemplate("Dreadnought/Concert",
                    FixStringMaterials(GetSingleScaleConfiguration(Measure.In(25.4), Measure.Mm(7.0), Measure.Mm(11.0), Measure.Mm(4), 20))),
                new LayoutTemplate("Martin",
                    FixStringMaterials(GetSingleScaleConfiguration(Measure.In(25.4), Measure.Mm(7.0), Measure.Mm(10.4), Measure.Mm(4.75), 20))),
                new LayoutTemplate($"12 {Texts.Preset_Strings}", FixStringMaterials(GetTwelveStringsConfig()))
            ];
        }

        private static InstrumentLayoutConfiguration FixStringMaterials(InstrumentLayoutConfiguration configuration)
        {
            var stringConfigs = configuration.StringConfigurations.OfType<SingleStringConfiguration>().ToArray();
            if (stringConfigs.Length == 6)
            {
                stringConfigs[0].Gauge = Measure.In(0.056);
                stringConfigs[1].Gauge = Measure.In(0.045);
                stringConfigs[2].Gauge = Measure.In(0.035);
                stringConfigs[3].Gauge = Measure.In(0.026);
                stringConfigs[4].Gauge = Measure.In(0.017);
                stringConfigs[5].Gauge = Measure.In(0.013);

                foreach (var stringProp in configuration.EnumerateStringProperties())
                {
                    stringProp.Data.Material ??= new StringMaterialConfiguration();
                    if (stringProp.Index.CourseIndex < 4)
                        stringProp.Data.Material.MaterialType = StringMaterialType.BronzeWound;
                    else
                        stringProp.Data.Material.MaterialType = StringMaterialType.SteelPlain;

                }
            }

            

            return configuration;
        }

        static InstrumentLayoutConfiguration GetTwelveStringsConfig()
        {
            return new LayoutConfigurationBuilder()
                .WithInstrumentType(InstrumentType.AcousticGuitar)
                .WithNumberOfFrets(20)
                .AddStringCourse(c =>
                    c
                    .WithSpacing(Measure.Mm(2.4))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.E, 2).WithGauge(Measure.In(0.052)).WithMaterialType(StringMaterialType.BronzeWound))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.E, 3).WithGauge(Measure.In(0.030)).WithMaterialType(StringMaterialType.BronzeWound))
                )
                .AddStringCourse(c =>
                    c
                    .WithSpacing(Measure.Mm(2.3))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.A, 2).WithGauge(Measure.In(0.042)).WithMaterialType(StringMaterialType.BronzeWound))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.A, 3).WithGauge(Measure.In(0.020)).WithMaterialType(StringMaterialType.SteelPlain))
                )
                .AddStringCourse(c =>
                    c
                    .WithSpacing(Measure.Mm(2.2))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.D, 3).WithGauge(Measure.In(0.032)).WithMaterialType(StringMaterialType.BronzeWound))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.D, 4).WithGauge(Measure.In(0.014)).WithMaterialType(StringMaterialType.SteelPlain))
                )
                .AddStringCourse( c =>
                    c
                    .WithSpacing(Measure.Mm(2.0))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.G, 3).WithGauge(Measure.In(0.025)).WithMaterialType(StringMaterialType.BronzeWound))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.G, 4).WithGauge(Measure.In(0.010)).WithMaterialType(StringMaterialType.SteelPlain))
                )
                .AddStringCourse(c =>
                    c
                    .WithSpacing(Measure.Mm(1.8))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.B, 3).WithGauge(Measure.In(0.016)).WithMaterialType(StringMaterialType.SteelPlain))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.B, 3).WithGauge(Measure.In(0.016)).WithMaterialType(StringMaterialType.SteelPlain))
                )
                .AddStringCourse(c =>
                    c
                    .WithSpacing(Measure.Mm(1.6))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.E, 4).WithGauge(Measure.In(0.012)).WithMaterialType(StringMaterialType.SteelPlain))
                    .AddString(sb => sb.WithTuning(Physics.NoteName.E, 4).WithGauge(Measure.In(0.012)).WithMaterialType(StringMaterialType.SteelPlain))
                )
                .WithScaleLength(Measure.In(25.5))
                .WithMargins(Measure.Mm(2), true)
                .WithNutSpacing(Measure.Mm(8))
                .WithBridgeSpacing(Measure.Mm(11))
                .WithExtension(Measure.Mm(10))
                .Build();
        }
    }
}
