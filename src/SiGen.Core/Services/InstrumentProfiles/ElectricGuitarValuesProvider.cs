using SiGen.Data.Presets;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services.InstrumentProfiles
{
    public class ElectricGuitarValuesProvider : GenericGuitarValuesProvider
    {
        public override IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [
                new SpacingPreset("Narrow   (10 mm)", Measure.Mm(10)),
                new SpacingPreset("Standard (10.4 mm)", Measure.Mm(10.4)),
                new SpacingPreset("Wide     (10.7 mm)", Measure.Mm(10.7)),
            ];

        }

        public override IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [
                new SpacingPreset("Fender   (7 mm)", Measure.Mm(7)),
                new SpacingPreset("Gibson   (7.1 mm)", Measure.Mm(7.1)),
                new SpacingPreset("PRS      (6.98 mm)", Measure.Mm(6.98)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset("Fender   (3.4 mm)", Measure.Mm(3.4)),
                new SpacingPreset("Gibson   (3.75 mm)", Measure.Mm(3.75)),
                new SpacingPreset("PRS      (3.97 mm)", Measure.Mm(3.97)),
            ];
        }

        public override IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets()
        {
            return
            [
                new ScaleLengthPreset("Gibson", Measure.In(24.75)),
                new ScaleLengthPreset("PRS", Measure.In(25.0)),
                new ScaleLengthPreset("Fender/Ibanez", Measure.In(25.5)),
                new ScaleLengthPreset("Baritone", Measure.In(27.0)),
            ];
        }
    }
}
