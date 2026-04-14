using SiGen.Physics;
using System.Text.Json.Serialization;

namespace SiGen.Layouts.Configuration
{
    public class FretConfiguration
    {
        /// <summary>
        /// Specifies the starting fret index for this string.
        /// Set a positive value to begin fretting after the nut (e.g., for banjo or partial fret instruments).
        /// Set a negative value to add extra frets before the nut (e.g., extended fingerboard).
        /// </summary>
        public int? StartingFret { get; set; }

        /// <summary>
        /// The total number of frets for this string.
        /// If not set, the instrument's global number of frets is used.
        /// </summary>
        /// <remarks>
        /// Overrides <see cref="InstrumentLayoutConfiguration.NumberOfFrets"/> when specified.
        /// </remarks>
        public int? NumberOfFrets { get; set; }
    
        /// <summary>
        /// List of custom fret intervals for this string.
        /// </summary>
        public List<double>? Intervals { get; set; }

        /// <summary>
        /// Temperament used for fret placement on this string.
        /// </summary>
        /// <remarks>When set to <see cref="Temperament.Custom"/>, specify the intervals in <see cref="Intervals"/></remarks>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Temperament? Temperament { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>only meaningful when Temperament == Equal</remarks>
        public int? ETSteps { get; set; } = 12;
    }
}
