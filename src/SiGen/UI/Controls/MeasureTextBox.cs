using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.Controls
{
    public class MeasureTextBox : TextBox
    {
        public static readonly StyledProperty<Measure?> ValueProperty =
            AvaloniaProperty.Register<MeasureTextBox, Measure?>(nameof(Value), coerce: CoerceValue);

        public static readonly StyledProperty<Measure?> MinimumValueProperty =
            AvaloniaProperty.Register<MeasureTextBox, Measure?>(nameof(MinimumValue));

        public static readonly StyledProperty<Measure?> MaximumValueProperty =
            AvaloniaProperty.Register<MeasureTextBox, Measure?>(nameof(MaximumValue));

        public static readonly StyledProperty<bool> AllowEmptyProperty =
            AvaloniaProperty.Register<MeasureTextBox, bool>(nameof(AllowEmpty), false);

        protected override Type StyleKeyOverride => typeof(TextBox);

        public Measure? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public Measure? MinimumValue
        {
            get => GetValue(MinimumValueProperty);
            set => SetValue(MinimumValueProperty, value);
        }

        public Measure? MaximumValue
        {
            get => GetValue(MaximumValueProperty);
            set => SetValue(MaximumValueProperty, value);
        }

        public bool AllowEmpty
        {
            get => GetValue(AllowEmptyProperty);
            set => SetValue(AllowEmptyProperty, value);
        }


        public MeasureTextBox()
        {
            AddHandler(GotFocusEvent, OnGotFocus, RoutingStrategies.Tunnel);
            AddHandler(LostFocusEvent, OnLostFocus, RoutingStrategies.Bubble);
            //Watermark = "Enter measurement"; // Optional: set a watermark 

        }

        private static Measure? CoerceValue(AvaloniaObject sender, Measure? value)
        {
            if (value is null) return null; // Allow null values

            var min = (sender as MeasureTextBox)?.MinimumValue;
            var max = (sender as MeasureTextBox)?.MaximumValue;
            var coerced = value;

            if (min is not null && coerced.CompareTo(min) < 0)
                coerced = min;
            if (max is not null && coerced.CompareTo(max) > 0)
                coerced = max;

            return coerced; 
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ValueProperty)
            {
                if (Value is not null)
                {
                    Text = Value.ToStringFormatted();
                }
                else
                {
                    Text = string.Empty;
                }
            }
            else if (change.Property == MinimumValueProperty || change.Property == MaximumValueProperty)
            {
                // Re-coerce Value if min/max changed
                if (Value is not null)
                {
                    var coerced = CoerceValue(this, Value);

                    if (coerced != null && !Equals(coerced, Value))
                        Value = coerced;
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.Enter)
                ValidateTextInput();
        }

        private void OnGotFocus(object? sender, GotFocusEventArgs e)
        {
            if (Value is not null)
                Text = Value.ToStringFormatted(); 
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            ValidateTextInput();
        }

        private void ValidateTextInput()
        {
            if (string.IsNullOrEmpty(Text) && AllowEmpty)
            {
                Value = null; // Clear value if text is empty and AllowEmpty is true
                return;
            }

            if (MeasureParser.TryParse(Text ?? string.Empty, out var parsed, Value?.Unit))
            {
                Value = parsed;
                //Text = parsed.ToStringFormatted(); // e.g., 4 1/2"
            }
            else
            {
                // Invalid entry, restore formatted value or set error style
                Text = Value?.ToStringFormatted() ?? string.Empty;
            }
        }
    }
}
