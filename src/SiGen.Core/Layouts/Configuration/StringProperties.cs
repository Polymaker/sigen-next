using SiGen.Measuring;
using SiGen.Physics;
using System.Text.Json.Serialization;

namespace SiGen.Layouts.Configuration
{
    public class StringProperties
    {
        /// <summary>
        /// Gets or sets the gauge of the string.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Measure? Gauge { get; set; }

        /// <summary>
        /// The tuning of the string.
        /// </summary>
        /// <remarks>
        /// Optional. Only used for fret compensation calculation.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PitchInterval? Tuning { get; set; }

        //not used yet, will be used to grab material information from DB if not present in the configuration
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? InstrumentStringId { get; set; }

        /// <summary>
        /// Gets or sets information about the string material.
        /// </summary>
        /// <remarks>Optional. Only used for display purpose or fret compensation calculation</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public StringMaterialConfiguration? Material { get; set; }
    }
}
