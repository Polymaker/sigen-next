using SiGen.Layouts.Data;
using SiGen.Measuring;
using System.Text.Json.Serialization;

namespace SiGen.Layouts.Configuration
{
    /// <summary>
    /// The configuration for a group of strings (e.g. a course of a 12-string guitar or mandolin).
    /// </summary>
    public class StringGroupConfiguration : BaseStringConfiguration
    {
        /// <summary>
        /// Spacing between the strings in the group
        /// </summary>
        public Measure? Spacing { get; set; } //todo: separate spacing for nut and bridge?
        public List<StringProperties> Strings { get; set; } = new List<StringProperties>();

        [JsonIgnore]
        public override int NumberOfStrings => Strings.Count;

        public override bool IsStringCourse => true;

        public Measure? GetGauge(int index)
        {
            if (index < 0 || index >= Strings.Count)
                return null;
            return Strings[index].Gauge;
        }

        public Measure GetTotalSpacing()
        {
            var spacing = Measure.IsNullOrEmpty(Spacing) ? Measure.Mm(1.5) : Spacing;
            return spacing.Value * (NumberOfStrings - 1);
        }

        public override Measure? GetTotalWidth()
        {
            Measure measure = Measure.Zero;
            var spacing = Measure.IsNullOrEmpty(Spacing) ? Measure.Mm(1.5) : Spacing.Value; 
            measure += spacing * (NumberOfStrings - 1);

            foreach (var str in Strings)
            {
                if (!Measure.IsNullOrEmpty(str.Gauge))
                    measure += str.Gauge.Value;
            }

            return measure;
        }

        public override Measure GetStringSpan(bool includeGauge)
        {
            var spacing = GetTotalSpacing();
            if (includeGauge)
            {
                if (Strings[0].Gauge.HasValue)
                    spacing += Strings[0].Gauge!.Value / 2d;

                if (Strings[^1].Gauge.HasValue)
                    spacing += Strings[^1].Gauge!.Value / 2d;
            }
            return spacing;
        }

        public override Measure GetHalfWidth(FingerboardSide side, bool includeGauge)
        {
            if (!includeGauge)
                return GetTotalSpacing() / 2d;

            var half = GetTotalSpacing() / 2d;
            if (side == FingerboardSide.Bass && Strings[0].Gauge.HasValue)
                half += Strings[0].Gauge!.Value / 2d;
            else if (side == FingerboardSide.Treble && Strings[^1].Gauge.HasValue)
                half += Strings[^1].Gauge!.Value / 2d;
            return half;
        }
    }
}
