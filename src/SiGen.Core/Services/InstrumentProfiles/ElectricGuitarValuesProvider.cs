using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Localization;
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
        public override InstrumentType InstrumentType => InstrumentType.ElectricGuitar;

        public override IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets()
        {
            return [
                new SpacingPreset(Texts.Preset_Narrow, Measure.Mm(10)),
                new SpacingPreset(Texts.Preset_Standard, Measure.Mm(10.4)),
                new SpacingPreset(Texts.Preset_Wide, Measure.Mm(10.7)),
            ];

        }

        public override IReadOnlyList<SpacingPreset> GetNutSpacingPresets()
        {
            return [
                new SpacingPreset("Fender", Measure.Mm(7)),
                new SpacingPreset("Gibson", Measure.Mm(7.1)),
                new SpacingPreset("PRS", Measure.Mm(6.98)),
            ];
        }

        public override IReadOnlyList<SpacingPreset> GetMarginPresets()
        {
            return [
                new SpacingPreset("Fender", Measure.Mm(3.4)),
                new SpacingPreset("Gibson", Measure.Mm(3.75)),
                new SpacingPreset("PRS", Measure.Mm(3.97)),
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
