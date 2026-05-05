using SiGen.Data.Common;
using SiGen.Layouts.Data;
using SiGen.Measuring;
using SiGen.Physics;
using System.Text.Json.Serialization;

namespace SiGen.Layouts.Configuration
{
    public abstract class BaseStringConfiguration
    {
        private IStringConfigurationCollection? _Owner;

        //[JsonIgnore]
        //public SiGen.Data.Common.StringIndex Position { get; internal set; } = new(0, 0, null);

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
        public StringFretConfiguration? Frets { get; set; } /*= null!;*/

        [JsonIgnore]
        public abstract bool IsStringCourse { get; }

        [JsonIgnore]
        public abstract int NumberOfStrings { get; }

        internal void AssignOwner(IStringConfigurationCollection? owner)
        {
            _Owner = owner;
        }

        /// <summary>
        /// Returns the total width of the string or group of strings taking into account the string(s) gauge.
        /// </summary>
        /// <returns></returns>
        public abstract Measure? GetTotalWidth();

        /// <summary>
        /// Calculates the span of the string, optionally including the gauge measurement.
        /// </summary>
        /// <param name="includeGauge">true to include the gauge in the span calculation; otherwise, false.</param>
        /// <returns>A Measure representing the calculated span of the string. The value reflects whether the gauge was included
        /// based on the parameter.</returns>
        public abstract Measure GetStringSpan(bool includeGauge);

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

        public NoteAndOctave? GetPrimaryNote()
        {
            if (this is SingleStringConfiguration ssc)
                return ssc.Tuning;
            else if (this is StringGroupConfiguration sgc)
            {
                return sgc.Strings.Where(x=>x.Tuning.HasValue)
                    .Select(x=>x.Tuning).FirstOrDefault();
            }
            return null;
        }

        #region Helpers

        public void SetStartingFret(int fretNumber)
        {
            Frets ??= new StringFretConfiguration();
            Frets.StartingFret = fretNumber;
        }

        public void SetNumberOfFrets(int fretCount)
        {
            Frets ??= new StringFretConfiguration();
            Frets.NumberOfFrets = fretCount;
        }

        #endregion

        [JsonIgnore]
        public bool IsGaugeDefined => EnumerateStrings().All(x => x.Data.Gauge.HasValue);

        /// <summary>
        /// Returns half the width of the string or group of strings, measured from the center outward to the specified fingerboard side.
        /// </summary>
        /// <remarks>
        /// For a single string, returns zero or half the string gauge (when <paramref name="includeGauge"/> is true). <br/>
        /// For a string course (group of strings), returns half the span of the strings plus half the gauge of the outermost string 
        /// on the specified fingerboard side (when <paramref name="includeGauge"/> is true).
        /// </remarks>
        /// <param name="side">The fingerboard side to measure towards.</param>
        /// <param name="includeGauge">true to include the gauge measurement in the calculation; otherwise, false.</param>
        /// <returns>A Measure representing half the width from center to the specified side.</returns>
        public abstract Measure GetHalfWidth(FingerboardSide side, bool includeGauge);
    
        public IEnumerable<StringContext<StringProperties>> EnumerateStrings()
        {
            var baseIndex = _Owner?.GetIndex(this) ?? new StringIndex(0, 0, null);
            if (this is SingleStringConfiguration ssc)
            {
                yield return new StringContext<StringProperties>(baseIndex.OverallIndex, baseIndex.CourseIndex, null, ssc.Properties ?? new StringProperties());
            }
            else if (this is StringGroupConfiguration sgc)
            {
                for (int i = 0; i < sgc.Strings.Count; i++)
                {
                    yield return new StringContext<StringProperties>(baseIndex.OverallIndex + i, baseIndex.CourseIndex, i, sgc.Strings[i] ?? new StringProperties());
                }
            }
        }

        public StringIndex GetIndex() => _Owner?.GetIndex(this) ?? new StringIndex(0, 0, null);
    }
}
