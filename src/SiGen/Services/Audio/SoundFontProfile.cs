using SiGen.Data.Common;
using System.Collections.Generic;
using System.IO;

namespace SiGen.Services.Audio
{
    /// <summary>
    /// Describes a SoundFont file and the conditions under which it should be selected for playback.
    /// </summary>
    public class SoundFontProfile
    {
        public string Name { get; init; } = "";
        public string FilePath { get; init; } = "";

        /// <summary>Instrument types this profile is preferred for, in order of priority.</summary>
        public IReadOnlyList<InstrumentType> PreferredInstruments { get; init; } = [];

        /// <summary>Minimum string gauge in mm this profile is suited for. 0 means no minimum.</summary>
        public double MinGaugeMm { get; init; } = 0;

        /// <summary>Maximum string gauge in mm this profile is suited for.</summary>
        public double MaxGaugeMm { get; init; } = double.MaxValue;

        public bool IsAvailable => File.Exists(FilePath);
    }
}
