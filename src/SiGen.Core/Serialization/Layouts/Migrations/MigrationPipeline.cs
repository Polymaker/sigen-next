using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SiGen.Serialization.Layouts.Migrations
{
    /// <summary>
    /// Orchestrates JSON migrations between different layout versions.
    /// </summary>
    public class MigrationPipeline
    {
        private readonly List<IJsonMigration> _migrations;

        public MigrationPipeline()
        {
            _migrations = new List<IJsonMigration>();
        }

        /// <summary>
        /// Registers a migration.
        /// </summary>
        public void RegisterMigration(IJsonMigration migration)
        {
            _migrations.Add(migration);
        }

        /// <summary>
        /// Migrates JSON data from one version to the latest version.
        /// </summary>
        /// <param name="json">The JSON string to migrate.</param>
        /// <param name="fromVersion">The current version of the JSON.</param>
        /// <param name="toVersion">The target version (usually the latest).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The migrated JSON string.</returns>
        public async Task<string> MigrateAsync(
            string json,
            int fromVersion,
            int toVersion,
            CancellationToken cancellationToken = default)
        {
            if (fromVersion == toVersion)
                return json;

            if (fromVersion > toVersion)
                throw new InvalidOperationException($"Cannot migrate backwards from version {fromVersion} to {toVersion}.");

            var path = FindMigrationPath(fromVersion, toVersion);
            if (path == null || path.Count == 0)
                throw new InvalidOperationException($"No migration path found from version {fromVersion} to {toVersion}.");

            var currentJson = json;
            foreach (var migration in path)
            {
                using var doc = JsonDocument.Parse(currentJson);
                currentJson = await migration.MigrateAsync(doc, cancellationToken);
            }

            return currentJson;
        }

        /// <summary>
        /// Finds the shortest migration path from one version to another.
        /// </summary>
        private List<IJsonMigration>? FindMigrationPath(int fromVersion, int toVersion)
        {
            var path = new List<IJsonMigration>();
            var currentVersion = fromVersion;

            while (currentVersion < toVersion)
            {
                var migration = _migrations.FirstOrDefault(m => m.FromVersion == currentVersion);
                if (migration == null)
                    return null;

                path.Add(migration);
                currentVersion = migration.ToVersion;
            }

            return path;
        }

        /// <summary>
        /// Checks if a migration path exists between two versions.
        /// </summary>
        public bool CanMigrate(int fromVersion, int toVersion)
        {
            if (fromVersion == toVersion)
                return true;

            if (fromVersion > toVersion)
                return false;

            return FindMigrationPath(fromVersion, toVersion) != null;
        }
    }
}

