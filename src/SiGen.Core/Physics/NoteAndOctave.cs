using System.Text.Json;
using System.Text.RegularExpressions;

namespace SiGen.Physics
{
    public readonly record struct 
        NoteAndOctave(NoteName Note, int Octave, double CentOffset = 0)
    {
        public override string ToString() =>
            CentOffset == 0
                ? $"{Note}{Octave}"
                : $"{Note}{Octave} ({CentOffset:+0.##;-0.##} cents)";

        public string ToShortString() =>
            CentOffset == 0
                ? $"{Note}{Octave}"
                : $"{Note}{Octave}{CentOffset:+0.##;-0.##}";

        public string ToStringFormatted()
        {
            return Note.ToString().Replace('b', '♭') + Octave.ToString();
        }

        private static Regex NotePattern = new(@"^([A-Ga-g])([#b♯♭])?(\d+)?(([+-]\d+))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryParse(string s, out NoteAndOctave result)
        {
            var match = NotePattern.Match(s);
            result = default;
            if (!match.Success) return false;

            // Group 1: Note name
            var noteChar = match.Groups[1].Value.ToUpper();
            // Group 2: Accidental
            var accidental = match.Groups[2].Success ? match.Groups[2].Value : null;
            // Group 3: Octave
            var octaveStr = match.Groups[3].Success ? match.Groups[3].Value : null;
            // Group 4: Cents offset
            var centsStr = match.Groups[4].Success ? match.Groups[4].Value : null;

            // Map sharp accidental to flat NoteName
            NoteName noteName;
            try
            {
                if (accidental == "#" || accidental == "♯")
                {
                    noteName = noteChar switch
                    {
                        "C" => NoteName.Db,
                        "D" => NoteName.Eb,
                        "F" => NoteName.Gb,
                        "G" => NoteName.Ab,
                        "A" => NoteName.Bb,
                        _ => throw new JsonException($"No flat equivalent for {noteChar}#")
                    };
                }
                else if (accidental == "b" || accidental == "♭")
                {
                    noteName = noteChar switch
                    {
                        "D" => NoteName.Db,
                        "E" => NoteName.Eb,
                        "G" => NoteName.Gb,
                        "A" => NoteName.Ab,
                        "B" => NoteName.Bb,
                        _ => throw new JsonException($"No flat equivalent for {noteChar}b")
                    };
                }
                else
                {
                    noteName = noteChar switch
                    {
                        "C" => NoteName.C,
                        "D" => NoteName.D,
                        "E" => NoteName.E,
                        "F" => NoteName.F,
                        "G" => NoteName.G,
                        "A" => NoteName.A,
                        "B" => NoteName.B,
                        _ => throw new JsonException($"Invalid note name: {noteChar}")
                    };
                }
            }
            catch {
                return false;
            }

            int octave = octaveStr != null ? int.Parse(octaveStr) : 4;
            double centOffset = 0;
            if (centsStr != null && double.TryParse(centsStr, out var cents))
                centOffset = cents;

            result = new NoteAndOctave(noteName, octave, centOffset);
            return true;
        }
    
        public NoteAndOctave Transpose(int semitones)
        {
            int totalSemitones = (int)Note + (Octave * 12) + semitones;
            int newOctave = totalSemitones / 12;
            int newNoteOffset = totalSemitones % 12;
            if (newNoteOffset < 0)
            {
                newNoteOffset += 12;
                newOctave -= 1;
            }
            NoteName newNote = (NoteName)newNoteOffset;
            return new NoteAndOctave(newNote, newOctave, CentOffset);
        }

        public double ToAbsoluteCents() => (Octave * 1200) + ((int)Note * 100) + CentOffset;
    }
}
