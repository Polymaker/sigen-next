using System.Collections.Generic;

namespace SiGen.Settings;

public class UserSettings
{
    public List<RecentFileModel> RecentFiles { get; set; } = new();
    public string Theme { get; set; } = "Light"; // e.g. "Light", "Dark"
    public string PreferredUnits { get; set; } = "Metric"; // e.g. "Metric", "Imperial"
}

