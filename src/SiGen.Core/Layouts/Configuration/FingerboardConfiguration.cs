using SiGen.Layouts.Data;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SiGen.Layouts.Configuration
{
    public enum MarginMode
    {
        All,           // All margins are the same
        NutBridge,     // Nut and Bridge margins differ, but each side is the same
        BassTreble,    // Bass and Treble margins differ, but each end is the same
        Individual     // All margins are set individually
    }

    public class FingerboardConfiguration
    {
        /// <summary>
        /// Indicates whether the margins should be compensated for string widths.
        /// </summary>
        public bool CompensateMarginsForStrings { get; set; }

        /// <summary>
        /// Length to continue the fingerboard after the last fret.
        /// </summary>
        public Measure? ExtensionAfterLastFret { get; set; }

        public Measure? NutTrebleMargin { get; set; }
        public Measure? NutBassMargin { get; set; }
        public Measure? BridgeTrebleMargin { get; set; }
        public Measure? BridgeBassMargin { get; set; }

        public Measure GetMargin(FingerboardEnd end, FingerboardSide side)
        {
            return (end, side) switch
            {
                (FingerboardEnd.Nut, FingerboardSide.Treble) => NutTrebleMargin ?? Measure.Zero,
                (FingerboardEnd.Nut, FingerboardSide.Bass) => NutBassMargin ?? Measure.Zero,
                (FingerboardEnd.Bridge, FingerboardSide.Treble) => BridgeTrebleMargin ?? Measure.Zero,
                (FingerboardEnd.Bridge, FingerboardSide.Bass) => BridgeBassMargin ?? Measure.Zero,
                _ => throw new ArgumentOutOfRangeException(nameof(end), "Invalid combination of end and side.")
            };
        }

        public void SetAllMargins(Measure? measure)
        {
            NutBassMargin = measure;
            NutTrebleMargin = measure;
            BridgeBassMargin = measure;
            BridgeTrebleMargin = measure;
        }

        public void SetNutAndBridgeMargins(FingerboardEnd end, Measure? measure)
        {
            if (end == FingerboardEnd.Nut)
            {
                NutBassMargin = measure;
                NutTrebleMargin = measure;
            }
            else
            {
                BridgeBassMargin = measure;
                BridgeTrebleMargin = measure;
            }
        }

        public void SetBassAndTrebleMargins(FingerboardSide side, Measure? measure)
        {
            if (side == FingerboardSide.Bass)
            {
                NutBassMargin = measure;
                BridgeBassMargin = measure;
            }
            else
            {
                NutTrebleMargin = measure;
                BridgeTrebleMargin = measure;
            }
        }

        [JsonIgnore]
        public MarginMode MarginDefinitionMode
        {
            get
            {
                var nt = NutTrebleMargin ?? Measure.Zero;
                var nb = NutBassMargin ?? Measure.Zero;
                var bt = BridgeTrebleMargin ?? Measure.Zero;
                var bb = BridgeBassMargin ?? Measure.Zero;

                if (nt == nb && nt == bt && nt == bb)
                    return MarginMode.All;
                if (nt == nb && bt == bb)
                    return MarginMode.NutBridge;
                if (nt == bt && nb == bb)
                    return MarginMode.BassTreble;
                return MarginMode.Individual;
            }
        }
    }
}
