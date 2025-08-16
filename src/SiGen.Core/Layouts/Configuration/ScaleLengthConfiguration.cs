using SiGen.Layouts.Data;
using SiGen.Measuring;
using System.Text.Json.Serialization;

namespace SiGen.Layouts.Configuration
{
    public class ScaleLengthConfiguration
    {
        /// <summary>
        /// Specifies the method used to apply the scale length.
        /// Determines whether the scale is measured along the fingerboard center, along each string, or selected automatically.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScaleLengthCalculationMethod CalculationMethod { get; set; }

        /// <summary>
        /// Defines the scale length layout mode. <br/>
        /// Single: all strings use the same scale length. <br/>
        /// Multiscale: separate scale lengths for bass and treble strings. <br/>
        /// PerString: each string can have an individual scale length.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScaleLengthMode Mode { get; set; }

        /// <summary>
        /// The scale length value for all strings when <see cref="Mode"/> is set to <see cref="ScaleLengthMode.Single"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Measure? SingleScale { get; set; }

        /// <summary>
        /// The scale length value for treble strings when <see cref="Mode"/> is set to <see cref="ScaleLengthMode.Multiscale"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Measure? TrebleScale { get; set; }

        /// <summary>
        /// The scale length value for bass strings when <see cref="Mode"/> is set to <see cref="ScaleLengthMode.PerString"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Measure? BassScale { get; set; }

        /// <summary>
        /// The alignment ratio used for multiscale layouts.
        /// Controls the position of the neutral fret or the angle of the fanned frets.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MultiScaleRatio { get; set; }

        /// <summary>
        /// The skew amount to apply between bass and treble strings.
        /// Allows for fanned frets even with a single scale length.
        /// Can be positive or negative.
        /// </summary>
        public Measure? BassTrebleSkew { get; set; }
    }
}
