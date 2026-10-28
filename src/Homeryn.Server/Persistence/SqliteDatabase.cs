using Microsoft.Data.Sqlite;

namespace Homeryn.Server.Persistence
{
    public class SqliteDatabase
    {
        private readonly string connectionString;

        public SqliteDatabase(string databasePath)
        {
            string fullPath = Path.GetFullPath(databasePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            SqliteConnectionStringBuilder settings = new SqliteConnectionStringBuilder();
            settings.DataSource = fullPath;
            settings.DefaultTimeout = 2;
            settings.Pooling = false;
            connectionString = settings.ToString();
        }

        public void Initialize()
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteCommand journal = connection.CreateCommand();
            journal.CommandText = "PRAGMA journal_mode = WAL;";
            journal.ExecuteNonQuery();

            using SqliteTransaction transaction = connection.BeginTransaction();
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "PRAGMA user_version;";
            long version = (long)command.ExecuteScalar()!;
            if (version > 2)
            {
                throw new InvalidDataException("This database requires a newer server version.");
            }
            if (version == 0)
            {
                command.CommandText = """
                    CREATE TABLE ShoppingItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL CHECK (length(Name) BETWEEN 1 AND 80),
                        Quantity INTEGER NOT NULL CHECK (Quantity BETWEEN 1 AND 999),
                        IsPurchased INTEGER NOT NULL CHECK (IsPurchased IN (0, 1)),
                        Version INTEGER NOT NULL CHECK (Version > 0)
                    );
                    PRAGMA user_version = 1;
                    """;
                command.ExecuteNonQuery();
            }
            if (version < 2)
            {
                command.CommandText = """
                    CREATE TABLE Chores (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL CHECK (length(Name) BETWEEN 1 AND 80),
                        Assignee TEXT NULL CHECK (Assignee IS NULL OR length(Assignee) BETWEEN 1 AND 60),
                        DueDate TEXT NULL,
                        IsCompleted INTEGER NOT NULL CHECK (IsCompleted IN (0, 1)),
                        Version INTEGER NOT NULL CHECK (Version > 0)
                    );
                    PRAGMA user_version = 2;
                    """;
                command.ExecuteNonQuery();
            }
            transaction.Commit();
        }

        private SqliteConnection OpenConnection()
        {
            SqliteConnection connection = new SqliteConnection(connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

    }
}
