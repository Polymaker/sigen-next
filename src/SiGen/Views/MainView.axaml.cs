using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Layouts.Builders;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Physics;
using SiGen.Serialization;
using SiGen.ViewModels;
using System.Text.Json;

namespace SiGen.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var services = (App.Current as App)?.Services;

        var config = CreateMultiScaleConfig2();

        if (services != null)
        {
            var documentContext = services.GetRequiredService<LayoutDocumentViewModel>();
            InfoPanel.DataContext = documentContext.GetPanelViewModel< ViewModels.EditorPanels.InstrumentInfoPanelViewModel>();
            ScalePanel.DataContext = documentContext.GetPanelViewModel<ViewModels.EditorPanels.ScaleLengthPanelViewModel>();

            documentContext.SetDocument(config, null);
            SILayoutViewer.Layout = documentContext.CurrentLayout;
            documentContext.LayoutChanged += (s, args) =>
            {
                SILayoutViewer.Layout = documentContext.CurrentLayout;
            };
        }
        else
        {
            var result = LayoutBuilder.Build(config);
            SILayoutViewer.Layout = result.Layout;
            var infoVM = new ViewModels.EditorPanels.InstrumentInfoPanelViewModel();
            infoVM.LoadConfiguration(config);
            InfoPanel.DataContext = infoVM;

            var scaleVM = new ViewModels.EditorPanels.ScaleLengthPanelViewModel();
            scaleVM.LoadConfiguration(config);
            ScalePanel.DataContext = scaleVM;

            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.Converters.Add(new MeasureConverter());
            options.Converters.Add(new BaseStringConfigurationConverter());
            //string json = JsonSerializer.Serialize(config, options);
            //var config2 = JsonSerializer.Deserialize<InstrumentLayoutConfiguration>(json, options);
        }
            
       
    }

    private InstrumentLayoutConfiguration CreateSingleScaleConfig()
    {
        var layoutConfig = new InstrumentLayoutConfiguration();
        layoutConfig.NumberOfStrings = 6;
        ConfigureStrings(layoutConfig);

        layoutConfig.NutSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
        layoutConfig.NutSpacing.SpacingMode = StringSpacingMode.Proportional;
        layoutConfig.NutSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(7.3m));

        layoutConfig.BridgeSpacing.CenterAlignment = LayoutCenterAlignment.OuterStrings;
        layoutConfig.BridgeSpacing.SpacingMode = StringSpacingMode.CenterToCenter;
        layoutConfig.BridgeSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(10.5m));

        layoutConfig.ScaleLength.CalculationMethod = ScaleLengthCalculationMethod.AlongFingerboard;
        layoutConfig.ScaleLength.Mode = ScaleLengthMode.Single;
        layoutConfig.ScaleLength.SingleScale = SiGen.Measuring.Measure.In(25.5m);

        layoutConfig.Margin.SetAll(SiGen.Measuring.Measure.Mm(3.25m));

        layoutConfig.NumberOfFrets = 24;

        return layoutConfig;
    }

    private InstrumentLayoutConfiguration CreateMandolinLayout()
    {
        var layoutConfig = new InstrumentLayoutConfiguration();
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
        layoutConfig.Margin.SetAll(SiGen.Measuring.Measure.Mm(2.5d));
        layoutConfig.Margin.CompensateForStrings = true;

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
            PitchInterval.FromNote(NoteName.G, 3), // G3
            PitchInterval.FromNote(NoteName.D, 4), // D4
            PitchInterval.FromNote(NoteName.A, 4), // A4
            PitchInterval.FromNote(NoteName.E, 5)  // E5
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

    private InstrumentLayoutConfiguration CreateMultiScaleConfig2()
    {
        var layoutConfig = new InstrumentLayoutConfiguration();
        layoutConfig.NumberOfStrings = 7;
        ConfigureStrings(layoutConfig);
        for (int i = layoutConfig.NumberOfStrings - 1; i > 1; i--)
        {
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[i]).Gauge = ((SingleStringConfiguration)layoutConfig.StringConfigurations[i - 2]).Gauge;
            ((SingleStringConfiguration)layoutConfig.StringConfigurations[i]).Tuning = ((SingleStringConfiguration)layoutConfig.StringConfigurations[i - 2]).Tuning;
        }
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[0]).Gauge = SiGen.Measuring.Measure.In(0.130);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[1]).Gauge = SiGen.Measuring.Measure.In(0.100);

        layoutConfig.NutSpacing.CenterAlignment = LayoutCenterAlignment.SymmetricFingerboard;

        //layoutConfig.NutSpacing.CenterAlignment = LayoutCenterAlignment.Manual;
        //layoutConfig.NutSpacing.AlignmentRatio = 1;
        layoutConfig.NutSpacing.SpacingMode = StringSpacingMode.CenterToCenter;
        layoutConfig.NutSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(9.2m));
        layoutConfig.BridgeSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(18m));
        layoutConfig.NutSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(8m));
        layoutConfig.BridgeSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(14m));
        for (int i = 0; i < 4; i++)
        {
            layoutConfig.NutSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(7.3m));
            layoutConfig.BridgeSpacing.StringDistances.Add(SiGen.Measuring.Measure.Mm(10.5m));
        }

        layoutConfig.BridgeSpacing.CenterAlignment = LayoutCenterAlignment.SymmetricFingerboard;
        layoutConfig.BridgeSpacing.SpacingMode = StringSpacingMode.CenterToCenter;


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
        layoutConfig.Margin.SetAll(SiGen.Measuring.Measure.Mm(3.25m));
        layoutConfig.Margin.CompensateForStrings = true;
        layoutConfig.NumberOfFrets = 24;
        //layoutConfig.StringConfigurations[2].Frets = new FretConfiguration
        //{
        //    StartingFret = -2
        //};
        //layoutConfig.StringConfigurations[3].Frets = new FretConfiguration
        //{
        //    StartingFret = -2
        //};
        layoutConfig.StringConfigurations[6].Frets = new FretConfiguration
        {
            //StartingFret = 1,
            //NumberOfFrets = 22,
        };
        return layoutConfig;
    }

    private void ConfigureStrings(InstrumentLayoutConfiguration layoutConfig)
    {
        layoutConfig.InitializeStringConfigs();
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[0]).Gauge = SiGen.Measuring.Measure.In(0.046);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[0]).Tuning = PitchInterval.FromNote(NoteName.E, 2);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[1]).Gauge = SiGen.Measuring.Measure.In(0.036);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[1]).Tuning = PitchInterval.FromNote(NoteName.A, 2);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[2]).Gauge = SiGen.Measuring.Measure.In(0.026);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[2]).Tuning = PitchInterval.FromNote(NoteName.D, 3);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[3]).Gauge = SiGen.Measuring.Measure.In(0.017);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[3]).Tuning = PitchInterval.FromNote(NoteName.G, 3);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[4]).Gauge = SiGen.Measuring.Measure.In(0.013);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[4]).Tuning = PitchInterval.FromNote(NoteName.B, 3);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[5]).Gauge = SiGen.Measuring.Measure.In(0.01);
        ((SingleStringConfiguration)layoutConfig.StringConfigurations[5]).Tuning = PitchInterval.FromNote(NoteName.E, 4);
    }
}