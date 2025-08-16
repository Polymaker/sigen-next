using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SiGen.UI.Controls;

public partial class StringCountControl : UserControl
{
    public static readonly StyledProperty<int> StringCountProperty =
            AvaloniaProperty.Register<StringCountControl, int>(nameof(StringCount), 6);

    public static readonly StyledProperty<int> MinStringCountProperty =
        AvaloniaProperty.Register<StringCountControl, int>(nameof(MinStringCount), 1);

    public static readonly StyledProperty<int> MaxStringCountProperty =
        AvaloniaProperty.Register<StringCountControl, int>(nameof(MaxStringCount), 20);

    public static readonly StyledProperty<bool> IsLeftHandedProperty =
        AvaloniaProperty.Register<StringCountControl, bool>(nameof(IsLeftHanded), false);


    public static readonly StyledProperty<StringCountViewModel> ViewModelProperty =
        AvaloniaProperty.Register<StringCountControl, StringCountViewModel>(nameof(ViewModel));
    private StackPanel? _stringDotsPanel;
    //private StringCountViewModel _viewModel;

    private StringCountViewModel ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public StringCountControl()
    {
        InitializeComponent();
        ViewModel = new StringCountViewModel(this);
        //DataContext = _viewModel;

        // Subscribe to property changes
        PropertyChanged += OnPropertyChanged;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _stringDotsPanel = this.FindControl<StackPanel>("StringDotsPanel");
        UpdateStringDots();
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
    }
    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == StringCountProperty || e.Property == IsLeftHandedProperty)
        {
            UpdateStringDots();
            ViewModel.NotifyPropertiesChanged();
        }
    }

    public int StringCount
    {
        get => GetValue(StringCountProperty);
        set => SetValue(StringCountProperty, value);
    }

    public int MinStringCount
    {
        get => GetValue(MinStringCountProperty);
        set => SetValue(MinStringCountProperty, value);
    }

    public int MaxStringCount
    {
        get => GetValue(MaxStringCountProperty);
        set => SetValue(MaxStringCountProperty, value);
    }

    public bool IsLeftHanded
    {
        get => GetValue(IsLeftHandedProperty);
        set => SetValue(IsLeftHandedProperty, value);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateStringDots();
    }

    private void UpdateStringDots()
    {
        if (_stringDotsPanel == null) return;

        _stringDotsPanel.Children.Clear();

        double dotWidth = 12;
        double dotMargin = _stringDotsPanel.Spacing;
        double totalDotWidth = dotWidth + dotMargin;

        // Get available width
        double availableWidth = DotContainer.Bounds.Width - (DotContainer.Padding.Left + DotContainer.Padding.Right);
        if (double.IsNaN(availableWidth) || availableWidth == 0)
        {
            // Try to use panel's actual width if Bounds is not set yet
            availableWidth = DotContainer.Width - (DotContainer.Padding.Left + DotContainer.Padding.Right);
        }
        if (double.IsNaN(availableWidth) || availableWidth <= 0)
        {
            // Fallback: show all dots if width is not available yet
            availableWidth = double.PositiveInfinity;
        }

        int maxDots = (int)Math.Floor(availableWidth / totalDotWidth);
        if (maxDots < 3) maxDots = 3; // Always show at least 3 dots (first, last, and one skipped label if needed)

        if (StringCount <= maxDots)
        {
            // Show all dots
            for (int i = 0; i < StringCount; i++)
            {
                var dot = CreateDot(i);
                _stringDotsPanel.Children.Add(dot);
            }
        }
        else
        {
            // Show first dot, skipped label, last dot
            int dotsToShow = maxDots;
            if (maxDots % 2 == 0 && maxDots > 3)
            {
                // If even, show one less dot to keep balance
                dotsToShow--;
            }
            int skipped = StringCount - (dotsToShow - 1);

            // Always show first dot
            var firstDot = CreateDot(0);
            _stringDotsPanel.Children.Add(firstDot);

            int centerIndex = dotsToShow / 2;

            // Add left-side dots (if any)
            for (int i = 1; i < centerIndex; i++)
            {
                var dot = CreateDot(i);
                _stringDotsPanel.Children.Add(dot);
            }

            // Add skipped label
            var skippedLabel = new TextBlock
            {
                Text = $"+{skipped}",
                FontSize = 12,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, -1, 0, 0),
                Classes = { "string-dot-skipped" }
            };
            _stringDotsPanel.Children.Add(skippedLabel);

            // Add right-side dots (if any)
            int rightStart = StringCount - (dotsToShow - centerIndex - 1);
            for (int i = rightStart; i < StringCount - 1; i++)
            {
                var dot = CreateDot(i);
                _stringDotsPanel.Children.Add(dot);
            }

            // Always show last dot
            var lastDot = CreateDot(StringCount - 1);
            _stringDotsPanel.Children.Add(lastDot);
        }

        Ellipse CreateDot(int i)
        {
            var dot = new Ellipse
            {
                Width = dotWidth,
                Height = dotWidth,
                //Margin = new Thickness(dotMargin),
            };

            // Color coding: first and last strings get special colors
            if (i == 0)
            {
                dot.Classes.Add("string-dot");
                dot.Classes.Add(IsLeftHanded ? "treble" : "bass");
            }
            else if (i == StringCount - 1)
            {
                dot.Classes.Add("string-dot");
                dot.Classes.Add(IsLeftHanded ? "bass" : "treble");
            }
            else
            {
                dot.Classes.Add("string-dot");
            }
            

            //// Add tooltip showing string position
            //string position = IsLeftHanded ?
            //    (i == 0 ? "Treble side (highest pitch)" :
            //     i == StringCount - 1 ? "Bass side (lowest pitch)" :
            //     $"{Lang.Resources.StringLabel} {i + 1}") :
            //    (i == 0 ? "Bass side (lowest pitch)" :
            //     i == StringCount - 1 ? "Treble side (highest pitch)" :
            //     $"{Lang.Resources.StringLabel} {i + 1}");

            ToolTip.SetTip(dot, $"{Lang.Resources.StringLabel} {i + 1}");

            return dot;
        }
    }

    // Event for notifying parent about string count changes
    public event EventHandler<StringCountChangedEventArgs>? StringCountChanged;

    internal void OnStringCountChanged(StringCountChangeType changeType)
    {
        StringCountChanged?.Invoke(this, new StringCountChangedEventArgs(StringCount, changeType, IsLeftHanded));
    }
}

