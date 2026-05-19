using Avalonia.Data.Converters;
using SiGen.Lang;
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
            {
                if (parameter is string mode && mode.Equals("RecentRelative", StringComparison.OrdinalIgnoreCase))
                    return FormatRecentDateLabel(dt, culture);

                return dt.ToString(parameter as string ?? Format, culture);
            }

            return string.Empty;
        }

        private static string FormatRecentDateLabel(DateTime dateTime, CultureInfo culture)
        {
            var uiCulture = /*Resources.Culture ?? culture ?? */CultureInfo.CurrentUICulture;
            var now = DateTime.Now;
            var time = dateTime.ToString("HH:mm", uiCulture);

            if (dateTime.Date == now.Date)
                return string.Format(uiCulture, GetResourceString("DateLabel.Today", uiCulture, "Today {0}"), time);

            if (dateTime.Date == now.Date.AddDays(-1))
                return string.Format(uiCulture, GetResourceString("DateLabel.Yesterday", uiCulture, "Yesterday {0}"), time);

            if (dateTime > now.AddMonths(-1))
                return dateTime.ToString(GetResourceString("DateFormat.WithinMonth", uiCulture, "MMM d HH:mm"), uiCulture);

            if (dateTime <= now.AddYears(-1))
                return dateTime.ToString(GetResourceString("DateFormat.OlderThanYear", uiCulture, "MMM d yyyy"), uiCulture);

            return dateTime.ToString(GetResourceString("DateFormat.WithinYear", uiCulture, "MMM d"), uiCulture);
        }

        private static string GetResourceString(string key, CultureInfo culture, string fallback)
        {
            return Resources.ResourceManager.GetString(key, culture) ?? fallback;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
