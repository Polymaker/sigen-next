using SiGen.Data.Common;
using SiGen.Data.Presets;

namespace SiGen.Services
{
    public interface IInstrumentValuesProvider
    {
        InstrumentType InstrumentType { get; }
        int StandardStringCount { get; }
        IReadOnlyList<int> GetCommonStringsCount();
        IReadOnlyList<int> GetCommonFretsCount();
        IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets();
        IReadOnlyList<TuningPreset> GetTuningPresets();
        IReadOnlyList<SpacingPreset> GetNutSpacingPresets();
        IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets();
        IReadOnlyList<SpacingPreset> GetMarginPresets();

    }
}
