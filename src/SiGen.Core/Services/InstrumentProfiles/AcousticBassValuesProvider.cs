using SiGen.Data.Presets;

namespace SiGen.Services.InstrumentProfiles
{
    public class AcousticBassValuesProvider : GenericBassGuitarValuesProvider
    {
        public override IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return
            [
                new ScaleLengthPreset("Standard (34\")", SiGen.Measuring.Measure.In(34)),
                new ScaleLengthPreset("Short Scale (30\")", SiGen.Measuring.Measure.In(30)),
            ];
        }
    }
}
