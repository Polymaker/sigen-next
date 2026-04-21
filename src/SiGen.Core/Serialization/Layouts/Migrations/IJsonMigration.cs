using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SiGen.Serialization.Layouts.Migrations
{
    /// <summary>
    /// Represents a migration that transforms JSON data from one version to another.
    /// </summary>
    public interface IJsonMigration
    {
        /// <summary>
        /// Gets the source version this migration applies to.
        /// </summary>
        int FromVersion { get; }

        /// <summary>
        /// Gets the target version after migration.
        /// </summary>
        int ToVersion { get; }

        /// <summary>
        /// Migrates JSON data from one version to another.
        /// </summary>
        /// <param name="jsonDocument">The JSON document to migrate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The migrated JSON as a string.</returns>
        Task<string> MigrateAsync(JsonDocument jsonDocument, CancellationToken cancellationToken = default);
    }

    public class MigrateV2ToV3 : IJsonMigration
    {
        public int FromVersion => 2;
        public int ToVersion => 3;

        public Task<string> MigrateAsync(JsonDocument jsonDocument, CancellationToken cancellationToken = default)
        {
            var root = jsonDocument.RootElement;
            var writer = new System.IO.MemoryStream();
            using (var jsonWriter = new Utf8JsonWriter(writer, new JsonWriterOptions { Indented = true }))
            {
                jsonWriter.WriteStartObject();

                foreach (var property in root.EnumerateObject())
                {
                    if (property.Name == "NumberOfFrets")
                    {
                        // Create the Frets object with the NumberOfFrets property
                        jsonWriter.WritePropertyName("Frets");
                        jsonWriter.WriteStartObject();
                        jsonWriter.WriteNumber("NumberOfFrets", property.Value.GetInt32());
                        jsonWriter.WriteEndObject();
                    }
                    else
                    {
                        // Copy all other properties as-is
                        property.WriteTo(jsonWriter);
                    }
                }

                jsonWriter.WriteEndObject();
            }

            string migratedJson = System.Text.Encoding.UTF8.GetString(writer.ToArray());
            return Task.FromResult(migratedJson);
        }
    }
}

