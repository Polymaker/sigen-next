using SiGen.Layouts.Data;
using SiGen.Measuring;

namespace SiGen.Layouts.Configuration
{
    public class MarginConfiguration
    {
        public Measure? NutTreble { get; set; }
        public Measure? NutBass { get; set; }
        public Measure? BridgeTreble { get; set; }
        public Measure? BridgeBass { get; set; }

        /// <summary>
        /// Indicates whether the margins should be compensated for string widths.
        /// </summary>
        public bool CompensateForStrings { get; set; }

        public Measure GetMargin(FingerboardEnd end, FingerboardSide side)
        {
            return (end, side) switch
            {
                (FingerboardEnd.Nut, FingerboardSide.Treble) => NutTreble ?? Measure.Zero,
                (FingerboardEnd.Nut, FingerboardSide.Bass) => NutBass ?? Measure.Zero,
                (FingerboardEnd.Bridge, FingerboardSide.Treble) => BridgeTreble ?? Measure.Zero,
                (FingerboardEnd.Bridge, FingerboardSide.Bass) => BridgeBass ?? Measure.Zero,
                _ => throw new ArgumentOutOfRangeException(nameof(end), "Invalid combination of end and side.")
            };
        }

        public void SetAll(Measure? measure)
        {
            NutBass = measure;
            NutTreble = measure;
            BridgeBass = measure;
            BridgeTreble = measure;
        }
    }
}
