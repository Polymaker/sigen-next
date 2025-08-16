using SiGen.Physics;

namespace SiGen.Data.Presets
{
    public class TuningPreset
    {
        public string Name { get; set; } = string.Empty;
        public int NumberOfStrings => Tunings.Length;//{ get; set; }
        public PitchInterval[] Tunings { get; set; } = new PitchInterval[0];

        public TuningPreset(string name, PitchInterval[] tunings)
        {
            Name = name;
            Tunings = tunings;
        }
    }
}
