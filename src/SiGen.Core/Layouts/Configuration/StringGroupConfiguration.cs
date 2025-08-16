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
        public int StringCount => Strings.Count;

        public Measure? GetGauge(int index)
        {
            if (index < 0 || index >= Strings.Count)
                return null;
            return Strings[index].Gauge;
        }

        public Measure GetTotalSpacing()
        {
            var spacing = Measure.IsNullOrEmpty(Spacing) ? Measure.Mm(1.5) : Spacing;
            return spacing * (StringCount - 1);
        }

        public override Measure? GetTotalWidth()
        {
            Measure measure = Measure.Zero;
            var spacing = Measure.IsNullOrEmpty(Spacing) ? Measure.Mm(1.5) : Spacing; 
            measure += spacing * (StringCount - 1);

            foreach (var str in Strings)
            {
                if (!Measure.IsNullOrEmpty(str.Gauge))
                    measure += str.Gauge;
            }

            return measure;
        }
    }
}
