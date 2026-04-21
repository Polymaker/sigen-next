namespace SiGen.Serialization.Layouts
{
    /// <summary>
    /// Represents the file format of a layout file.
    /// </summary>
    public enum FileFormat
    {
        /// <summary>
        /// Unknown or unsupported format.
        /// </summary>
        Unknown,

        /// <summary>
        /// XML format (legacy, versions 1-2).
        /// </summary>
        Xml,

        /// <summary>
        /// JSON format (current, version 3+).
        /// </summary>
        Json
    }
}
