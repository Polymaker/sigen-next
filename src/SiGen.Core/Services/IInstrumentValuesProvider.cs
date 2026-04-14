using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;

namespace SiGen.Services
{
    public interface IInstrumentValuesProvider
    {
        InstrumentType InstrumentType { get; }
        int StandardStringCount { get; }
        IReadOnlyList<int> GetCommonStringsCount();
        IReadOnlyList<int> GetCommonFretsCount();
        IReadOnlyList<ScaleLengthPreset> GetScaleLengthPresets();
        IReadOnlyList<InstrumentTuningPreset> GetTuningPresets();
        IReadOnlyList<SpacingPreset> GetNutSpacingPresets();
        IReadOnlyList<SpacingPreset> GetBridgeSpacingPresets();
        IReadOnlyList<SpacingPreset> GetMarginPresets();

        InstrumentLayoutConfiguration GetDefaultConfiguration();

        IReadOnlyList<LayoutTemplate> GetLayoutTemplates();
    }
}
