using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Data.Presets;
using SiGen.Services;
using SiGen.UI.Utils;
using SiGen.ViewModels.EditorPanels;
using System;
using System.Diagnostics;

namespace SiGen.UI.EditorPanels;

public partial class StringSpacingEditorPanel : UserControl
{
    public StringSpacingEditorPanel()
    {
        InitializeComponent();
        ShowSliderButton.Click += ShowSliderButton_Click;

    }

    private StringSpacingPanelViewModel? ViewModel => DataContext as StringSpacingPanelViewModel;
    private StringSpacingPanelViewModel? _attachedViewModel;

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        RebuildPresetFlyouts();
        UpdateSpreadMinMax();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_attachedViewModel != null)
        {
            _attachedViewModel.InstrumentTypeChanged -= ViewModel_InstrumentTypeChanged;
            _attachedViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _attachedViewModel.NumberOfStringsChanged -= ViewModel_NumberOfStringsChanged;
            _attachedViewModel = null;
        }

        if (DataContext is StringSpacingPanelViewModel viewModel)
        {
            viewModel.InstrumentTypeChanged += ViewModel_InstrumentTypeChanged;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.NumberOfStringsChanged += ViewModel_NumberOfStringsChanged;
            _attachedViewModel = viewModel;
            if (IsLoaded) 
            {
                RebuildPresetFlyouts();
                UpdateSpreadMinMax();
            }

        }
    }

    private void ViewModel_NumberOfStringsChanged(object? sender, EventArgs e)
    {
        UpdateSpreadMinMax();
    }

    private void UpdateSpreadMinMax()
    {
        if (ViewModel == null) return;

        NutStringSpreadBox.MinimumValue = Measuring.Measure.Mm(2) * (ViewModel.NumberOfStrings - 1);
        NutStringSpreadBox.MaximumValue = Measuring.Measure.Mm(25) * (ViewModel.NumberOfStrings - 1);
        BridgeStringSpreadBox.MinimumValue = Measuring.Measure.Mm(2) * (ViewModel.NumberOfStrings - 1);
        BridgeStringSpreadBox.MaximumValue = Measuring.Measure.Mm(25) * (ViewModel.NumberOfStrings - 1);
    }

    private void ShowSliderButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            FlyoutBase.ShowAttachedFlyout(NutCenterAlignmentBox);
        }
    }

    private void ViewModel_InstrumentTypeChanged(object? sender, System.EventArgs e)
    {
        RebuildPresetFlyouts();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (!IsLoaded) return;

        if (e.PropertyName == nameof(ViewModel.NutSpacingMode))
        {
            NutSpacingInfoButton.IsVisible = NutSpacingInfoButton.Flyout != null && ViewModel!.NutSpacingMode != Layouts.Data.StringSpacingMode.Manual;
        }
        else if (e.PropertyName == nameof(ViewModel.BridgeSpacingMode))
        {
            BridgeSpacingInfoButton.IsVisible = BridgeSpacingInfoButton.Flyout != null && ViewModel!.BridgeSpacingMode != Layouts.Data.StringSpacingMode.Manual;
        }
        else if (e.PropertyName == nameof(ViewModel.BridgeSpacingMode))
        {
            BridgeSpacingInfoButton.IsVisible = BridgeSpacingInfoButton.Flyout != null && ViewModel!.BridgeSpacingMode != Layouts.Data.StringSpacingMode.Manual;
        }
    }

    #region Info loading

    private void RebuildPresetFlyouts()
    {
        var provider = ViewModel?.LayoutDocumentContext.InstrumentValuesProvider;

        if (provider == null)
        {
            BridgeSpacingInfoButton.Flyout = null;
            BridgeSpacingInfoButton.IsVisible = false;
            NutSpacingInfoButton.Flyout = null;
            NutSpacingInfoButton.IsVisible = false;
        }
        else
        {
            NutSpacingInfoButton.Flyout = CreateNutSpacingPresetMenu(provider);
            NutSpacingInfoButton.IsVisible = NutSpacingInfoButton.Flyout != null && ViewModel!.NutSpacingMode != Layouts.Data.StringSpacingMode.Manual;

            BridgeSpacingInfoButton.Flyout = CreateBridgeSpacingPresetMenu(provider);
            BridgeSpacingInfoButton.IsVisible = BridgeSpacingInfoButton.Flyout != null && ViewModel!.BridgeSpacingMode != Layouts.Data.StringSpacingMode.Manual;
        }
    }

    private FlyoutBase? CreateNutSpacingPresetMenu(IInstrumentValuesProvider provider)
    {
        var presets = provider.GetNutSpacingPresets();
        if (presets.Count == 0) return null;

        var menuBuilder = new MenuFlyoutBuilder()
            .AddHeader(Lang.Resources.StringSpacingEditorPanel_SpacingPresetHeader)
            .AddSubText(Lang.Resources.PresetFlyout_ClickPresetToApply);

        foreach (var preset in presets)
        {
            menuBuilder.AddOption(CreateSpacingPresetHeader(preset), () =>
            {
                if (ViewModel != null)
                    ViewModel.NutSpacing = preset.Spacing;
            });
        }

        return menuBuilder.Build();
    }

    private FlyoutBase? CreateBridgeSpacingPresetMenu(IInstrumentValuesProvider provider)
    {
        var presets = provider.GetBridgeSpacingPresets();
        if (presets.Count == 0) return null;

        var menuBuilder = new MenuFlyoutBuilder()
            .AddHeader(Lang.Resources.StringSpacingEditorPanel_SpacingPresetHeader)
            .AddSubText(Lang.Resources.PresetFlyout_ClickPresetToApply);

        foreach (var preset in presets)
        {
            menuBuilder.AddOption(CreateSpacingPresetHeader(preset), () =>
            {
                if (ViewModel != null)
                    ViewModel.BridgeSpacing = preset.Spacing;
            });
        }

        return menuBuilder.Build();
    }

    private static Grid CreateSpacingPresetHeader(SpacingPreset preset)
    {
        var labelGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        labelGrid.Children.Add(new TextBlock { Text = preset.Name });
        Grid.SetColumn(labelGrid.Children[0], 0);
        labelGrid.Children.Add(new TextBlock { Text = $"({preset.Spacing.ToStringFormatted()})" });
        Grid.SetColumn(labelGrid.Children[1], 1);
        return labelGrid;
    }

    #endregion
}