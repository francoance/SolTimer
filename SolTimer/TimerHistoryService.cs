using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace SolTimer
{
    /// <summary>
    /// Persists timer history to a SQLite database located in
    /// <c>%APPDATA%\SolTimer\timer_history.db</c>.
    /// On first run, any existing <c>timer_history.json</c> is migrated
    /// automatically and renamed to <c>timer_history.json.migrated</c>.
    /// </summary>
    public class TimerHistoryService
    {
        private static TimerHistoryService? _instance;

        private readonly string _dbPath;
        private readonly string _legacyJsonPath;
        private readonly string _connectionString;

        // ------------------------------------------------------------------ //
        //  Singleton
        // ------------------------------------------------------------------ //

        /// <summary>Gets the application-wide singleton instance.</summary>
        public static TimerHistoryService Instance
        {
            get
            {
                _instance ??= new TimerHistoryService();
                return _instance;
            }
        }

        private TimerHistoryService()
        {
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SolTimer");

            Directory.CreateDirectory(appDataDir);

            _dbPath = Path.Combine(appDataDir, "timer_history.db");
            _legacyJsonPath = Path.Combine(appDataDir, "timer_history.json");
            _connectionString = $"Data Source={_dbPath}";

            Initialize();
        }

        // ------------------------------------------------------------------ //
        //  Public API
        // ------------------------------------------------------------------ //

        /// <summary>Returns all history entries ordered by most-recent first.</summary>
        public List<TimerEntry> GetHistory()
        {
            var entries = new List<TimerEntry>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT id, title, duration_ticks, created_at " +
                "FROM timer_entries " +
                "ORDER BY created_at DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                entries.Add(new TimerEntry
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Duration = TimeSpan.FromTicks(reader.GetInt64(2)),
                    CreatedAt = DateTime.Parse(reader.GetString(3),
                        null, System.Globalization.DateTimeStyles.RoundtripKind)
                });
            }

            return entries;
        }

        /// <summary>Inserts a new timer entry and sets its <see cref="TimerEntry.Id"/>.</summary>
        public void SaveTimer(string title, TimeSpan duration)
        {
            var entry = new TimerEntry(title, duration);

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                "INSERT INTO timer_entries (title, duration_ticks, created_at) " +
                "VALUES ($title, $ticks, $created_at); " +
                "SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("$title", entry.Title);
            command.Parameters.AddWithValue("$ticks", entry.Duration.Ticks);
            command.Parameters.AddWithValue("$created_at", entry.CreatedAt.ToString("o"));

            entry.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        /// <summary>
        /// Deletes a history entry. Deletes by <see cref="TimerEntry.Id"/> when
        /// available (non-zero), otherwise falls back to title + created_at match.
        /// </summary>
        public void DeleteTimer(TimerEntry entry)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            if (entry.Id != 0)
            {
                command.CommandText = "DELETE FROM timer_entries WHERE id = $id";
                command.Parameters.AddWithValue("$id", entry.Id);
            }
            else
            {
                command.CommandText =
                    "DELETE FROM timer_entries " +
                    "WHERE title = $title AND created_at = $created_at";
                command.Parameters.AddWithValue("$title", entry.Title);
                command.Parameters.AddWithValue("$created_at", entry.CreatedAt.ToString("o"));
            }

            command.ExecuteNonQuery();
        }

        // ------------------------------------------------------------------ //
        //  Initialisation and migration
        // ------------------------------------------------------------------ //

        private void Initialize()
        {
            CreateSchema();
            MigrateFromJson();
        }

        private void CreateSchema()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                "CREATE TABLE IF NOT EXISTS timer_entries (" +
                "    id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "    title TEXT NOT NULL," +
                "    duration_ticks INTEGER NOT NULL," +
                "    created_at TEXT NOT NULL" +
                ")";

            command.ExecuteNonQuery();
        }

        private void MigrateFromJson()
        {
            if (!File.Exists(_legacyJsonPath))
                return;

            // Only migrate when the database is currently empty to avoid
            // double-inserting if the app starts again before cleanup.
            if (!IsDatabaseEmpty())
                return;

            try
            {
                var json = File.ReadAllText(_legacyJsonPath);
                if (string.IsNullOrWhiteSpace(json))
                    return;

                var legacyEntries = JsonSerializer.Deserialize<List<TimerEntry>>(json);
                if (legacyEntries == null || legacyEntries.Count == 0)
                    return;

                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                using var transaction = connection.BeginTransaction();
                foreach (var entry in legacyEntries)
                {
                    using var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText =
                        "INSERT INTO timer_entries (title, duration_ticks, created_at) " +
                        "VALUES ($title, $ticks, $created_at)";

                    command.Parameters.AddWithValue("$title", entry.Title ?? string.Empty);
                    command.Parameters.AddWithValue("$ticks", entry.Duration.Ticks);
                    command.Parameters.AddWithValue("$created_at", entry.CreatedAt.ToString("o"));

                    command.ExecuteNonQuery();
                }

                transaction.Commit();

                // Rename — not delete — so the original data is preserved as a backup.
                File.Move(_legacyJsonPath, _legacyJsonPath + ".migrated");
            }
            catch
            {
                // Migration failure is non-fatal; the app continues with an empty history.
            }
        }

        private bool IsDatabaseEmpty()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM timer_entries";
            var count = Convert.ToInt64(command.ExecuteScalar());
            return count == 0;
        }
    }
}
