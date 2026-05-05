using SiGen.Physics;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SiGen.Layouts.Configuration
{
    /// <summary>
    /// Base fret configuration shared by both per-string and global configurations:
    /// temperament, number of frets and custom intervals.
    /// </summary>
    public abstract class FretConfigurationBase
    {
        /// <summary>
        /// The total number of frets.
        /// When set on a string, overrides <see cref="InstrumentLayoutConfiguration.NumberOfFrets"/>.
        /// </summary>
        public int? NumberOfFrets { get; set; }

        /// <summary>
        /// List of custom fret intervals.
        /// </summary>
        public List<double>? Intervals { get; set; }

        /// <summary>
        /// Temperament used for fret placement.
        /// </summary>
        /// <remarks>When set to <see cref="Temperament.Custom"/>, specify the intervals in <see cref="Intervals"/>.</remarks>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Temperament? Temperament { get; set; }

        /// <summary>
        /// Number of equal temperament divisions per octave.
        /// </summary>
        /// <remarks>Only meaningful when Temperament == Equal.</remarks>
        public int? ETSteps { get; set; } = 12;
    }

    /// <summary>
    /// Per-string fret configuration. Adds <see cref="StartingFret"/> on top of the shared base.
    /// </summary>
    public class StringFretConfiguration : FretConfigurationBase
    {
        /// <summary>
        /// Specifies the starting fret index for this string.
        /// Set a positive value to begin fretting after the nut (e.g., for banjo or partial fret instruments).
        /// Set a negative value to add extra frets before the nut (e.g., extended fingerboard).
        /// </summary>
        public int? StartingFret { get; set; }
    }

    /// <summary>
    /// Global fret configuration for the instrument. Adds <see cref="ShapeStyle"/> on top of the shared base.
    /// </summary>
    public class GlobalFretConfiguration : FretConfigurationBase
    {
        /// <summary>
        /// Controls how non-straight fret slots are shaped.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter)), DefaultValue(FretShapeStyle.Polyline)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public FretShapeStyle ShapeStyle { get; set; } = FretShapeStyle.Polyline;
    }

    public enum FretShapeStyle
    {
        /// <summary>Fret shape is approximated as a series of straight segments.</summary>
        Polyline,
        /// <summary>Fret shape is a smooth Bézier spline with handles tangent to the overall curve.</summary>
        SmoothSpline,
        /// <summary>
        /// Like SmoothSpline but with a small flat section under each string,
        /// suitable for True Temperament-style frets.
        /// </summary>
        NotchedSpline,
    }
}
