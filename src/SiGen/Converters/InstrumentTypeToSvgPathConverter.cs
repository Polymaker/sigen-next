using Avalonia.Data.Converters;
using System;
using System.Globalization;
using SiGen.Settings;
using SiGen.Data.Common;

namespace SiGen.Converters
{
    public class InstrumentTypeToSvgPathConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string strValue && Enum.TryParse<InstrumentType>(strValue, out var parsedType))
                return $"/Assets/Icons/InstrumentType_{parsedType}.svg";
            if (value is InstrumentType type)
                return $"/Assets/Icons/InstrumentType_{type}.svg";
            return "/Assets/Icons/InstrumentType_Custom.svg";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
