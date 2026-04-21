using Avalonia.Styling;
using SiGen.Data.Common;
using SiGen.Measuring;
using SiGen.Settings;
using SiGen.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SiGen.Services;

public interface ISettingsService
{
    UserSettings Settings { get; }
    void Save();
    void Load();
    void AddRecentFile(ILayoutDocumentContext document);

    /// <summary>
    /// Raised when the recent files list is updated.
    /// </summary>
    event EventHandler? RecentFilesChanged;

    /// <summary>
    /// Raised when the application theme is changed.
    /// </summary>
    event EventHandler<AppTheme>? ThemeChanged;

    /// <summary>
    /// Raised when the application language is changed.
    /// </summary>
    event EventHandler<AppLanguage>? LanguageChanged;

    /// <summary>
    /// Raised when the preferred unit system is changed.
    /// </summary>
    event EventHandler<UnitSystem>? UnitSystemChanged;

    /// <summary>
    /// Raised when the layout viewer color scheme settings are changed.
    /// </summary>
    event EventHandler<LayoutViewerColorSchemeSettings>? LayoutViewerColorSchemeChanged;

    /// <summary>
    /// Sets the application theme and raises the <see cref="ThemeChanged"/> event.
    /// </summary>
    void SetTheme(AppTheme theme);

    /// <summary>
    /// Sets the application language and raises the <see cref="LanguageChanged"/> event.
    /// </summary>
    void SetLanguage(AppLanguage language);

    /// <summary>
    /// Sets the preferred unit system and raises the <see cref="UnitSystemChanged"/> event.
    /// </summary>
    void SetUnitSystem(UnitSystem unitSystem);

    /// <summary>
    /// Sets the layout viewer color scheme settings and raises the <see cref="LayoutViewerColorSchemeChanged"/> event.
    /// </summary>
    void SetLayoutViewerColorScheme(LayoutViewerColorSchemeSettings colorSchemeSettings);
}

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    public UserSettings Settings { get; private set; } = new UserSettings();

    public const int MaxRecentFiles = 10;

    /// <summary>
    /// Raised when the recent files list is updated.
    /// </summary>
    public event EventHandler? RecentFilesChanged;

    /// <summary>
    /// Raised when the application theme is changed.
    /// </summary>
    public event EventHandler<AppTheme>? ThemeChanged;

    /// <summary>
    /// Raised when the application language is changed.
    /// </summary>
    public event EventHandler<AppLanguage>? LanguageChanged;

    /// <summary>
    /// Raised when the preferred unit system is changed.
    /// </summary>
    public event EventHandler<UnitSystem>? UnitSystemChanged;

    /// <summary>
    /// Raised when the layout viewer color scheme settings are changed.
    /// </summary>
    public event EventHandler<LayoutViewerColorSchemeSettings>? LayoutViewerColorSchemeChanged;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var settingsDir = Path.Combine(appData, "SiGen");
        Directory.CreateDirectory(settingsDir);
        _settingsFilePath = Path.Combine(settingsDir, "usersettings.json");
        Load();
    }

    public void Load()
    {
        if (File.Exists(_settingsFilePath))
        {
            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<UserSettings>(json);
                if (loaded != null)
                {
                    // Remove non-existing files from recent list
                    loaded.RecentFiles = loaded.RecentFiles
                        .Where(f => File.Exists(f.FilePath))
                        .ToList();
                    Settings = loaded;
                }
            }
            catch { }
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
        catch { }
    }

    //public void AddRecentFile(string filePath)
    //{
    //    if (string.IsNullOrWhiteSpace(filePath))
    //        return;
    //    var displayName = Path.GetFileNameWithoutExtension(filePath);
    //    // Remove if already exists
    //    Settings.RecentFiles.RemoveAll(f => string.Equals(f.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
    //    // Insert at the top
    //    Settings.RecentFiles.Insert(0, new RecentFileModel
    //    {
    //        FilePath = filePath,
    //        DisplayName = displayName,
    //        LastOpened = DateTime.Now
    //    });
    //    // Trim excess
    //    if (Settings.RecentFiles.Count > MaxRecentFiles)
    //        Settings.RecentFiles.RemoveRange(MaxRecentFiles, Settings.RecentFiles.Count - MaxRecentFiles);
    //    Save();
    //}

    public void AddRecentFile(ILayoutDocumentContext document)
    {
        if (string.IsNullOrWhiteSpace(document.FilePath))
            return;
        var displayName = Path.GetFileNameWithoutExtension(document.FilePath);
        // Remove if already exists
        Settings.RecentFiles.RemoveAll(f => string.Equals(f.FilePath, document.FilePath, StringComparison.OrdinalIgnoreCase));
        // Insert at the top
        Settings.RecentFiles.Insert(0, new RecentFileModel
        {
            FilePath = document.FilePath,
            DisplayName = displayName,
            LastOpened = DateTime.Now,
            InstrumentType = document.Configuration?.InstrumentType ?? InstrumentType.Custom
        });
        // Trim excess
        if (Settings.RecentFiles.Count > MaxRecentFiles)
            Settings.RecentFiles.RemoveRange(MaxRecentFiles, Settings.RecentFiles.Count - MaxRecentFiles);
        Save();

        // Notify subscribers that the recent files list has changed
        RecentFilesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void SetTheme(AppTheme theme)
    {
        if (Settings.Theme != theme)
        {
            Settings.Theme = theme;
            Save();
            ThemeChanged?.Invoke(this, theme);
        }
    }

    /// <inheritdoc/>
    public void SetLanguage(AppLanguage language)
    {
        if (Settings.Language != language)
        {
            Settings.Language = language;
            Save();
            LanguageChanged?.Invoke(this, language);
        }
    }

    /// <inheritdoc/>
    public void SetUnitSystem(UnitSystem unitSystem)
    {
        if (Settings.PreferredUnits != unitSystem)
        {
            Settings.PreferredUnits = unitSystem;
            Save();
            UnitSystemChanged?.Invoke(this, unitSystem);
        }
    }

    /// <inheritdoc/>
    public void SetLayoutViewerColorScheme(LayoutViewerColorSchemeSettings colorSchemeSettings)
    {
        Settings.LayoutViewerColorScheme = colorSchemeSettings;
        Save();
        LayoutViewerColorSchemeChanged?.Invoke(this, colorSchemeSettings);
    }
}

