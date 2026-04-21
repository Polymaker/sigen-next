using SiGen.Measuring;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SiGen.Settings;

public class UserSettings
{
    /// <summary>
    /// Gets or sets the list of recently opened files.
    /// </summary>
    public List<RecentFileModel> RecentFiles { get; set; } = new();

    /// <summary>
    /// Gets or sets the application theme preference.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AppTheme Theme { get; set; } = AppTheme.System;

    /// <summary>
    /// Gets or sets the preferred language.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AppLanguage Language { get; set; } = AppLanguage.System;

    /// <summary>
    /// Gets or sets the preferred measurement unit system.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnitSystem PreferredUnits { get; set; } = UnitSystem.Metric;

    /// <summary>
    /// Gets or sets the layout viewer color scheme settings.
    /// </summary>
    public LayoutViewerColorSchemeSettings LayoutViewerColorScheme { get; set; } = new();
}

