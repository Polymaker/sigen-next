using Avalonia.Data.Converters;
using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Localization;
using SiGen.Physics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Converters
{
    public class EnumValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ScaleLengthMode scaleLengthMode)
            {
                string key = $"Editor.ScaleLengthMode.{scaleLengthMode}.Tooltip";
                return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
            }
            else if (value is StringSpacingMode spacingMode)
            {
                string key = $"StringSpacingMode.{spacingMode}";
                return Texts.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is LayoutCenterAlignment centerAlignment)
            {
                string key = $"LayoutCenterAlignment.{centerAlignment}";
                return Texts.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is MarginMode marginMode)
            {
                string key = $"Editor.MarginMode.{marginMode}";
                return Lang.Resources.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is InstrumentType instrument)
            {
                string key = $"InstrumentType.{instrument}";
                return Lang.Resources.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is Temperament temperament)
            {
                string key = $"Temperament.{temperament}";
                return Texts.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            return value?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
