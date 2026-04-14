namespace SiGen.Data.Common
{
    /// <summary>
    /// A flattened representation of a string's position and data.
    /// </summary>
    /// <param name="OverallIndex">The overall index of the string (0 to TotalStrings - 1)</param>
    /// <param name="CourseIndex">The index of the course this string belongs to (0 to NumberOfStrings - 1)</param>
    /// <param name="SubIndex">The index of the string within its course (0-based), or null if it's a single string</param>
    /// <typeparam name="T">The type of data being carried (e.g., StringProperties, PhysicsResult, etc.)</typeparam>
    public record StringContext<T>(
        int OverallIndex,    // 0 to TotalStrings - 1
        int CourseIndex,     // The "BaseStringConfiguration" index (e.g., 0-5 for a 12-string)
        int? SubIndex,       // null for single strings, 0-1 for a 12-string course
        T Data               // The payload (usually your StringProperties)
    )
    {
        public StringIndex Index => new(OverallIndex, CourseIndex, SubIndex);
        public bool IsInCourse => SubIndex.HasValue;
    }

    /// <param name="OverallIndex">The overall index of the string (0 to TotalStrings - 1)</param>
    /// <param name="CourseIndex">The index of the course this string belongs to (0 to NumberOfStrings - 1)</param>
    /// <param name="SubIndex">The index of the string within its course (0-based), or null if it's a single string</param>
    public record StringIndex(
        int OverallIndex,
        int CourseIndex,
        int? SubIndex
    );


}
 