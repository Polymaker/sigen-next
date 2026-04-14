using SiGen.Layouts.Configuration;
using SiGen.Physics;

namespace SiGen.Data.Presets
{
    public class InstrumentTuningPreset
    {
        public string Name { get; set; } = string.Empty;
        public int NumberOfStrings => Courses.Length;//{ get; set; }

        // This represents the playable groups (e.g., 6 for guitar, 4 for mandolin)
        public TuningCourse[] Courses { get; set; } = Array.Empty<TuningCourse>();

        // Total physical strings (e.g., 8 for mandolin)
        public int TotalStringCount => Courses.Sum(c => c.Strings.Length);

        public int CourseCount => Courses.Length;


        public InstrumentTuningPreset(string name, NoteAndOctave[] tunings)
        {
            Name = name;
            Courses = tunings.Select(x => TuningCourse.Single(x)).ToArray();
        }

        public InstrumentTuningPreset(string name, TuningCourse[] courses)
        {
            Name = name;
            Courses = courses;
        }

        /// <summary>
        /// Returns true if the given layout configuration is compatible with this tuning preset.
        /// </summary>
        /// <param name="layoutConfiguration"></param>
        /// <returns></returns>
        public bool IsCompatible(InstrumentLayoutConfiguration layoutConfiguration)
        {
            if (layoutConfiguration.NumberOfStrings != CourseCount)
                return false;

            for (int i = 0; i < CourseCount; i++)
            {
                //allow single strings tuning to match any configuration, will set all strings in the course to the same note
                if (Courses[i].NumberOfStrings == 1) continue;

                var strConfig = layoutConfiguration.StringConfigurations[i];

                //if the configuration string is single but the tuning course is grouped, no match  
                if (!strConfig.IsStringCourse) return false;

                int numStringsInCourse = ((StringGroupConfiguration)strConfig).NumberOfStrings;
                if (numStringsInCourse != Courses[i].NumberOfStrings)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the given layout configuration has the exact same tuning as this preset.
        /// </summary>
        /// <param name="layoutConfiguration"></param>
        /// <returns></returns>
        public bool HasSameTuning(InstrumentLayoutConfiguration layoutConfiguration)
        {
            if (layoutConfiguration.NumberOfStrings != CourseCount)
                return false;

            for (int i = 0; i < CourseCount; i++)
            {
                var strConfig = layoutConfiguration.StringConfigurations[i];

                //if the tuning course has only one note, it is either for a single string or a string course tuned to the same note
                if (Courses[i].NumberOfStrings == 1)
                {
                    //if the configuration string is single, check for exact match
                    if (strConfig is SingleStringConfiguration single && single.Tuning != Courses[i].PrimaryNote) return false;
                    //if the configuration string is grouped, check that all strings match the tuning
                    if (strConfig is StringGroupConfiguration group && !group.Strings.All(x => x.Tuning == Courses[i].PrimaryNote)) return false;
                }

                //if the configuration string is single but the tuning course is grouped, no match  
                if (!strConfig.IsStringCourse) return false;

                int numStringsInCourse = ((StringGroupConfiguration)strConfig).NumberOfStrings;

                if (numStringsInCourse != Courses[i].NumberOfStrings)
                    return false;

                for (int j = 0; j < numStringsInCourse; j++)
                {
                    var courseNote = Courses[i].Strings[j];
                    var configNote = ((StringGroupConfiguration)strConfig).Strings[j].Tuning;
                    if (configNote != courseNote)
                        return false;
                }
            }
            return true;
        }
    }

    public record TuningCourse(NoteAndOctave[] Strings)
    {
        // Helper to get the primary pitch (usually the first string in the group)
        public NoteAndOctave PrimaryNote => Strings[0];

        // Check if it's a grouped course (12-string, Mandolin, etc.)
        public bool IsGrouped => Strings.Length > 1;

        public int NumberOfStrings => Strings.Length;

        // Factory method for standard single strings
        public static TuningCourse Single(NoteAndOctave note)
            => new TuningCourse([note]);

        public static TuningCourse Single(NoteName note, int octave)
            => new TuningCourse([new NoteAndOctave(note, octave)]);

        public override string ToString()
        {
            if (IsGrouped)
                return string.Join("|", Strings);
            return Strings[0].ToStringFormatted();
        }
    }
}
