using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Converters;
using SiGen.Data.Presets;
using SiGen.Lang;
using SiGen.Layouts.Configuration;
using SiGen.Services;
using SiGen.Services.InstrumentProfiles;
using SiGen.UI.Utils;
using SiGen.ViewModels.EditorPanels;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SiGen.UI.EditorPanels;

public partial class ScaleLengthEditorPanel : UserControl
{
    public ScaleLengthEditorPanel()
    {
        InitializeComponent();
        FretAlignCbo.PointerPressed += FretAlignCbo_PointerPressed;
    }

   

    private void FretAlignCbo_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        FretAlignCbo.IsDropDownOpen = true;
        var pseudoClassProp = typeof(StyledElement).GetProperty("PseudoClasses", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (pseudoClassProp != null)
        {
            var pseudoClasses = (IPseudoClasses)pseudoClassProp.GetValue(FretAlignCbo)!;
            pseudoClasses.Remove(":pressed");
        }
    }


    private ScaleLengthPanelViewModel? _attachedViewModel;

    protected ScaleLengthPanelViewModel? ViewModel => DataContext as ScaleLengthPanelViewModel;

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_attachedViewModel != null)
        {
            _attachedViewModel.InstrumentTypeChanged -= ViewModel_InstrumentTypeChanged;
            _attachedViewModel.NumberOfStringsChanged -= ViewModel_NumberOfStringsChanged;
            _attachedViewModel = null;
        }

        if (DataContext is ScaleLengthPanelViewModel viewModel)
        {
            viewModel.InstrumentTypeChanged += ViewModel_InstrumentTypeChanged;
            viewModel.NumberOfStringsChanged += ViewModel_NumberOfStringsChanged;   
            _attachedViewModel = viewModel;
        }

        BuildPresetFlyoutMenus();
    }


    private void ViewModel_InstrumentTypeChanged(object? sender, EventArgs e)
    {
        BuildPresetFlyoutMenus();
    }

    private void ViewModel_NumberOfStringsChanged(object? sender, EventArgs e)
    {
        if (ViewModel != null && IsLoaded)
        {
            var instrumentType = ViewModel.LayoutDocument.Configuration.InstrumentType;
            if (instrumentType == Data.Common.InstrumentType.ElectricGuitar || 
                instrumentType == Data.Common.InstrumentType.ElectricBass)
            {
                BuildMultiScalePresetMenu();
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        BuildPresetFlyoutMenus();
    }

    private void BuildPresetFlyoutMenus()
    {
        if (!IsLoaded || ViewModel == null) return;

        BuildSingleScalePresetMenu();
        BuildMultiScalePresetMenu();
    }

    private void BuildSingleScalePresetMenu()
    {
        SingleScaleLengthField.Info = null;

        var provider = ViewModel?.LayoutDocument?.InstrumentValuesProvider;
        if (provider == null) return;

        var scaleLengthPresets = provider.GetScaleLengthPresets();
        if (scaleLengthPresets.Count == 0) return;

        var menuBuilder = new MenuFlyoutBuilder()
            .AddHeader(Lang.Resources.ScaleLengthEditorPanel_SingleScalePresetHeader)
            .AddSubText(Lang.Resources.PresetFlyout_ClickPresetToApply);

        foreach (var preset in scaleLengthPresets)
        {
            var length = preset.ScaleLength;
            menuBuilder.AddOption(CreateScaleLengthPresetHeader(preset), () =>
            {
				if (ViewModel != null) ViewModel.SingleScale = length;
            });
        }

        SingleScaleLengthField.Info = menuBuilder.Build();
    }

    private static Grid CreateScaleLengthPresetHeader(ScaleLengthPreset preset)
    {
        var labelGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        labelGrid.Children.Add(new TextBlock { Text = preset.Name });
        Grid.SetColumn(labelGrid.Children[0], 0);
        labelGrid.Children.Add(new TextBlock { Text = $"({preset.ScaleLength.ToStringFormatted()})" });
        Grid.SetColumn(labelGrid.Children[1], 1);
        return labelGrid;
    }

    private void BuildMultiScalePresetMenu()
    {
        BassScaleLengthField.Info = null;

        if (ViewModel == null) return;

        var instrumentType = ViewModel.LayoutDocument.Configuration.InstrumentType;


        if (instrumentType == Data.Common.InstrumentType.ElectricGuitar || instrumentType == Data.Common.InstrumentType.ElectricBass)
        {
            var menuBuilder = new MenuFlyoutBuilder()
                .AddHeader(Lang.Resources.ScaleLengthEditorPanel_SingleScalePresetHeader)
                .AddSubText(Lang.Resources.PresetFlyout_ClickPresetToApply);

            List<Tuple<Measuring.Measure, Measuring.Measure>> presets = [];

            if (instrumentType == Data.Common.InstrumentType.ElectricGuitar)
            {
                presets.Add(new Tuple<Measuring.Measure, Measuring.Measure>(Measuring.Measure.In(25.5), Measuring.Measure.In(24.75)));
                presets.Add(new Tuple<Measuring.Measure, Measuring.Measure>(Measuring.Measure.In(26.0), Measuring.Measure.In(24)));
                presets.Add(new Tuple<Measuring.Measure, Measuring.Measure>(Measuring.Measure.In(27.0), Measuring.Measure.In(25.5)));
                presets.Add(new Tuple<Measuring.Measure, Measuring.Measure>(Measuring.Measure.In(28), Measuring.Measure.In(25.5)));
            }
            else if (instrumentType == Data.Common.InstrumentType.ElectricBass)
            {
                presets.Add(new Tuple<Measuring.Measure, Measuring.Measure>(Measuring.Measure.In(35.0), Measuring.Measure.In(33.0)));

                if (ViewModel.NumberOfStrings == 4)
                    presets.Add(new Tuple<Measuring.Measure, Measuring.Measure>(Measuring.Measure.In(36.25), Measuring.Measure.In(34.0)));
                else if (ViewModel.NumberOfStrings == 5)
                    presets.Add(new Tuple<Measuring.Measure, Measuring.Measure>(Measuring.Measure.In(37.0), Measuring.Measure.In(34.0)));
                else if (ViewModel.NumberOfStrings == 6)
                    presets.Add(new Tuple<Measuring.Measure, Measuring.Measure>(Measuring.Measure.In(37.0), Measuring.Measure.In(33.25)));
            }

            foreach (var preset in presets)
            {
                menuBuilder.AddOption($"{preset.Item1.ToStringFormatted()} → {preset.Item2.ToStringFormatted()}", () =>
                {
					if (ViewModel != null) 
					{
						ViewModel.BassScale = preset.Item1;
						ViewModel.TrebleScale = preset.Item2;
					}
                });
            }

            BassScaleLengthField.Info = menuBuilder.Build();
        }


    }

    private void ScaleLengthModeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ScaleLengthModeComboBox?.SelectedItem is ScaleLengthMode mode)
        {
            string key = $"Editor.ScaleLengthMode.{mode}.Tooltip";
            ToolTip.SetTip(ScaleLengthModeComboBox, Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture));
        }
    }
}