using Avalonia.Data.Converters;
using SiGen.Layouts.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Converters
{
    public class ScaleLengthModeToTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                ScaleLengthMode.Single => Lang.Resources.Editor_ScaleLengthMode_Single,
                ScaleLengthMode.Multiscale => Lang.Resources.Editor_ScaleLengthMode_Multiscale,
                ScaleLengthMode.PerString => Lang.Resources.Editor_ScaleLengthMode_PerString,
                _ => value?.ToString()
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
