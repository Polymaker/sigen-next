using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Utilities
{
    internal static class LayoutTemplates
    {
        public static InstrumentLayoutConfiguration CreateSingleScaleConfig()
        {
            var layoutConfig = new InstrumentLayoutConfiguration();
            layoutConfig.InstrumentType = Data.Common.InstrumentType.ElectricGuitar;
            layoutConfig.NumberOfStrings = 6;
            layoutConfig.InitializeStringConfigs();
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[0]).Gauge = SiGen.Measuring.Measure.In(0.046);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[0]).Tuning = new NoteAndOctave(NoteName.E, 2);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[1]).Gauge = SiGen.Measuring.Measure.In(0.036);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[1]).Tuning = new NoteAndOctave(NoteName.A, 2);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[2]).Gauge = SiGen.Measuring.Measure.In(0.026);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[2]).Tuning = new NoteAndOctave(NoteName.D, 3);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[3]).Gauge = SiGen.Measuring.Measure.In(0.017);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[3]).Tuning = new NoteAndOctave(NoteName.G, 3);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[4]).Gauge = SiGen.Measuring.Measure.In(0.013);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[4]).Tuning = new NoteAndOctave(NoteName.B, 3);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[5]).Gauge = SiGen.Measuring.Measure.In(0.01);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[5]).Tuning = new NoteAndOctave(NoteName.E, 4);

            layoutConfig.NutSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            layoutConfig.NutSpacing.SpacingMode = StringSpacingMode.Proportional;
            layoutConfig.NutSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(7.3m));

            layoutConfig.BridgeSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            layoutConfig.BridgeSpacing.SpacingMode = StringSpacingMode.CenterToCenter;
            layoutConfig.BridgeSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(10.5m));

            layoutConfig.ScaleLength.CalculationMethod = ScaleLengthCalculationMethod.AlongFingerboard;
            layoutConfig.ScaleLength.Mode = ScaleLengthMode.Single;
            layoutConfig.ScaleLength.SingleScale = SiGen.Measuring.Measure.In(25.5m);

            layoutConfig.Fingerboard.SetAllMargins(SiGen.Measuring.Measure.Mm(3.25m));
            layoutConfig.Fingerboard.CompensateMarginsForStrings = true;
            layoutConfig.Fingerboard.ExtensionAfterLastFret = Measuring.Measure.Mm(10);
            layoutConfig.NumberOfFrets = 24;

            return layoutConfig;
        }

        public static InstrumentLayoutConfiguration CreateBassGuitarMultiscaleLayout()
        {
            var layoutConfig = new InstrumentLayoutConfiguration();
            layoutConfig.InstrumentType = Data.Common.InstrumentType.Custom;
            layoutConfig.NumberOfStrings = 7;
            layoutConfig.InitializeStringConfigs();
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[0]).Gauge = SiGen.Measuring.Measure.In(0.105);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[0]).Tuning = new NoteAndOctave(NoteName.E, 1);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[1]).Gauge = SiGen.Measuring.Measure.In(0.080);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[1]).Tuning = new NoteAndOctave(NoteName.A, 1);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[2]).Gauge = SiGen.Measuring.Measure.In(0.046);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[2]).Tuning = new NoteAndOctave(NoteName.E, 2);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[3]).Gauge = SiGen.Measuring.Measure.In(0.036);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[3]).Tuning = new NoteAndOctave(NoteName.A, 2);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[4]).Gauge = SiGen.Measuring.Measure.In(0.026);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[4]).Tuning = new NoteAndOctave(NoteName.D, 3);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[5]).Gauge = SiGen.Measuring.Measure.In(0.017);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[5]).Tuning = new NoteAndOctave(NoteName.G, 3);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[6]).Gauge = SiGen.Measuring.Measure.In(0.013);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[6]).Tuning = new NoteAndOctave(NoteName.B, 3);


            layoutConfig.NutSpacing.CenterAlignment = LayoutCenterAlignment.SymmetricFingerboard;
            layoutConfig.BridgeSpacing.CenterAlignment = LayoutCenterAlignment.SymmetricFingerboard;

            layoutConfig.NutSpacing.SpacingMode = StringSpacingMode.Manual;
            layoutConfig.BridgeSpacing.SpacingMode = StringSpacingMode.Manual;
            layoutConfig.NutSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(9.2m));
            layoutConfig.BridgeSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(18m));
            layoutConfig.NutSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(8m));
            layoutConfig.BridgeSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(14m));
            
            for (int i = 0; i < 4; i++)
            {
                layoutConfig.NutSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(7.3m));
                layoutConfig.BridgeSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(10.5m));
            }

            layoutConfig.ScaleLength.CalculationMethod = ScaleLengthCalculationMethod.AlongString;
            layoutConfig.ScaleLength.Mode = ScaleLengthMode.PerString;
            for (int i = 0; i < layoutConfig.NumberOfStrings; i++)
            {
                var baseScale = i < 2 ? SiGen.Measuring.Measure.In(34) : SiGen.Measuring.Measure.In(27);
                if (i == 1)
                    baseScale *= 0.98;
                else if (i >= 2)
                    baseScale *= 1d - ((i - 2) * 0.015d);
                layoutConfig.StringConfigurations[i].ScaleLength = baseScale;
                //layoutConfig.StringConfigurations[i].MultiScaleRatio = 1;
            }
            layoutConfig.ScaleLength.MultiScaleRatio = 0.5;
            layoutConfig.Fingerboard.SetAllMargins(SiGen.Measuring.Measure.Mm(3.25m));
            layoutConfig.Fingerboard.CompensateMarginsForStrings = true;
            layoutConfig.Fingerboard.ExtensionAfterLastFret = Measuring.Measure.Mm(12);
            layoutConfig.NumberOfFrets = 24;
            layoutConfig.StringConfigurations[0].Frets ??= new FretConfiguration();
            layoutConfig.StringConfigurations[0].Frets!.NumberOfFrets = 21;
            layoutConfig.StringConfigurations[1].Frets ??= new FretConfiguration();
            layoutConfig.StringConfigurations[1].Frets!.NumberOfFrets = 21;
            return layoutConfig;
        }

        public static InstrumentLayoutConfiguration CreateMandolinLayout()
        {
            var layoutConfig = new InstrumentLayoutConfiguration();
            layoutConfig.InstrumentType = Data.Common.InstrumentType.Mandolin;
            layoutConfig.NumberOfStrings = 4;
            layoutConfig.ScaleLength.Mode = ScaleLengthMode.Single;
            layoutConfig.ScaleLength.CalculationMethod = ScaleLengthCalculationMethod.AlongFingerboard;
            layoutConfig.ScaleLength.SingleScale = SiGen.Measuring.Measure.In(13.875);
            layoutConfig.NumberOfFrets = 18;

            layoutConfig.NutSpacing.SpacingMode = StringSpacingMode.Proportional;
            layoutConfig.NutSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            layoutConfig.NutSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(7.5));

            layoutConfig.BridgeSpacing.SpacingMode = StringSpacingMode.CenterToCenter;
            layoutConfig.BridgeSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            layoutConfig.BridgeSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(10.5));
            layoutConfig.Fingerboard.SetAllMargins(SiGen.Measuring.Measure.Mm(2.5d));
            layoutConfig.Fingerboard.CompensateMarginsForStrings = true;

            var gauges = new[] { 0.040, 0.026, 0.016, 0.011 };
            var spacings = new[]
            {
                SiGen.Measuring.Measure.Mm(2d), // G
                SiGen.Measuring.Measure.Mm(1.75d), // D
                SiGen.Measuring.Measure.Mm(1.6d), // A
                SiGen.Measuring.Measure.Mm(1.5d)  // E
            };
            var tunings = new[]
            {
                new NoteAndOctave(NoteName.G, 3), // G3
                new NoteAndOctave(NoteName.D, 4), // D4
                new NoteAndOctave(NoteName.A, 4), // A4
                new NoteAndOctave(NoteName.E, 5)  // E5
            };

            for (int i = 0; i < layoutConfig.NumberOfStrings; i++)
            {
                var stringConfig = new StringGroupConfiguration();
                stringConfig.Spacing = spacings[i];
                stringConfig.Strings.Add(new StringProperties
                {
                    Gauge = SiGen.Measuring.Measure.In(gauges[i]),
                    Tuning = tunings[i],
                });
                stringConfig.Strings.Add(new StringProperties
                {
                    Gauge = SiGen.Measuring.Measure.In(gauges[i]),
                    Tuning = tunings[i],
                });
                layoutConfig.StringConfigurations.Add(stringConfig);
            }
            layoutConfig.StringConfigurations[2].Frets ??= new FretConfiguration();
            layoutConfig.StringConfigurations[2].Frets!.NumberOfFrets = 19;
            layoutConfig.StringConfigurations[3].Frets ??= new FretConfiguration();
            layoutConfig.StringConfigurations[3].Frets!.NumberOfFrets = 22;
            layoutConfig.InitializeStringConfigs();

            return layoutConfig;
        }

        public static InstrumentLayoutConfiguration CreateBassLayout()
        {
            var provider = new Services.InstrumentValuesProviderFactory().CreateProvider(Data.Common.InstrumentType.ElectricBass)!;
            var layoutConfig = new InstrumentLayoutConfiguration();
            layoutConfig.InstrumentType = Data.Common.InstrumentType.ElectricBass;
            layoutConfig.NumberOfStrings = 4;
            layoutConfig.ScaleLength.Mode = ScaleLengthMode.Single;
            layoutConfig.InitializeStringConfigs();
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[0]).Gauge = SiGen.Measuring.Measure.In(0.105);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[0]).Tuning = new NoteAndOctave(NoteName.E, 1);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[1]).Gauge = SiGen.Measuring.Measure.In(0.080);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[1]).Tuning = new NoteAndOctave(NoteName.A, 1);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[2]).Gauge = SiGen.Measuring.Measure.In(0.060);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[2]).Tuning = new NoteAndOctave(NoteName.D, 2);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[3]).Gauge = SiGen.Measuring.Measure.In(0.040);
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[3]).Tuning = new NoteAndOctave(NoteName.G, 2);
            layoutConfig.ScaleLength.SingleScale = SiGen.Measuring.Measure.In(34);

            layoutConfig.NumberOfFrets = 22;
            layoutConfig.NutSpacing.SpacingMode = StringSpacingMode.Proportional;
            layoutConfig.NutSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            layoutConfig.NutSpacing.StringDistances.Add(provider.GetNutSpacingPresets().ElementAt(1).Spacing);

            layoutConfig.BridgeSpacing.SpacingMode = StringSpacingMode.CenterToCenter;
            layoutConfig.BridgeSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
            layoutConfig.BridgeSpacing.StringDistances.Add(provider.GetBridgeSpacingPresets().ElementAt(1).Spacing);

            layoutConfig.Fingerboard.SetAllMargins(SiGen.Measuring.Measure.Mm(3.25m));
            layoutConfig.Fingerboard.CompensateMarginsForStrings = true;

            return layoutConfig;
        }
    }
}
