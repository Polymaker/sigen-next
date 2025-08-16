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
            AvaloniaProperty.Register<MeasureTextBox, Measure?>(nameof(Value));

        protected override Type StyleKeyOverride => typeof(TextBox);

        public Measure? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public MeasureTextBox()
        {
            AddHandler(GotFocusEvent, OnGotFocus, RoutingStrategies.Tunnel);
            AddHandler(LostFocusEvent, OnLostFocus, RoutingStrategies.Bubble);
            //Watermark = "Enter measurement"; // Optional: set a watermark 

        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ValueProperty)
            {
                // Update the Text property when Value changes
                if (Value is not null)
                {
                    Text = Value.ToStringFormatted(); // e.g., 4 1/2"
                }
                else
                {
                    Text = string.Empty; // Clear text if Value is null
                }
            }
        }

        private void OnGotFocus(object? sender, GotFocusEventArgs e)
        {
            if (Value is not null)
                Text = Value.ToStringFormatted(); 
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (MeasureParser.TryParse(Text ?? string.Empty, out var parsed, Value?.Unit))
            {
                Value = parsed;
                Text = parsed.ToStringFormatted(); // e.g., 4 1/2"
            }
            else
            {
                // Invalid entry, restore formatted value or set error style
                Text = Value?.ToStringFormatted() ?? string.Empty;
            }
        }
    }
}
