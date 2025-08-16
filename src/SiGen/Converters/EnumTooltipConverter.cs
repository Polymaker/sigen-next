using Avalonia.Data.Converters;
using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Converters
{
    public class EnumTooltipConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ScaleLengthMode scaleLengthMode)
            {
                string key = $"Editor.ScaleLengthMode.{scaleLengthMode}.Tooltip";
                return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
