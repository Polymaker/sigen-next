using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SiGen.Converters
{
    public class OrConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            foreach (var value in values)
            {
                if (value is bool b && b)
                    return true;
                if (value != null && value.Equals(true))
                    return true;
            }
            return false;
        }

        //public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        //{
        //    throw new NotSupportedException();
        //}
    }
}
