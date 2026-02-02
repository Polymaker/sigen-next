using SiGen.Layouts.Data;
using SiGen.Measuring;
using SiGen.Physics;
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

        public abstract Measure GetTotalWidth2(bool includeGauge);

        public void SetGauge(Measure? gauge)
        {
            if (this is SingleStringConfiguration ssc)
                ssc.Gauge = gauge;
            else if (this is StringGroupConfiguration sgc)
            {
                foreach (var str in sgc.Strings)
                    str.Gauge = gauge;
            }
        }

        public void SetTuning(NoteAndOctave? tuning)
        {
            if (this is SingleStringConfiguration ssc)
                ssc.Tuning = tuning;
            else if (this is StringGroupConfiguration sgc)
            {
                foreach (var str in sgc.Strings)
                    str.Tuning = tuning;
            }
        }

        public abstract Measure GetHalfWidth(FingerboardSide side, bool includeGauge);
    }
}
