using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Converters;
using SiGen.Layouts.Configuration;
using SiGen.Services;
using SiGen.Services.InstrumentProfiles;
using SiGen.ViewModels.EditorPanels;
using System;
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

        // Detach from previous ViewModel
        if (_attachedViewModel != null)
        {
            _attachedViewModel.ConfigurationChanged -= ViewModel_ConfigurationChanged;
            _attachedViewModel.InstrumentTypeChanged -= ViewModel_InstrumentTypeChanged;
            _attachedViewModel = null;
        }

        if (DataContext is ScaleLengthPanelViewModel viewModel)
        {
            viewModel.ConfigurationChanged += ViewModel_ConfigurationChanged;
            viewModel.InstrumentTypeChanged += ViewModel_InstrumentTypeChanged;
            _attachedViewModel = viewModel;
            if (IsLoaded)
                ConfigureSingleScalePresets(viewModel.LayoutDocumentContext.InstrumentValuesProvider);
        }
    }


    private void ViewModel_InstrumentTypeChanged(object? sender, EventArgs e)
    {
        if (ViewModel != null)
            ConfigureSingleScalePresets(ViewModel.LayoutDocumentContext.InstrumentValuesProvider);
    }

    private void ViewModel_ConfigurationChanged(object? sender, EventArgs e)
    {
        //todo: get the difference between the bass and treble scale length and set the min/max of the SkewMeasureBox to the difference plus/minus a constant (e.g. 20mm)

    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (ViewModel != null)
            ConfigureSingleScalePresets(ViewModel.LayoutDocumentContext.InstrumentValuesProvider);
        //var skewHelpTextBlock = new TextBlock();
        //skewHelpTextBlock.Inlines = StringToInlinesConverter.Instance.Convert(Lang.Help.BassTrebleSkew_Help, typeof(InlineCollection), null, CultureInfo.CurrentCulture) as InlineCollection;
        //BassTrebleSkewField.Help = skewHelpTextBlock;
    }


    private void ConfigureSingleScalePresets(IInstrumentValuesProvider? provider)
    {
        //var provider = new ElectricGuitarValuesProvider();
        if (provider == null)
        {
            SingleScaleLengthField.Info = null;
            return;
        }

        var scaleLengths = provider.GetScaleLengthPresets();
        var stackPanel = new StackPanel() { Spacing = 4 };
        //string instrumentType = Lang.Resources.ResourceManager.GetString($"InstrumentType.{provider.InstrumentType}", Lang.Resources.Culture) ?? provider.InstrumentType.ToString();
        stackPanel.Children.Add(new TextBlock
        {
            Text = Lang.Help.SingleScale_Presets_Title,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MaxWidth = 220
        });
        foreach (var scaleLength in scaleLengths)
        {
            var button = new Button
            {
                Content = $"{scaleLength.Name} ({scaleLength.ScaleLength.ToStringFormatted()})",
                Tag = scaleLength.ScaleLength,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };
            button.Click += (s, args) =>
            {
                SingleScaleLengthField.CloseInfo();
                if (DataContext is ScaleLengthPanelViewModel viewModel && button.Tag is Measuring.Measure length)
                {
                    viewModel.SingleScale = length;
                }
            };
            stackPanel.Children.Add(button);
        }

        SingleScaleLengthField.Info = stackPanel;
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