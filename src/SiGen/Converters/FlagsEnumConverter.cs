using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SiGen.Converters
{
    /// <summary>
    /// Converter for checking and toggling flags in a flags enum
    /// </summary>
    public class FlagsEnumConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            if (!value.GetType().IsEnum || !parameter.GetType().IsEnum)
                return false;

            var enumValue = System.Convert.ToInt32(value);
            var flagValue = System.Convert.ToInt32(parameter);

            return (enumValue & flagValue) == flagValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // This converter is one-way only for checking
            throw new NotImplementedException();
        }
    }
}
