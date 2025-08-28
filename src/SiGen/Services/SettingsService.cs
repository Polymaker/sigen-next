using SiGen.Data.Common;
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
    //void AddRecentFile(string filePath);
    void AddRecentFile(ILayoutDocumentContext document);
}

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    public UserSettings Settings { get; private set; } = new UserSettings();

    public const int MaxRecentFiles = 10;

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
    }
}

internal class MockSettingsService : ISettingsService
{
    public UserSettings Settings { get; private set; } = new UserSettings();

    public MockSettingsService()
    {
        Settings.RecentFiles.Add(new RecentFileModel
        {
            DisplayName = "My First Layout",
            FilePath = @"C:\Layouts\MyFirstLayout.sigen",
            LastOpened = DateTime.Now.AddDays(-2),
        });
        Settings.RecentFiles.Add(new RecentFileModel
        {
            DisplayName = "Bass Guitar",
            FilePath = @"C:\Layouts\BassGuitar.sigen",
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
}

