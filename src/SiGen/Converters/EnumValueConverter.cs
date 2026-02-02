using Avalonia.Data.Converters;
using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Localization;
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
                //if (parameter as string == "Tooltip")
                //{
                //    string key = $"Editor.StringSpacingMode.{spacingMode}.Tooltip";
                //    return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
                //}
                string key = $"StringSpacingMode.{spacingMode}";
                return Texts.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is LayoutCenterAlignment centerAlignment)
            {
                //if (parameter as string == "Tooltip")
                //{
                //    string key = $"Editor.StringSpacingMode.{spacingMode}.Tooltip";
                //    return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
                //}
                string key = $"LayoutCenterAlignment.{centerAlignment}";
                return Texts.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is MarginMode marginMode)
            {
                //if (parameter as string == "Tooltip")
                //{
                //    string key = $"Editor.StringSpacingMode.{spacingMode}.Tooltip";
                //    return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
                //}
                string key = $"Editor.MarginMode.{marginMode}";
                return Lang.Resources.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is InstrumentType instrument)
            {
                string key = $"InstrumentType.{instrument}";
                return Lang.Resources.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            return value?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
