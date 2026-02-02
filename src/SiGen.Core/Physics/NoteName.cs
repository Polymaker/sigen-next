using System.ComponentModel;

namespace SiGen.Physics
{
    public enum NoteName
    {
        C,
        /// <summary>
        /// C♯/D♭
        /// </summary>
        [Description("C♯/D♭")]
        Db,
        D,
        /// <summary>
        /// D♯/E♭
        /// </summary>
        [Description("D♯/E♭")]
        Eb,
        E,
        F,
        /// <summary>
        /// F♯/G♭
        /// </summary>
        [Description("F♯/G♭")]
        Gb,
        G,
        /// <summary>
        /// G♯/A♭
        /// </summary>
        [Description("G♯/A♭")]
        Ab,
        A,
        /// <summary>
        /// A♯/B♭
        /// </summary>
        [Description("A♯/B♭")]
        Bb,
        B
    }
}
