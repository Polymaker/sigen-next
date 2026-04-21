using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiGen.Measuring;
using SiGen.Services;
using SiGen.Settings;
using SiGen.UI.LayoutViewer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiGen.ViewModels.Dialogs;

public partial class UserSettingsDialogViewModel : DialogViewModelBase
{
    private readonly ISettingsService _settingsService;
    private AppLanguage _originalLanguage;

    public override string Title => Lang.Resources.SettingsDialog_Title;

    [ObservableProperty]
    private AppTheme selectedTheme;

    [ObservableProperty]
    private AppLanguage selectedLanguage;

    [ObservableProperty]
    private UnitSystem selectedUnitSystem;

    [ObservableProperty]
    private LayoutViewerPreset selectedLayoutViewerPreset;

    /// <summary>
    /// Gets whether the language has been changed from the original.
    /// </summary>
    public bool IsLanguageChanged => SelectedLanguage != _originalLanguage;

    public IReadOnlyList<AppTheme> AvailableThemes { get; } =
    [
        AppTheme.System,
        AppTheme.Light,
        AppTheme.Dark
    ];

    public IReadOnlyList<AppLanguage> AvailableLanguages { get; } =
    [
        AppLanguage.System,
        AppLanguage.English,
        AppLanguage.French,
        AppLanguage.Spanish,
        AppLanguage.German
    ];

    public IReadOnlyList<UnitSystem> AvailableUnitSystems { get; } =
    [
        UnitSystem.Metric,
        UnitSystem.Imperial
    ];

    public IReadOnlyList<LayoutViewerPreset> AvailableLayoutViewerPresets { get; } =
    [
        LayoutViewerPreset.Light,
        LayoutViewerPreset.Dark,
        LayoutViewerPreset.Blueprint,
        LayoutViewerPreset.Custom
    ];

    /// <summary>
    /// Gets the current layout viewer color scheme for preview purposes.
    /// </summary>
    public LayoutViewerColorScheme CurrentColorSchemePreview => 
        GetColorSchemeFromPreset(SelectedLayoutViewerPreset);

    // Design-time constructor
    public UserSettingsDialogViewModel()
    {
        _settingsService = null!;
        SelectedTheme = AppTheme.System;
        SelectedLanguage = AppLanguage.System;
        _originalLanguage = AppLanguage.System;
        SelectedUnitSystem = UnitSystem.Metric;
        SelectedLayoutViewerPreset = LayoutViewerPreset.Light;
    }

    public UserSettingsDialogViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        
        // Load current settings
        SelectedTheme = settingsService.Settings.Theme;
        SelectedLanguage = settingsService.Settings.Language;
        _originalLanguage = settingsService.Settings.Language;
        SelectedUnitSystem = settingsService.Settings.PreferredUnits;
        SelectedLayoutViewerPreset = settingsService.Settings.LayoutViewerColorScheme.Preset;
    }

    partial void OnSelectedLayoutViewerPresetChanged(LayoutViewerPreset value)
    {
        OnPropertyChanged(nameof(CurrentColorSchemePreview));
    }

    partial void OnSelectedLanguageChanged(AppLanguage value)
    {
        OnPropertyChanged(nameof(IsLanguageChanged));
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_settingsService == null)
        {
            Complete();
            return;
        }

        // Apply all settings
        _settingsService.SetTheme(SelectedTheme);
        _settingsService.SetLanguage(SelectedLanguage);
        _settingsService.SetUnitSystem(SelectedUnitSystem);

        // Update layout viewer color scheme
        var colorSchemeSettings = new LayoutViewerColorSchemeSettings
        {
            Preset = SelectedLayoutViewerPreset
        };
        
        // If custom, preserve existing custom colors
        if (SelectedLayoutViewerPreset == LayoutViewerPreset.Custom)
        {
            var existing = _settingsService.Settings.LayoutViewerColorScheme;
            colorSchemeSettings.BackgroundColor = existing.BackgroundColor;
            colorSchemeSettings.GridColor = existing.GridColor;
            colorSchemeSettings.MajorAxisColor = existing.MajorAxisColor;
            colorSchemeSettings.OverlayTextColor = existing.OverlayTextColor;
            colorSchemeSettings.StringColor = existing.StringColor;
            colorSchemeSettings.FretColor = existing.FretColor;
            colorSchemeSettings.FingerBoardEdgeColor = existing.FingerBoardEdgeColor;
            colorSchemeSettings.NutColor = existing.NutColor;
            colorSchemeSettings.BridgeColor = existing.BridgeColor;
            colorSchemeSettings.GuideLineColor = existing.GuideLineColor;
        }

        _settingsService.SetLayoutViewerColorScheme(colorSchemeSettings);

        Dispatcher.UIThread.Post(() =>
        {
            var app = App.Current;
            if (app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                foreach (var w in desktop.Windows)
                {
                    try
                    {
                        w.InvalidateArrange();
                        w.InvalidateVisual();
                    }
                    catch { }
                }
            }
        });

        Complete();
    }

    private static LayoutViewerColorScheme GetColorSchemeFromPreset(LayoutViewerPreset preset) => preset switch
    {
        LayoutViewerPreset.Light => LayoutViewerColorScheme.LightMode,
        LayoutViewerPreset.Dark => LayoutViewerColorScheme.DarkMode,
        LayoutViewerPreset.Blueprint => LayoutViewerColorScheme.Blueprint,
        _ => LayoutViewerColorScheme.LightMode
    };
}
