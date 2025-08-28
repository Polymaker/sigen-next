using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SiGen.Measuring
{
    public static class MeasureParser
    {
        private static readonly Regex _unitRegex = new(@"
            (?<value>[+-]?[\d.,]+(?:\s*\d+\/\d+)?)\s*
            (?<unit>mm|in|inch|po|pi|cm|ft|\'|\"")?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);


        public static bool TryParse(string? input, [NotNullWhen(true)] out Measure? measure, LengthUnit? defaultUnit = null)
        {
            measure = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Replace(",", ".", StringComparison.Ordinal);

            var match = _unitRegex.Match(input);
            if (!match.Success)
                return false;

            var valueGroup = match.Groups["value"].Value;
            var unitGroup = match.Groups["unit"].Value.ToLowerInvariant();

            if (!TryParseFractionalOrDecimal(valueGroup, out double value))
                return false;

            LengthUnit unit;
            if (string.IsNullOrEmpty(unitGroup))
            {
                // Check for any letter in the input (invalid unit)
                if (input.Any(char.IsLetter))
                    return false;
                if (defaultUnit == null)
                    return false;
                unit = defaultUnit.Value;
            }
            else
            {
                try
                {
                    unit = unitGroup switch
                    {
                        "mm" => LengthUnit.Mm,
                        "cm" => LengthUnit.Cm,
                        "in" or "inch" or "pi" or "po" or "\"" => LengthUnit.In,
                        "ft" or "'" => LengthUnit.Ft,
                        _ => throw new FormatException($"Unrecognized unit: {unitGroup}")
                    };
                }
                catch
                {
                    return false;
                }
            }

            measure = new Measure(unit, value);
            return true;
        }

        private static bool TryParseFractionalOrDecimal(string valueText, out double value)
        {
            value = 0;
            valueText = valueText.Trim();

            try
            {
                // Handle fractional part: e.g. "4 1/2"
                if (valueText.Contains('/'))
                {
                    var parts = valueText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    double whole = 0, fraction = 0;

                    if (parts.Length == 2)
                    {
                        whole = double.Parse(parts[0], CultureInfo.InvariantCulture);
                        var fracParts = parts[1].Split('/');
                        fraction = double.Parse(fracParts[0], CultureInfo.InvariantCulture) /
                                   double.Parse(fracParts[1], CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        var fracParts = valueText.Split('/');
                        fraction = double.Parse(fracParts[0], CultureInfo.InvariantCulture) /
                                   double.Parse(fracParts[1], CultureInfo.InvariantCulture);
                    }

                    value = whole + fraction;
                    return true;
                }

                value = double.Parse(valueText, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
