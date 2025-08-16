namespace SiGen.Physics
{
    public readonly record struct NoteAndOctave(NoteName Note, int Octave, double CentOffset = 0)
    {
        public override string ToString() =>
            CentOffset == 0
                ? $"{Note}{Octave}"
                : $"{Note}{Octave} ({CentOffset:+0.##;-0.##} cents)";
    }
}
