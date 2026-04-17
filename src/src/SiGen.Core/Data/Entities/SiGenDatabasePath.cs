using System;
using System.IO;

namespace SiGen.Data.Entities
{
    public static class SiGenDatabasePath
    {
        public const string DatabaseFileName = "SiGenDatabase.db";
        public const string DatabasePathEnvironmentVariable = "SIGEN_DB_PATH";

        public static string GetDatabaseFilePath()
        {
            var envPath = Environment.GetEnvironmentVariable(DatabasePathEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                var fullPath = Path.GetFullPath(envPath);
                EnsureDirectory(fullPath);
                return fullPath;
            }

            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(appDataFolder, "SiGen", DatabaseFileName);
            EnsureDirectory(dbPath);
            return dbPath;
        }

        public static string GetConnectionString()
            => $"Data Source={GetDatabaseFilePath()}";

        private static void EnsureDirectory(string dbPath)
        {
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
        }
    }
}
