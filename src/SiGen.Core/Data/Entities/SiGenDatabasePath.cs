using System;
using System.IO;

namespace SiGen.Data.Entities
{
    public static class SiGenDatabasePath
    {
        public const string DatabaseFileName = "SiGenDatabase.db";
        public const string DatabasePathEnvironmentVariable = "SIGEN_DB_PATH";
        public const string PortableModeEnvironmentVariable = "SIGEN_PORTABLE";

        public static string GetDatabaseFilePath()
        {
            var envPath = Environment.GetEnvironmentVariable(DatabasePathEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                var fullPath = Path.GetFullPath(envPath);
                EnsureDirectory(fullPath);
                return fullPath;
            }

            var portableDbPath = Path.Combine(AppContext.BaseDirectory, DatabaseFileName);
            if (IsPortableModeRequested() || File.Exists(portableDbPath))
            {
                EnsureDirectory(portableDbPath);
                return portableDbPath;
            }

            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(appDataFolder, "SiGen", DatabaseFileName);
            EnsureDirectory(dbPath);
            return dbPath;
        }

        public static string GetConnectionString()
            => $"Data Source={GetDatabaseFilePath()}";

        private static bool IsPortableModeRequested()
        {
            var value = Environment.GetEnvironmentVariable(PortableModeEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        private static void EnsureDirectory(string dbPath)
        {
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
        }
    }
}
