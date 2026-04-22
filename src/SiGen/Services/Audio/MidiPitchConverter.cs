using SiGen.Physics;
using System;

namespace SiGen.Services.Audio
{
    /// <summary>
    /// Represents a MIDI note with microtonal pitch bend adjustment.
    /// </summary>
    public readonly struct MidiNoteWithBend
    {
        /// <summary>
        /// The base MIDI note number (0-127), rounded to nearest 12TET semitone.
        /// </summary>
        public int MidiNote { get; }

        /// <summary>
        /// Pitch bend offset in cents (typically ±200 cents for standard MIDI pitch bend range).
        /// </summary>
        public double PitchBendCents { get; }

        public MidiNoteWithBend(int midiNote, double pitchBendCents)
        {
            MidiNote = midiNote;
            PitchBendCents = pitchBendCents;
        }

        public override string ToString() => $"MIDI {MidiNote} {PitchBendCents:+0.0;-0.0} cents";
    }

    /// <summary>
    /// Utility for converting pitch intervals to MIDI note + pitch bend.
    /// </summary>
    public static class MidiPitchConverter
    {
        /// <summary>
        /// Converts a pitch interval (in cents from C0) to MIDI note number + pitch bend offset.
        /// </summary>
        /// <param name="cents">Pitch in cents, where 0 = C0, 100 = C#0, 1200 = C1, etc.</param>
        /// <returns>MIDI note (nearest 12TET semitone) and pitch bend offset in cents.</returns>
        public static MidiNoteWithBend FromCents(double cents)
        {
            // Calculate the exact semitone value (0 = C0, 1 = C#0, etc.)
            double exactSemitone = (cents / 100.0) + 12;

            // Round to nearest integer semitone for base MIDI note
            int baseSemitone = (int)Math.Round(exactSemitone);

            // Calculate the pitch bend offset (difference from rounded semitone)
            double pitchBendCents = (exactSemitone - baseSemitone) * 100.0;

            // Clamp MIDI note to valid range (0-127)
            int midiNote = Math.Clamp(baseSemitone, 0, 127);

            return new MidiNoteWithBend(midiNote, pitchBendCents);
        }

        /// <summary>
        /// Converts a PitchInterval to MIDI note number + pitch bend offset.
        /// </summary>
        /// <param name="interval">Pitch interval from your physics system.</param>
        /// <returns>MIDI note (nearest 12TET semitone) and pitch bend offset in cents.</returns>
        public static MidiNoteWithBend FromPitchInterval(PitchInterval interval)
        {
            return FromCents(interval.Cents);
        }

        /// <summary>
        /// Converts a PitchInterval relative to a base pitch to MIDI note + pitch bend.
        /// </summary>
        /// <param name="basePitch">The reference pitch (e.g., open string tuning).</param>
        /// <param name="interval">The interval to add to the base pitch.</param>
        /// <returns>MIDI note and pitch bend offset.</returns>
        public static MidiNoteWithBend FromPitchInterval(PitchInterval basePitch, PitchInterval interval)
        {
            return FromCents(basePitch.Cents + interval.Cents);
        }

        /// <summary>
        /// Converts MIDI pitch bend cents to a 14-bit MIDI pitch bend value.
        /// Standard MIDI pitch bend range is ±2 semitones (±200 cents).
        /// </summary>
        /// <param name="cents">Pitch bend in cents (typically -200 to +200).</param>
        /// <param name="bendRangeCents">Maximum pitch bend range in cents. Default is 200 (±2 semitones).</param>
        /// <returns>MIDI pitch bend value (0-16383, where 8192 = no bend).</returns>
        public static int CentsToMidiPitchBend(double cents, double bendRangeCents = 200.0)
        {
            // Clamp to valid range
            cents = Math.Clamp(cents, -bendRangeCents, bendRangeCents);

            // Convert to 14-bit MIDI value (0-16383, center = 8192)
            int pitchBendValue = (int)(8192 + (cents / bendRangeCents) * 8192);
            return Math.Clamp(pitchBendValue, 0, 16383);
        }
    }
}
