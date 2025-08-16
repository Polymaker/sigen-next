using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Layouts.Configuration;
using SiGen.Services.InstrumentProfiles;
using SiGen.ViewModels.EditorPanels;
using System;

namespace SiGen.UI.EditorPanels;

public partial class ScaleLengthEditorPanel : UserControl
{
    public ScaleLengthEditorPanel()
    {
        InitializeComponent();
        TestButton.Click += TestButton_Click;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is ScaleLengthPanelViewModel viewModel)
        {
            
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureSingleScalePresets();
    }

    private void TestButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ScaleLengthPanelViewModel viewModel)
        {
            
        }
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