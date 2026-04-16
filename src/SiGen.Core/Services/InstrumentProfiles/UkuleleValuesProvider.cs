using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Configuration.Builders;
using SiGen.Measuring;
using SiGen.Physics;

namespace SiGen.Services.InstrumentProfiles
{
    public class UkuleleValuesProvider : IInstrumentValuesProvider
    {
        public InstrumentType InstrumentType => InstrumentType.Ukulele;

        public int StandardStringCount => 4;

        public IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [];
        }

        public IReadOnlyList<int> GetCommonFretsCount()
        {
            return [12, 15, 17, 20];
        }

        public IReadOnlyList<int> GetCommonStringsCount()
        {
            return [4];
        }

        public InstrumentLayoutConfiguration GetDefaultConfiguration()
        {
            var builder = new LayoutConfigurationBuilder();
            builder.WithInstrumentType(InstrumentType.Ukulele)
                   .WithNumberOfFrets(17)
                   .WithScaleLength(Measure.In(15))
                   .WithNutBridgeMargins(Measure.Mm(3.5), Measure.Mm(5))
                   .AddSingleString(sb => sb.WithGauge(Measure.In(0.028)).WithTuning(NoteName.A, 4).WithMaterialType(StringMaterialType.NylonPlain))
                   .AddSingleString(sb => sb.WithGauge(Measure.In(0.032)).WithTuning(NoteName.E, 4).WithMaterialType(StringMaterialType.NylonPlain))
                   .AddSingleString(sb => sb.WithGauge(Measure.In(0.040)).WithTuning(NoteName.C, 4).WithMaterialType(StringMaterialType.NylonPlain))
                   .AddSingleString(sb => sb.WithGauge(Measure.In(0.028)).WithTuning(NoteName.G, 4).WithMaterialType(StringMaterialType.NylonPlain))
                   .WithNutSpacing(Measure.Mm(10))
                   .WithBridgeSpacing(Measure.Mm(15))
                   .WithExtension(Measure.Mm(20))
                   ;
            return builder.Build();
        }

        public IReadOnlyList<LayoutTemplate> GetLayoutTemplates()
        {
            return [
                new LayoutTemplate("Concert", GetDefaultConfiguration()),
                ];
        }

        public IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [];
        }

        public IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [];
        }

        public IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return [
                new ScaleLengthPreset("Soprano", Measure.In(13)),
                new ScaleLengthPreset("Concert", Measure.In(15)),
                new ScaleLengthPreset("Tenor", Measure.In(17)),
                new ScaleLengthPreset("Baritone", Measure.In(19)),
            ];
        }

        public IReadOnlyList<InstrumentTuningPreset> GetTuningPresets()
        {
            return [];
        }
    }
}
