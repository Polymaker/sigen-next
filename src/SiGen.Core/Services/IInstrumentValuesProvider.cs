using SiGen.Data.Presets;

namespace SiGen.Services
{
    public interface IInstrumentValuesProvider
    {
        int StandardStringCount { get; }
        IReadOnlyList<int> GetCommonStringCounts();

        IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets();
        IReadOnlyList<TuningPreset> GetTuningPresets();
        IReadOnlyList<SpacingPreset> GetNutSpacingPresets();
        IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets();
        IReadOnlyList<SpacingPreset> GetMarginPresets();

    }
}
