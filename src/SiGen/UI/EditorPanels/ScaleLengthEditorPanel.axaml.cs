using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Converters;
using SiGen.Layouts.Configuration;
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
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is ScaleLengthPanelViewModel viewModel)
        {
            viewModel.ConfigurationChanged += ViewModel_ConfigurationChanged;
        }
    }

    private void ViewModel_ConfigurationChanged(object? sender, EventArgs e)
    {
        //todo: get the difference between the bass and treble scale length and set the min/max of the SkewMeasureBox to the difference plus/minus a constant (e.g. 20mm)

    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureSingleScalePresets();
        //var skewHelpTextBlock = new TextBlock();
        //skewHelpTextBlock.Inlines = StringToInlinesConverter.Instance.Convert(Lang.Help.BassTrebleSkew_Help, typeof(InlineCollection), null, CultureInfo.CurrentCulture) as InlineCollection;
        //BassTrebleSkewField.Help = skewHelpTextBlock;
    }

    private void ConfigureSingleScalePresets()
    {
        var provider = new ElectricGuitarValuesProvider();
        var scaleLengths = provider.GetScaleLengthPresets();
        var stackPanel = new StackPanel() { Spacing = 4 };
        foreach (var scaleLength in scaleLengths)
        {
            var button = new Button
            {
                Content = $"{scaleLength.Name} ({scaleLength.ScaleLength.ToStringFormatted()})",
                Tag = scaleLength.ScaleLength
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