internal class MockSettingsService : ISettingsService
{
    public UserSettings Settings { get; private set; } = new UserSettings();

    public event EventHandler? RecentFilesChanged;
    public event EventHandler<AppTheme>? ThemeChanged;
    public event EventHandler<AppLanguage>? LanguageChanged;
    public event EventHandler<UnitSystem>? UnitSystemChanged;
    public event EventHandler<LayoutViewerColorSchemeSettings>? LayoutViewerColorSchemeChanged;

    public MockSettingsService()
    {
        Settings.RecentFiles.Add(new RecentFileModel
        {
            DisplayName = "My First Layout",
            FilePath = @"C:\Layouts\MyFirstLayout.sil",
            LastOpened = DateTime.Now.AddDays(-2),
        });
        Settings.RecentFiles.Add(new RecentFileModel
        {
            DisplayName = "Bass Guitar",
            FilePath = @"C:\Layouts\BassGuitar.sil",
            LastOpened = DateTime.Now.AddDays(-5),
        });
    }

    public void AddRecentFile(string filePath)
    {
        throw new NotImplementedException();
    }

    public void AddRecentFile(ILayoutDocumentContext document)
    {
        throw new NotImplementedException();
    }

    public void Load()
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public void SetTheme(AppTheme theme)
    {
        Settings.Theme = theme;
        ThemeChanged?.Invoke(this, theme);
    }

    public void SetLanguage(AppLanguage language)
    {
        Settings.Language = language;
        LanguageChanged?.Invoke(this, language);
    }

    public void SetUnitSystem(UnitSystem unitSystem)
    {
        Settings.PreferredUnits = unitSystem;
        UnitSystemChanged?.Invoke(this, unitSystem);
    }

    public void SetLayoutViewerColorScheme(LayoutViewerColorSchemeSettings colorSchemeSettings)
    {
        Settings.LayoutViewerColorScheme = colorSchemeSettings;
        LayoutViewerColorSchemeChanged?.Invoke(this, colorSchemeSettings);
    }
}

