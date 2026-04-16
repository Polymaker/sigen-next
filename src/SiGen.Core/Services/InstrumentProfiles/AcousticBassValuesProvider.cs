using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Configuration.Builders;
using SiGen.Measuring;

namespace SiGen.Services.InstrumentProfiles
{
    public class AcousticBassValuesProvider : GenericBassGuitarValuesProvider
    {
        public override InstrumentType InstrumentType => InstrumentType.AcousticBass;

        public override IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [];
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [];
        }

        public override IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [];
        }

        public override IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return
            [
                new ScaleLengthPreset("Short Scale", SiGen.Measuring.Measure.In(30)),
                new ScaleLengthPreset("Ibanez", SiGen.Measuring.Measure.In(32)),
                new ScaleLengthPreset("Standard", SiGen.Measuring.Measure.In(34)),
            ];
        }

        public override InstrumentLayoutConfiguration GetDefaultConfiguration()
        {
            var builder = new LayoutConfigurationBuilder();
            builder.WithInstrumentType(InstrumentType)
                .WithScaleLength(Measure.In(32))
                .WithNumberOfFrets(21)
                .WithNutSpacing(Measure.Mm(11.4))
                .WithBridgeSpacing(Measure.Mm(19))
                .WithMargins(Measure.Mm(4))
                .AddSingleString(sb => sb.WithGauge(Measure.In(0.100)).WithTuning(Physics.NoteName.E, 1).WithMaterialType(StringMaterialType.BronzeWound))
                .AddSingleString(sb => sb.WithGauge(Measure.In(0.080)).WithTuning(Physics.NoteName.A, 1).WithMaterialType(StringMaterialType.BronzeWound))
                .AddSingleString(sb => sb.WithGauge(Measure.In(0.065)).WithTuning(Physics.NoteName.D, 2).WithMaterialType(StringMaterialType.BronzeWound))
                .AddSingleString(sb => sb.WithGauge(Measure.In(0.045)).WithTuning(Physics.NoteName.G, 2).WithMaterialType(StringMaterialType.BronzeWound))
                .WithExtension(Measure.Mm(10))
                ;
            return builder.Build();
        }

        public override IReadOnlyList<LayoutTemplate> GetLayoutTemplates()
        {
            return [new LayoutTemplate("Default", GetDefaultConfiguration())];
        }
    }
}
