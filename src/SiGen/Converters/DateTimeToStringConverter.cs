using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SiGen.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public string Format { get; set; } = "g"; // Default: general date/time

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
                return dt.ToString(parameter as string ?? Format, culture);
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
