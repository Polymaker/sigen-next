using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Localization;
using SiGen.Measuring;
using SiGen.Physics;
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

        public override IReadOnlyList<int> GetCommonFretsCount()
        {
            return [21, 22, 24];
        }

        public override IReadOnlyList<int> GetCommonStringsCount()
        {
            return [6, 7, 8];
        }

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

        public override InstrumentLayoutConfiguration GetDefaultConfiguration()
        {
            return GetSingleScaleConfiguration(Measure.In(25.5), Measure.Mm(7), Measure.Mm(9.67), Measure.Mm(3), 24);
        }

        public override IReadOnlyList<LayoutTemplate> GetLayoutTemplates()
        {
            var microtonalConfig = GetSingleScaleConfiguration(Measure.In(25.5), Measure.Mm(7), Measure.Mm(10.5), Measure.Mm(3.5), 40);
            microtonalConfig.Frets.ETSteps = 24;

            var microtonalConfig2 = GetSingleScaleConfiguration(Measure.In(25.5), Measure.Mm(7), Measure.Mm(10.5), Measure.Mm(3.5), 22);
            microtonalConfig2.Frets.Intervals = new List<double> { 450, 550 };


            var sevenStringsConfig = GetSingleScaleConfiguration(Measure.In(25.5), Measure.Mm(7), Measure.Mm(9.67), Measure.Mm(3), 24);
            sevenStringsConfig.NumberOfStrings = 7;
            sevenStringsConfig.StringConfigurations.Insert(0, new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.B, 1),
                Gauge = Measure.In(0.054),
                MaterialType = StringMaterialType.NickelWound
            });

            var eightStringsConfig = GetSingleScaleConfiguration(Measure.In(27), Measure.Mm(7), Measure.Mm(9.4), Measure.Mm(3), 24);
            eightStringsConfig.NumberOfStrings = 8;
            eightStringsConfig.Fingerboard.CompensateMarginsForStrings = true;
            eightStringsConfig.StringConfigurations.Insert(0, new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.Gb, 1),
                Gauge = Measure.In(0.060),
                MaterialType = StringMaterialType.NickelWound
            });
            eightStringsConfig.StringConfigurations.Insert(1, new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.B, 1),
                Gauge = Measure.In(0.054),
                MaterialType = StringMaterialType.NickelWound
            });

            var multiScaleConfig = GetSingleScaleConfiguration(Measure.In(25.5), Measure.Mm(7), Measure.Mm(9.67), Measure.Mm(3), 24);
            multiScaleConfig.NumberOfStrings = 7;
            multiScaleConfig.StringConfigurations.Insert(0, new SingleStringConfiguration
            {
                Tuning = new NoteAndOctave(NoteName.B, 1),
                Gauge = Measure.In(0.054),
                MaterialType = StringMaterialType.NickelWound
            });

            multiScaleConfig.ScaleLength.Mode = ScaleLengthMode.Multiscale;
            multiScaleConfig.ScaleLength.CalculationMethod = ScaleLengthCalculationMethod.AlongString;
            multiScaleConfig.ScaleLength.BassScale = Measure.In(27);
            multiScaleConfig.ScaleLength.TrebleScale = Measure.In(25.5);
            multiScaleConfig.Fingerboard.CompensateMarginsForStrings = true;
            return [
                new LayoutTemplate("Fender", 
                    GetSingleScaleConfiguration(Measure.In(25.5), Measure.Mm(7), Measure.Mm(10.5), Measure.Mm(3.5), 22)),
                new LayoutTemplate("Gibson",
                    GetSingleScaleConfiguration(Measure.In(24.75), Measure.Mm(7.1), Measure.Mm(10.4), Measure.Mm(4), 22)),
                new LayoutTemplate($"7 {Texts.Preset_Strings}", sevenStringsConfig),
                new LayoutTemplate($"8 {Texts.Preset_Strings}", eightStringsConfig),
                new LayoutTemplate($"{Texts.ScaleLengthMode_Multiscale} 7 {Texts.Preset_Strings}", multiScaleConfig),
                new LayoutTemplate("Microtonal 40 Frets", microtonalConfig),
                new LayoutTemplate("Microtonal Test", microtonalConfig2)
            ];
        }
    }
}
