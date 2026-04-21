using SiGen.Layouts.Configuration;
using SiGen.Serialization.Layouts.Json;
using SiGen.Serialization.Layouts.Migrations;
using SiGen.Serialization.Layouts.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SiGen.Serialization.Layouts
{
    /// <summary>
    /// Main entry point for loading and saving layout files (.sil extension).
    /// Supports both XML (legacy) and JSON formats with automatic migration.
    /// </summary>
    public static class LayoutFileSerializer
    {
        public const string FileExtension = ".sil";
        public const int CurrentVersion = 3;

        private static readonly Dictionary<FileFormat, ILayoutSerializer> _serializers;
        private static readonly MigrationPipeline _migrationPipeline;

        static LayoutFileSerializer()
        {
            _migrationPipeline = new MigrationPipeline();

            // Register future JSON migrations here
            // e.g., _migrationPipeline.RegisterMigration(new V3ToV4Migration());
            _migrationPipeline.RegisterMigration(new MigrateV2ToV3());
            _serializers = new Dictionary<FileFormat, ILayoutSerializer>
            {
                { FileFormat.Xml, new XmlLayoutSerializer() },
                { FileFormat.Json, new JsonLayoutSerializer(_migrationPipeline) }
            };
        }

        /// <summary>
        /// Loads a layout configuration from a .sil file.
        /// Automatically detects format (XML/JSON) and migrates to the latest version.
        /// </summary>
        /// <param name="filePath">Path to the .sil file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The loaded configuration, migrated to the current version.</returns>
        public static async Task<InstrumentLayoutConfiguration> LoadAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Layout file not found: {filePath}", filePath);

            using (var stream = File.OpenRead(filePath))
            {
                return await LoadAsync(stream, cancellationToken);
            }
        }

        /// <summary>
        /// Loads a layout configuration from a stream.
        /// Automatically detects format (XML/JSON) and migrates to the latest version.
        /// </summary>
        /// <param name="stream">The stream to read from (must be seekable).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The loaded configuration, migrated to the current version.</returns>
        public static async Task<InstrumentLayoutConfiguration> LoadAsync(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("Stream must be seekable to detect format.", nameof(stream));

            var formatInfo = await VersionDetector.DetectAsync(stream, cancellationToken);

            if (!formatInfo.IsSupported)
            {
                throw new NotSupportedException(
                    $"Unsupported file format or version. Format: {formatInfo.Format}, Version: {formatInfo.Version}");
            }

            if (!_serializers.TryGetValue(formatInfo.Format, out var serializer))
            {
                throw new NotSupportedException($"No serializer found for format: {formatInfo.Format}");
            }

            // XML serializer handles v1-2 with custom deserialization and returns v3 config
            // JSON serializer handles v3+ with migrations applied before deserialization
            var configuration = await serializer.DeserializeAsync(stream, cancellationToken);

            return configuration;
        }

        /// <summary>
        /// Saves a layout configuration to a .sil file in JSON format.
        /// </summary>
        /// <param name="configuration">The configuration to save.</param>
        /// <param name="filePath">Path to save the .sil file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task SaveAsync(
            InstrumentLayoutConfiguration configuration,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = File.Create(filePath))
            {
                await SaveAsync(configuration, stream, cancellationToken);
            }
        }

        /// <summary>
        /// Saves a layout configuration to a stream in JSON format.
        /// </summary>
        /// <param name="configuration">The configuration to save.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task SaveAsync(
            InstrumentLayoutConfiguration configuration,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            var jsonSerializer = _serializers[FileFormat.Json];
            await jsonSerializer.SerializeAsync(configuration, stream, cancellationToken);
        }

        /// <summary>
        /// Detects the format and version of a layout file without fully loading it.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about the file format and version.</returns>
        public static async Task<FileFormatInfo> DetectFileFormatAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return await VersionDetector.DetectAsync(stream, cancellationToken);
            }
        }
    }
}
