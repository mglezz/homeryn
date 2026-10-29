using Microsoft.Data.Sqlite;

namespace Homeryn.Server.Persistence
{
    public static class DatabaseBackup
    {
        public static void Create(string sourcePath, string destinationPath)
        {
            string source = Path.GetFullPath(sourcePath);
            string destination = Path.GetFullPath(destinationPath);
            if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase) || File.Exists(destination))
            {
                throw new IOException("Choose a new backup filename. Existing files are never overwritten.");
            }
            SqliteConnectionStringBuilder settings = new SqliteConnectionStringBuilder();
            settings.DataSource = source;
            settings.Mode = SqliteOpenMode.ReadOnly;
            settings.Pooling = false;
            settings.DefaultTimeout = 2;
            using SqliteConnection connection = new SqliteConnection(settings.ToString());
            connection.Open();
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            string temporaryPath = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
            // Reserve a unique staging file; publish it only after a complete, verified backup.
            using (FileStream reservation = new FileStream(temporaryPath, FileMode.CreateNew))
            {
            }
            try
            {
                settings.DataSource = temporaryPath;
                settings.Mode = SqliteOpenMode.ReadWrite;
                using (SqliteConnection backup = new SqliteConnection(settings.ToString()))
                {
                    backup.Open();
                    connection.BackupDatabase(backup);
                    using SqliteCommand command = backup.CreateCommand();
                    command.CommandText = "PRAGMA journal_mode = DELETE;";
                    command.ExecuteNonQuery();
                    command.CommandText = "PRAGMA integrity_check;";
                    if (!string.Equals(command.ExecuteScalar() as string, "ok", StringComparison.Ordinal))
                    {
                        throw new InvalidDataException("The backup did not pass SQLite integrity validation.");
                    }
                }
                File.Move(temporaryPath, destination);
            }
            finally
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
