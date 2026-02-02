using System.Text.Json;
using System.Text.Json.Serialization;
using SiGen.Serialization;

namespace SiGen.Serialization
{
    public static class SiGenJsonOptions
    {
        public static readonly JsonSerializerOptions Default = CreateDefaultOptions();

        private static JsonSerializerOptions CreateDefaultOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            options.Converters.Add(new MeasureConverter());
            options.Converters.Add(new NullableMeasureConverter());
            options.Converters.Add(new BaseStringConfigurationConverter());
            options.Converters.Add(new NoteConverter());
            options.Converters.Add(new NullableNoteConverter());
            // Add other converters as needed
            return options;
        }
    }
}
