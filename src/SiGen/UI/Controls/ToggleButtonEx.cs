using Avalonia;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.Controls
{
    public class ToggleButtonEx : ToggleButton
    {
        protected override Type StyleKeyOverride => typeof(ToggleButton);

        public static readonly StyledProperty<bool> ToggleOnClickProperty =
        AvaloniaProperty.Register<SvgIcon, bool>(nameof(ToggleOnClick), defaultValue: true);

        public event EventHandler<ValueChangingEventArgs<bool?>>? IsCheckedChanging;

        private bool ignoreToggle;

        public bool ToggleOnClick
        {
            get => GetValue(ToggleOnClickProperty);
            set => SetValue(ToggleOnClickProperty, value);
        }

        protected override void OnClick()
        {
            if (!ToggleOnClick)
                ignoreToggle = true;

            base.OnClick();

            ignoreToggle = false;
        }

        protected override void Toggle()
        {
            if (ignoreToggle) return;

            bool? newValue;
            if (IsChecked.HasValue)
            {
                if (IsChecked.Value)
                {
                    if (IsThreeState)
                    {
                        newValue = null;
                    }
                    else
                    {
                        newValue = false;
                    }
                }
                else
                {
                    newValue = true;
                }
            }
            else
            {
                newValue = false;
            }

            var args = new ValueChangingEventArgs<bool?>(IsChecked, newValue);
            IsCheckedChanging?.Invoke(this, args);
            if (!args.Handled)
                SetCurrentValue(IsCheckedProperty, newValue);
        }
    }

    public class ValueChangingEventArgs<T> : EventArgs
    {
        public T OldValue { get; }
        public T NewValue { get; set; }
        public bool Handled { get; set; }

        public ValueChangingEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
            Handled = false;
        }
    }
}
