using SiGen.Measuring;
using System.Text.Json.Serialization;

namespace SiGen.Layouts.Configuration
{
    public abstract class BaseStringConfiguration
    {
        /// <summary>
        /// The length of the string or group of strings from the nut to the bridge.
        /// </summary>
        /// <remarks>
        /// Only used when the scale length mode is set to <see cref="ScaleLengthMode.PerString"/>.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Measure? ScaleLength { get; set; }
        /// <summary>
        /// The aligment ratio for multi-scale instruments.
        /// </summary>
        /// <remarks>
        /// Only used when the scale length mode is set to <see cref="ScaleLengthMode.PerString"/>.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MultiScaleRatio { get; set; }

        /// <summary>
        /// Individual fret configuration for the string or group of strings.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FretConfiguration? Frets { get; set; } /*= null!;*/

        /// <summary>
        /// Returns the total width of the string or group of strings taking into account the string(s) gauge.
        /// </summary>
        /// <returns></returns>
        public abstract Measure? GetTotalWidth();
    }
}
