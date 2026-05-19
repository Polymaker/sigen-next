using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SiGen.Export;
using System;

namespace SiGen.UI.Controls;

public partial class LineThicknessEditor : UserControl
{
    public static readonly StyledProperty<LineThickness> ThicknessProperty =
        AvaloniaProperty.Register<LineThicknessEditor, LineThickness>(nameof(Thickness), new LineThickness(1, ThicknessUnit.Point));

    public LineThickness Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    private bool _isSyncing;

    public LineThicknessEditor()
    {
        InitializeComponent();
        PART_Unit.ItemsSource = Enum.GetValues<ThicknessUnit>();
        _isSyncing = true;
        PART_Value.Value = (decimal)Thickness.Value;
        PART_Unit.SelectedItem = Thickness.Unit;
        _isSyncing = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ThicknessProperty)
        {
            _isSyncing = true;
            var thickness = change.GetNewValue<LineThickness>();
            PART_Value.Value = (decimal)thickness.Value;
            PART_Unit.SelectedItem = thickness.Unit;
            _isSyncing = false;
        }
    }

    // Called by inner controls when the user edits
    private void OnInnerChanged()
    {
        if (_isSyncing) return;

        if (PART_Value.Value.HasValue && PART_Unit.SelectedItem is not null)
        {
            SetValue(ThicknessProperty, new LineThickness((double)PART_Value.Value.Value, (ThicknessUnit)PART_Unit.SelectedItem!));
        }
    }

    private void OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e) => OnInnerChanged();
    private void OnUnitChanged(object? sender, SelectionChangedEventArgs e) => OnInnerChanged();
}