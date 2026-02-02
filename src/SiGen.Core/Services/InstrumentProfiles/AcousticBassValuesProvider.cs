using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;

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
                new ScaleLengthPreset("Standard", SiGen.Measuring.Measure.In(34)),
            ];
        }

        public override InstrumentLayoutConfiguration GetDefaultConfiguration()
        {
            throw new NotImplementedException();
        }
    }
}
