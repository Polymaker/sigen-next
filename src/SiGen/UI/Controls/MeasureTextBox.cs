using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using SiGen.Converters;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        [TypeConverter(typeof(MeasureTypeConverter))]
        public Measure? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        [TypeConverter(typeof(MeasureTypeConverter))]
        public Measure? MinimumValue
        {
            get => GetValue(MinimumValueProperty);
            set => SetValue(MinimumValueProperty, value);
        }

        [TypeConverter(typeof(MeasureTypeConverter))]
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

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            ConfigureContextMenu();
        }

        private void ConfigureContextMenu()
        {
            var baseContextMenu = ContextMenu ?? new ContextMenu();

            baseContextMenu.Opening += OnContextMenuOpening;

            if (baseContextMenu.Items.Count == 0)
            {
                PopulateDefaultContextMenuItems(baseContextMenu);
            }

            baseContextMenu.Items.Add(new Separator());
            var convertMenuItem = new MenuItem { Header = "Convert", Tag = "CONVERT" }; //todo: localize

            var mmMenuItem = new MenuItem { Header = "mm", Tag = LengthUnit.Mm };
            mmMenuItem.Click += (s, e) => ConvertToUnit(LengthUnit.Mm);
            
            var cmMenuItem = new MenuItem { Header = "cm", Tag = LengthUnit.Cm };
            cmMenuItem.Click += (s, e) => ConvertToUnit(LengthUnit.Cm);
            
            var inMenuItem = new MenuItem { Header = "in", Tag = LengthUnit.In };
            inMenuItem.Click += (s, e) => ConvertToUnit(LengthUnit.In);
            
            convertMenuItem.Items.Add(mmMenuItem);
            convertMenuItem.Items.Add(cmMenuItem);
            convertMenuItem.Items.Add(inMenuItem);
            
            
            baseContextMenu.Items.Add(convertMenuItem);
            
            ContextMenu = baseContextMenu;
        }

        private void OnContextMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (contextMenu == null) return;

           var convertMenuItem = contextMenu.Items.OfType<MenuItem>().FirstOrDefault(item => item.Tag?.ToString() == "CONVERT");
            if (convertMenuItem != null)
            {
                convertMenuItem.IsEnabled = Value.HasValue; // Enable "Convert" only if there's a valid measure
                if (Value.HasValue)
                {
                    foreach (MenuItem unitItem in convertMenuItem.Items.OfType<MenuItem>())
                    {
                        if (unitItem.Tag is LengthUnit targetUnit)
                        {
                            unitItem.IsEnabled = Value.Value.Unit != targetUnit; // Enable only if it's a different unit
                        }
                    }
                } 
            }
        }

        private void PopulateDefaultContextMenuItems(ContextMenu contextMenu)
        {
            var cutMenuItem = new MenuItem { Header = "Cut" }; //todo: localize
            cutMenuItem.Command = new RelayCommand(Cut, () => CanCut);
            cutMenuItem.InputGesture = CutGesture;
            
            var copyMenuItem = new MenuItem { Header = "Copy" }; //todo: localize
            copyMenuItem.Command = new RelayCommand(Copy, () => CanCopy);
            copyMenuItem.InputGesture = CopyGesture;

            var pasteMenuItem = new MenuItem { Header = "Paste" }; //todo: localize
            pasteMenuItem.Command = new RelayCommand(Paste, () => CanPaste);
            pasteMenuItem.InputGesture = PasteGesture;

            contextMenu.Items.Add(cutMenuItem);
            contextMenu.Items.Add(copyMenuItem);
            contextMenu.Items.Add(pasteMenuItem);
        }

        private void ConvertToUnit(LengthUnit targetUnit)
        {
            if (Value.HasValue && Value.Value.Unit != targetUnit)
            {
                var currentValue = Value.Value;
                var convertedValue = SiGen.Measuring.Measure.FromNormalizedValue(targetUnit, currentValue.NormalizedValue);
                Value = convertedValue; 
                //since the real measure stays the same but the unit changes, the Value change won't trigger, so we need to manually apply the new text
                ApplyValueToText(); 
            }
        }

        private static Measure? CoerceValue(AvaloniaObject sender, Measure? value)
        {
            if (value is null) return null; // Allow null values

            var min = (sender as MeasureTextBox)?.MinimumValue;
            var max = (sender as MeasureTextBox)?.MaximumValue;
            var coerced = value;

            if (min is not null && coerced.HasValue && coerced.Value.CompareTo(min) < 0)
                coerced = min;
            if (max is not null && coerced.HasValue && coerced.Value.CompareTo(max) > 0)
                coerced = max;

            return coerced;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ValueProperty)
            {
                ApplyValueToText();
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

        private void ApplyValueToText()
        {
            if (Value is not null)
                Text = Value.Value.ToStringFormatted();
            else
                Text = string.Empty;
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
                Text = Value.Value.ToStringFormatted();
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
                var coerced = CoerceValue(this, parsed);
                if (Value != coerced)
                    Value = coerced;
                else
                    Text = Value?.ToStringFormatted() ?? string.Empty;
            }
            else
            {
                // Invalid entry, restore formatted value or set error style
                Text = Value?.ToStringFormatted() ?? string.Empty;
            }
        }
    }
}
