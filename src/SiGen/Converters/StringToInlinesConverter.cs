using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Converters
{
    public class StringToInlinesConverter : IValueConverter
    {
        public static readonly StringToInlinesConverter Instance = new StringToInlinesConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string text)
                return null;

            var inlines = new InlineCollection();
            try
            {
                var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (i > 0)
                        inlines.Add(new LineBreak());
                    if (!string.IsNullOrEmpty(lines[i]))
                        inlines.Add(new Run(lines[i]));
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during conversion
                // For example, log the error or return an empty list
                Console.WriteLine($"Error converting string to inlines: {ex.Message}");
                return inlines;
            }
           

            return inlines;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
