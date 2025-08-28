using SiGen.Data.Common;
using System;
using System.Text.Json.Serialization;

namespace SiGen.Settings;

public class RecentFileModel
{
    public string FilePath { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime LastOpened { get; set; }
    //todo: change to string to avoid issues when loading old settings
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InstrumentType InstrumentType { get; set; } = InstrumentType.Custom;
}