public partial class StringCountViewModel : ObservableObject
{
    private readonly StringCountControl _control;

    public StringCountViewModel(StringCountControl control)
    {
        _control = control;
    }

    // Computed properties for UI binding
    public bool CanAddBassString => _control.StringCount < _control.MaxStringCount;
    public bool CanRemoveBassString => _control.StringCount > _control.MinStringCount;
    public bool CanAddTrebleString => _control.StringCount < _control.MaxStringCount;
    public bool CanRemoveTrebleString => _control.StringCount > _control.MinStringCount;

    public string StringCountText => _control.StringCount > 1 ? $"{_control.StringCount} {Lang.Resources.StringsLabel}" : $"{_control.StringCount} {Lang.Resources.StringLabel}";

    public int BassButtonsColumn => _control.IsLeftHanded ? 2 : 0;
    public int TrebleButtonsColumn => _control.IsLeftHanded ? 0 : 2;

    [RelayCommand(CanExecute = nameof(CanAddBassString))]
    private void AddBassString()
    {
        _control.StringCount++;
        _control.OnStringCountChanged(StringCountChangeType.AddedBass);
        NotifyPropertiesChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveBassString))]
    private void RemoveBassString()
    {
        _control.StringCount--;
        _control.OnStringCountChanged(StringCountChangeType.RemovedBass);
        NotifyPropertiesChanged();
    }

    [RelayCommand(CanExecute = nameof(CanAddTrebleString))]
    private void AddTrebleString()
    {
        _control.StringCount++;
        _control.OnStringCountChanged(StringCountChangeType.AddedTreble);
        NotifyPropertiesChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveTrebleString))]
    private void RemoveTrebleString()
    {
        _control.StringCount--;
        _control.OnStringCountChanged(StringCountChangeType.RemovedTreble);
        NotifyPropertiesChanged();
    }

    [RelayCommand]
    private void ToggleLayout()
    {
        _control.IsLeftHanded = !_control.IsLeftHanded;
        NotifyPropertiesChanged();
    }

    public void NotifyPropertiesChanged()
    {
        OnPropertyChanged(nameof(CanAddBassString));
        OnPropertyChanged(nameof(CanRemoveBassString));
        OnPropertyChanged(nameof(CanAddTrebleString));
        OnPropertyChanged(nameof(CanRemoveTrebleString));
        OnPropertyChanged(nameof(StringCountText));
        OnPropertyChanged(nameof(BassButtonsColumn));
        OnPropertyChanged(nameof(TrebleButtonsColumn));
    }
}

public enum StringCountChangeType
{
    AddedBass,
    RemovedBass,
    AddedTreble,
    RemovedTreble
}

public class StringCountChangedEventArgs : EventArgs
{
    public int NewStringCount { get; }
    public StringCountChangeType ChangeType { get; }
    public bool IsLeftHanded { get; }

    public StringCountChangedEventArgs(int newStringCount, StringCountChangeType changeType, bool isLeftHanded)
    {
        NewStringCount = newStringCount;
        ChangeType = changeType;
        IsLeftHanded = isLeftHanded;
    }
}