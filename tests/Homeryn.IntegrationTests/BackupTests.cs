using Homeryn.Desktop.Networking;
using Homeryn.Server.Persistence;
using Homeryn.Shared.Chores;
using Homeryn.Shared.Shopping;
using Microsoft.Data.Sqlite;

namespace Homeryn.IntegrationTests
{
    internal static class BackupTests
    {
        public static async Task<int> RunAsync(string directory, CancellationToken token)
        {
            string source = Path.Combine(directory, "backup-source.db");
            string backupPath = Path.Combine(directory, "backups", "snapshot.db");
            await using TestServer server = new TestServer(source);
            SqliteConnectionStringBuilder settings = new SqliteConnectionStringBuilder();
            settings.DataSource = source;
            settings.Pooling = false;
            // Hold a connection open so committed changes remain in the WAL during the backup.
            using SqliteConnection keeper = new SqliteConnection(settings.ToString());
            keeper.Open();
            using (SqliteCommand activate = keeper.CreateCommand())
            {
                activate.CommandText = "PRAGMA journal_mode = WAL; SELECT COUNT(*) FROM ShoppingItems;";
                activate.ExecuteScalar();
            }
            ShoppingItem item = await server.Client.AddAsync(new ShoppingItemInput("Backup milk", 2), token);
            item = await server.Client.UpdateAsync(item, new ShoppingItemInput("Backup milk", 3, true), token);
            ChoresClient chores = new ChoresClient(port: server.Port);
            Chore chore = await chores.AddAsync(new ChoreInput("Backup kitchen", "Alex", new DateOnly(2026, 10, 31), true), token);
            if (!File.Exists(source + "-wal"))
            {
                throw new InvalidOperationException("The backup test must exercise an active WAL database.");
            }
            DatabaseBackup.Create(source, backupPath);
            await server.Client.DeleteAsync(item, token);
            await using (TestServer restored = new TestServer(backupPath))
            {
                ShoppingItem copy = (await restored.Client.ReadAllAsync(token)).Single();
                Chore choreCopy = (await new ChoresClient(port: restored.Port).ReadAllAsync(token)).Single();
                if (copy.Id != item.Id || copy.Version != 2 || copy.Quantity != 3 || !copy.IsPurchased
                    || choreCopy.Id != chore.Id || choreCopy.Assignee != "Alex" || choreCopy.DueDate != chore.DueDate
                    || !choreCopy.IsCompleted || (await server.Client.ReadAllAsync(token)).Count != 0)
                {
                    throw new InvalidOperationException("The backup did not restore an independent consistent copy.");
                }
            }
            Console.WriteLine("PASS: Online backup restores shopping and chores from an active WAL independently");

            byte[] originalBackup = File.ReadAllBytes(backupPath);
            ExpectFailure(() => DatabaseBackup.Create(source, backupPath));
            ExpectFailure(() => DatabaseBackup.Create(source, source));
            if (!File.ReadAllBytes(backupPath).SequenceEqual(originalBackup))
            {
                throw new InvalidOperationException("A refused backup modified its existing destination.");
            }
            Console.WriteLine("PASS: Backup refuses existing destinations and the source database");

            string missing = Path.Combine(directory, "missing-source.db");
            string missingBackup = Path.Combine(directory, "missing-backup.db");
            ExpectFailure(() => DatabaseBackup.Create(missing, missingBackup));
            if (File.Exists(missing) || File.Exists(missingBackup))
            {
                throw new InvalidOperationException("Backing up a missing database must not create empty databases.");
            }
            Console.WriteLine("PASS: Missing backup sources fail without creating databases");

            string invalid = Path.Combine(directory, "invalid-source.db");
            string invalidBackup = Path.Combine(directory, "invalid-backup.db");
            File.WriteAllText(invalid, "This is not a SQLite database.");
            ExpectFailure(() => DatabaseBackup.Create(invalid, invalidBackup));
            if (File.Exists(invalidBackup) || Directory.GetFiles(directory, "invalid-backup.db.*.tmp").Length != 0)
            {
                throw new InvalidOperationException("An invalid source left a published or partial backup.");
            }
            Console.WriteLine("PASS: Invalid backup sources leave no published or partial backup");
            return 4;
        }

        private static void ExpectFailure(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception) when (exception is IOException || exception is SqliteException)
            {
                return;
            }
            throw new InvalidOperationException("An unsafe backup operation was accepted.");
        }
    }
}
