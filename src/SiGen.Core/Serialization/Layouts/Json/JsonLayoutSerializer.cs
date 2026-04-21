using SiGen.Layouts.Configuration;
using SiGen.Serialization.Layouts.Migrations;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SiGen.Serialization.Layouts.Json
{
    /// <summary>
    /// Serializes and deserializes layout configurations in JSON format (version 3+).
    /// </summary>
    public class JsonLayoutSerializer : ILayoutSerializer
    {
        public const int CurrentVersion = 3;

        private readonly JsonSerializerOptions _options;
        private readonly MigrationPipeline _migrationPipeline;

        public FileFormat Format => FileFormat.Json;


        public JsonLayoutSerializer(MigrationPipeline migrationPipeline)
        {
            _options = SiGenJsonOptions.Default;
            _migrationPipeline = migrationPipeline;
        }

        public async Task<InstrumentLayoutConfiguration> DeserializeAsync(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            // Read the JSON as string first to allow migrations
            string json;
            using (var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                json = await reader.ReadToEndAsync();
            }

            // Detect version from JSON
            int version = 2;
            using (var doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("version", out var versionProp))
                {
                    version = versionProp.GetInt32();
                }
            }

            if (version < CurrentVersion && _migrationPipeline.CanMigrate(version, CurrentVersion))
            {
                json = await _migrationPipeline.MigrateAsync(json, version, CurrentVersion, cancellationToken);
            }

            // Deserialize the (possibly migrated) JSON
            var configuration = JsonSerializer.Deserialize<InstrumentLayoutConfiguration>(json, _options);

            if (configuration == null)
                throw new InvalidDataException("Failed to deserialize layout configuration.");

            // Note: Don't auto-update version here - let the caller detect migration
            // This allows the UI to prompt the user to save in the new format

            return configuration;
        }

        public async Task SerializeAsync(
            InstrumentLayoutConfiguration configuration,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            configuration.Version = CurrentVersion;

            await JsonSerializer.SerializeAsync(
                stream,
                configuration,
                _options,
                cancellationToken);
        }
    }

}

