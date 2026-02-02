namespace SiGen.Layouts.Configuration
{
    public enum ScaleLengthMode
    {
        /// <summary>
        /// A single scale length for all strings.
        /// </summary>
        Single,
        /// <summary>
        /// A separate scale length for treble and bass strings.
        /// </summary>
        Multiscale,
        /// <summary>
        /// A separate scale length for each string.
        /// </summary>
        PerString
    }
}
