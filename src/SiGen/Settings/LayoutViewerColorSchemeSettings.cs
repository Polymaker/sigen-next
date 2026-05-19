using SiGen.UI.LayoutViewer;
using System.Text.Json.Serialization;

namespace SiGen.Settings;

/// <summary>
/// Represents a preset layout viewer theme.
/// </summary>
public enum LayoutViewerPreset
{
    /// <summary>
    /// Standard light mode theme.
    /// </summary>
    Light,

    /// <summary>
    /// Standard dark mode theme.
    /// </summary>
    Dark,

    /// <summary>
    /// Blueprint style theme.
    /// </summary>
    Blueprint,

    /// <summary>
    /// Custom user-defined colors.
    /// </summary>
    Custom
}

/// <summary>
/// User settings for the layout viewer color scheme.
/// </summary>
public class LayoutViewerColorSchemeSettings
{
    /// <summary>
    /// Gets or sets the selected preset theme.
    /// When set to <see cref="LayoutViewerPreset.Custom"/>, 
    /// the custom color values are used.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LayoutViewerPreset Preset { get; set; } = LayoutViewerPreset.Blueprint;

    /// <summary>
    /// Gets or sets the custom background color (hex format, e.g., "#FFFFFF").
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the custom grid color (hex format).
    /// </summary>
    public string? GridColor { get; set; }

    /// <summary>
    /// Gets or sets the custom major axis color (hex format).
    /// </summary>
    public string? MajorAxisColor { get; set; }

    /// <summary>
    /// Gets or sets the custom overlay text color (hex format).
    /// </summary>
    public string? OverlayTextColor { get; set; }

    /// <summary>
    /// Gets or sets the custom string color (hex format).
    /// </summary>
    public string? StringColor { get; set; }

    /// <summary>
    /// Gets or sets the custom fret color (hex format).
    /// </summary>
    public string? FretColor { get; set; }

    /// <summary>
    /// Gets or sets the custom fingerboard edge color (hex format).
    /// </summary>
    public string? FingerBoardEdgeColor { get; set; }

    /// <summary>
    /// Gets or sets the custom nut color (hex format).
    /// </summary>
    public string? NutColor { get; set; }

    /// <summary>
    /// Gets or sets the custom bridge color (hex format).
    /// </summary>
    public string? BridgeColor { get; set; }

    /// <summary>
    /// Gets or sets the custom guide line color (hex format).
    /// </summary>
    public string? GuideLineColor { get; set; }

    /// <summary>
    /// Creates a <see cref="LayoutViewerColorScheme"/> instance based on the current settings.
    /// </summary>
    /// <returns>A configured <see cref="LayoutViewerColorScheme"/> instance.</returns>
    public LayoutViewerColorScheme ToColorScheme()
    {
        return Preset switch
        {
            LayoutViewerPreset.Light => LayoutViewerColorScheme.LightMode,
            LayoutViewerPreset.Dark => LayoutViewerColorScheme.DarkMode,
            LayoutViewerPreset.Blueprint => LayoutViewerColorScheme.Blueprint,
            LayoutViewerPreset.Custom => CreateCustomRenderSettings(),
            _ => LayoutViewerColorScheme.LightMode
        };
    }

    private LayoutViewerColorScheme CreateCustomRenderSettings()
    {
        var settings = new LayoutViewerColorScheme();

        if (!string.IsNullOrEmpty(BackgroundColor))
            settings.BackgroundColor = Avalonia.Media.Color.Parse(BackgroundColor);

        if (!string.IsNullOrEmpty(GridColor))
            settings.GridColor = Avalonia.Media.Color.Parse(GridColor);

        if (!string.IsNullOrEmpty(MajorAxisColor))
            settings.MajorAxisColor = Avalonia.Media.Color.Parse(MajorAxisColor);

        if (!string.IsNullOrEmpty(OverlayTextColor))
            settings.OverlayTextColor = Avalonia.Media.Color.Parse(OverlayTextColor);

        if (!string.IsNullOrEmpty(StringColor))
            settings.StringColor = Avalonia.Media.Color.Parse(StringColor);

        if (!string.IsNullOrEmpty(FretColor))
            settings.FretColor = Avalonia.Media.Color.Parse(FretColor);

        if (!string.IsNullOrEmpty(FingerBoardEdgeColor))
            settings.FingerBoardEdgeColor = Avalonia.Media.Color.Parse(FingerBoardEdgeColor);

        if (!string.IsNullOrEmpty(NutColor))
            settings.NutColor = Avalonia.Media.Color.Parse(NutColor);

        if (!string.IsNullOrEmpty(BridgeColor))
            settings.BridgeColor = Avalonia.Media.Color.Parse(BridgeColor);

        if (!string.IsNullOrEmpty(GuideLineColor))
            settings.GuideLineColor = Avalonia.Media.Color.Parse(GuideLineColor);

        return settings;
    }

    /// <summary>
    /// Populates the custom color values from a <see cref="LayoutViewerColorScheme"/> instance.
    /// </summary>
    /// <param name="renderSettings">The render settings to copy from.</param>
    public void ApplyFromRenderSettings(LayoutViewerColorScheme renderSettings)
    {
        BackgroundColor = renderSettings.BackgroundColor.ToString();
        GridColor = renderSettings.GridColor.ToString();
        MajorAxisColor = renderSettings.MajorAxisColor.ToString();
        OverlayTextColor = renderSettings.OverlayTextColor.ToString();
        StringColor = renderSettings.StringColor.ToString();
        FretColor = renderSettings.FretColor.ToString();
        FingerBoardEdgeColor = renderSettings.FingerBoardEdgeColor.ToString();
        NutColor = renderSettings.NutColor.ToString();
        BridgeColor = renderSettings.BridgeColor.ToString();
        GuideLineColor = renderSettings.GuideLineColor.ToString();
    }
}
