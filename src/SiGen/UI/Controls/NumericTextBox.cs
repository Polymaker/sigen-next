using Avalonia;
using Avalonia.Controls;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;
using System.Timers;

namespace SiGen.UI.Controls
{
    public class NumericTextBox : Avalonia.Controls.TextBox
    {
        public static readonly StyledProperty<double?> ValueProperty =
           AvaloniaProperty.Register<MeasureTextBox, double?>(nameof(Value), coerce: CoerceValue);

        public static readonly StyledProperty<double?> MinimumValueProperty =
            AvaloniaProperty.Register<MeasureTextBox, double?>(nameof(MinimumValue));

        public static readonly StyledProperty<double?> MaximumValueProperty =
            AvaloniaProperty.Register<MeasureTextBox, double?>(nameof(MaximumValue));

        public static readonly StyledProperty<bool> AllowEmptyProperty =
            AvaloniaProperty.Register<MeasureTextBox, bool>(nameof(AllowEmpty), false);

        public static readonly StyledProperty<bool> AllowDecimalsProperty =
            AvaloniaProperty.Register<MeasureTextBox, bool>(nameof(AllowDecimals), true);

        protected override Type StyleKeyOverride => typeof(TextBox);

        public double? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double? MinimumValue
        {
            get => GetValue(MinimumValueProperty);
            set => SetValue(MinimumValueProperty, value);
        }

        public double? MaximumValue
        {
            get => GetValue(MaximumValueProperty);
            set => SetValue(MaximumValueProperty, value);
        }

        public bool AllowEmpty
        {
            get => GetValue(AllowEmptyProperty);
            set => SetValue(AllowEmptyProperty, value);
        }

        public bool AllowDecimals
        {
            get => GetValue(AllowDecimalsProperty);
            set => SetValue(AllowDecimalsProperty, value);
        }

        private static double? CoerceValue(AvaloniaObject sender, double? value)
        {
            if (value is null) return null; // Allow null values
            var numericTextBox = sender as NumericTextBox;
            var min = numericTextBox?.MinimumValue;
            var max = numericTextBox?.MaximumValue;
            var coerced = value;

            if (min is not null && coerced.HasValue && coerced.Value.CompareTo(min) < 0)
                coerced = min;
            if (max is not null && coerced.HasValue && coerced.Value.CompareTo(max) > 0)
                coerced = max;

            if (numericTextBox != null && !numericTextBox.AllowDecimals)
                coerced = Math.Truncate(coerced.Value);

            return coerced;
        }

        private string? _lastValidText;
        private bool _isEditing;
        private Timer? _commitTimer;
        private const int CommitDelayMs = 800;

        public NumericTextBox()
        {
            this.LostFocus += OnLostFocus;
            //this.KeyDown += OnKeyDown;
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            _isEditing = true;
            StartCommitTimer();
        }


        //private void OnKeyDown(object? sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Back) 
        //    {
        //        _isEditing = true;
        //        StartCommitTimer();
        //    }
        //    if (e.Key == Key.Enter)
        //    {
        //        CommitText();
        //        e.Handled = true;
        //    }
        //}
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                _isEditing = true;
                StartCommitTimer();
            }
            if (e.Key == Key.Enter)
            {
                CommitText();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private void OnLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            CommitText();
        }

        private void StartCommitTimer()
        {
            _commitTimer?.Stop();
            if (_commitTimer == null)
            {
                _commitTimer = new Timer(CommitDelayMs);
                _commitTimer.Elapsed += (s, e) =>
                {
                    _commitTimer?.Stop();
                    
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        if (Text != null && (Text.EndsWith(".") || Text.EndsWith(",")))
                            return;
                        CommitText();
                    });
                };
                _commitTimer.AutoReset = false;
            }
            _commitTimer.Start();
        }

        private void CommitText()
        {
            if (!_isEditing) return;
            _isEditing = false;
            _commitTimer?.Stop();
            if (string.IsNullOrWhiteSpace(Text))
            {
                if (AllowEmpty)
                {
                    Value = null;
                    _lastValidText = null;
                }
                else
                {
                    Text = _lastValidText ?? "";
                }
                return;
            }
            if (double.TryParse(Text, out var val)) //todo: consider culture
            {
                var coerced = CoerceValue(this, val);
                Value = coerced;
                var coercedText = coerced?.ToString() ?? "";
                if (Text != coercedText) Text = coercedText;
                _lastValidText = Text;
            }
            else
            {
                // Restore last valid text if parse fails
                Text = _lastValidText ?? "";
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ValueProperty)
            {
                if (!_isEditing)
                {
                    Text = Value?.ToString() ?? "";
                    _lastValidText = Text;
                }
            }
        }
    }
}
