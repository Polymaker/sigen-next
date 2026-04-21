using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SiGen.Converters
{
    public class ZoomPercentageConverter : IValueConverter
    {
        public int DecimalDigits { get; set; } = 0;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double zoom)
                return zoom.ToString($"P{DecimalDigits}", culture);

            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var text = value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return AvaloniaProperty.UnsetValue;

            var percentSymbol = culture.NumberFormat.PercentSymbol;
            var hasPercent = text.Contains(percentSymbol, StringComparison.CurrentCulture) || text.Contains("%", StringComparison.Ordinal);
            var normalizedText = text.Replace(percentSymbol, string.Empty).Replace("%", string.Empty).Trim();

            if (!double.TryParse(normalizedText, NumberStyles.Float | NumberStyles.AllowThousands, culture, out var number))
                return AvaloniaProperty.UnsetValue;

            return hasPercent || number > 1d ? number / 100d : number;
        }
    }
}
