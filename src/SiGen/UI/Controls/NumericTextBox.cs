using Avalonia;
using Avalonia.Controls;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private static double? CoerceValue(AvaloniaObject sender, double? value)
        {
            if (value is null) return null; // Allow null values

            var min = (sender as NumericTextBox)?.MinimumValue;
            var max = (sender as NumericTextBox)?.MaximumValue;
            var coerced = value;

            if (min is not null && coerced.HasValue && coerced.Value.CompareTo(min) < 0)
                coerced = min;
            if (max is not null && coerced.HasValue && coerced.Value.CompareTo(max) > 0)
                coerced = max;

            return coerced;
        }
    }
}
