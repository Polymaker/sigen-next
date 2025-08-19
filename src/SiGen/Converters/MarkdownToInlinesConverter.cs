using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SiGen.Converters
{
    public class MarkdownToInlinesConverter : IValueConverter
    {
        public static readonly MarkdownToInlinesConverter Instance = new MarkdownToInlinesConverter();

        // Regex for headings, bold, italic, and newlines
        private static readonly Regex MarkdownRegex = new Regex(
            @"(^#{1,6}\s.*$)|(\*\*([^\*]+)\*\*)|(\*([^\*]+)\*)|(\r\n|\n)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var inlines = new InlineCollection();

            if (value is not string text || string.IsNullOrEmpty(text))
                return inlines;

            int lastIndex = 0;
            foreach (Match match in MarkdownRegex.Matches(text))
            {
                // Add text before the match
                if (match.Index > lastIndex)
                {
                    inlines.Add(new Run(text.Substring(lastIndex, match.Index - lastIndex)));
                }

                if (match.Groups[1].Success) // Heading (# ...)
                {
                    var headingText = match.Groups[1].Value.Trim();
                    int level = 0;
                    while (level < headingText.Length && headingText[level] == '#')
                        level++;
                    // Remove leading #'s and whitespace
                    headingText = headingText.Substring(level).Trim();

                    var run = new Run(headingText)
                    {
                        FontWeight = FontWeight.Bold,
                        FontSize = level switch
                        {
                            1 => 22,
                            2 => 18,
                            3 => 16,
                            _ => 14
                        }
                    };
                    inlines.Add(run);
                    //inlines.Add(new LineBreak());
                }
                else if (match.Groups[2].Success) // **bold**
                {
                    inlines.Add(new Run(match.Groups[3].Value) { FontWeight = FontWeight.Bold });
                }
                else if (match.Groups[4].Success) // *italic*
                {
                    inlines.Add(new Run(match.Groups[5].Value) { FontStyle = FontStyle.Italic });
                }
                else if (match.Groups[6].Success) // Newline
                {
                    inlines.Add(new LineBreak());
                }

                lastIndex = match.Index + match.Length;
            }

            // Add remaining text
            if (lastIndex < text.Length)
            {
                inlines.Add(new Run(text.Substring(lastIndex)));
            }

            return inlines;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
