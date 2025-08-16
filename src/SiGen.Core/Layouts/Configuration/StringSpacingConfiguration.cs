using SiGen.Layouts.Data;
using SiGen.Measuring;
using System.Text.Json.Serialization;

namespace SiGen.Layouts.Configuration
{
    public class StringSpacingConfiguration
    {
        /// <summary>
        /// Specifies the method used to calculate string placement.
        /// Proportional mode ensures equal free space between strings (accounts for string gauge).
        /// CenterToCenter mode sets equal distance from the center of one string to the next.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StringSpacingMode SpacingMode { get; set; }

        /// <summary>
        /// Determines how the string set is centered on the fingerboard.
        /// Options include centering by outer strings, middle strings, fingerboard center, or symmetric alignment.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LayoutCenterAlignment CenterAlignment { get; set; }

        /// <summary>
        /// Indicates whether the current center alignment mode is symmetric.
        /// Returns true for symmetric string or fingerboard alignment.
        /// </summary>
        [JsonIgnore]
        public bool IsSymmetric => CenterAlignment == LayoutCenterAlignment.SymmetricStrings || 
            CenterAlignment == LayoutCenterAlignment.SymmetricFingerboard;

        /// <summary>
        /// Alignment ratio (0 to 1) used to align this end (nut or bridge) to the other.
        /// </summary>
        /// <remarks>Only relevant for manual alignment modes.</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? AlignmentRatio { get; set; }

        /// <summary>
        /// Specifies the distances between strings. <br/>
        /// - If a single value is provided, it is used as the uniform spacing for all string gaps.<br/>
        /// - If multiple values are provided, each value defines the spacing for the corresponding gap between adjacent strings.<br/>
        /// For proportional mode, the spacing is adjusted to maintain equal free space between strings, accounting for string gauge.
        /// </summary>
        public List<Measure> StringDistances { get; set; } = null!;

        public StringSpacingConfiguration()
        {
            StringDistances = new List<Measure>();
        }

        //public StringSpacingMode NutSpacingMode { get; set; }
        //public LayoutCenterAlignment NutCenterAlignment { get; set; }
        //public List<Measure>? NutDistances { get; set; }

        //public StringSpacingMode BridgeSpacingMode { get; set; }
        //public LayoutCenterAlignment BridgeCenterAlignment { get; set; }
        //public List<Measure>? BridgeDistances { get; set; }
    }
}
