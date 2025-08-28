using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Data.Presets;
using SiGen.Services;
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
        //var nutSliderFlyout = FlyoutBase.GetAttachedFlyout(NutCenterAlignmentBox);
        //if (nutSliderFlyout != null)
        //    nutSliderFlyout.Closed += NutSliderFlyout_Closed;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // Detach from previous ViewModel
        if (_attachedViewModel != null)
        {
            _attachedViewModel.InstrumentTypeChanged -= ViewModel_InstrumentTypeChanged;
            _attachedViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _attachedViewModel = null;
        }

        if (DataContext is StringSpacingPanelViewModel viewModel)
        {
            viewModel.InstrumentTypeChanged += ViewModel_InstrumentTypeChanged;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _attachedViewModel = viewModel;
            if (IsLoaded)
                RebuildPresetFlyouts();
        }
    }

    

    //private void NutSliderFlyout_Closed(object? sender, System.EventArgs e)
    //{
    //    if (ViewModel != null) ViewModel.IsEditingBySlider = false;
    //}

    private void ShowSliderButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            //ViewModel.IsEditingBySlider = true;
            //NutAlignmentSlider.Value = (double)vm.NutManualAlignment;
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
        if (provider == null) return null;
        var menu = new MenuFlyout();
        var presets = provider.GetNutSpacingPresets();
        if (presets.Count == 0) return null;
        menu.Items.Add(new MenuItem() {
            Header = Lang.Resources.StringSpacingEditorPanel_SpacingPresetHeader, 
            IsHitTestVisible = false, 
            FontWeight = Avalonia.Media.FontWeight.Bold,
        });
        foreach (var preset in presets)
        {
            var labelGrid = new Grid() { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            labelGrid.Children.Add(new TextBlock() { Text = preset.Name });
            Grid.SetColumn(labelGrid.Children[0], 0);
            labelGrid.Children.Add(new TextBlock() { Text = $"({preset.Spacing.ToStringFormatted()})" });
            Grid.SetColumn(labelGrid.Children[1], 1);
            var item = new MenuItem() { Header = labelGrid, Tag = preset };
            item.Click += (s, e) =>
            {
                if (s is MenuItem mi && mi.Tag is SpacingPreset np && ViewModel != null)
                {
                    ViewModel.NutSpacing = np.Spacing;
                }
            };
            menu.Items.Add(item);
        }

        return menu;
    }

    private FlyoutBase? CreateBridgeSpacingPresetMenu(IInstrumentValuesProvider provider)
    {
        if (provider == null) return null;
        var menu = new MenuFlyout();
        var presets = provider.GetBridgeSpacingPresets();
        if (presets.Count == 0) return null;
        menu.Items.Add(new MenuItem()
        {
            Header = Lang.Resources.StringSpacingEditorPanel_SpacingPresetHeader,
            IsHitTestVisible = false,
            FontWeight = Avalonia.Media.FontWeight.Bold,
        });
        foreach (var preset in presets)
        {
            var labelGrid = new Grid() {  ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            labelGrid.Children.Add(new TextBlock() { Text = preset.Name });
            Grid.SetColumn(labelGrid.Children[0], 0);
            labelGrid.Children.Add(new TextBlock() { Text = $"({preset.Spacing.ToStringFormatted()})" });
            Grid.SetColumn(labelGrid.Children[1], 1);
            var item = new MenuItem() { Header = labelGrid, Tag = preset };
            item.Click += (s, e) =>
            {
                if (s is MenuItem mi && mi.Tag is SpacingPreset np && ViewModel != null)
                {
                    ViewModel.BridgeSpacing = np.Spacing;
                }
            };
            menu.Items.Add(item);
        }
        return menu;
    }

    #endregion
}