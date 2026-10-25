using System.Globalization;
using Homeryn.Server.Chores;
using Homeryn.Shared.Chores;
using Microsoft.Data.Sqlite;

namespace Homeryn.Server.Persistence
{
    public class SqliteChoreRepository : IChoreRepository
    {
        private readonly string connectionString;

        public SqliteChoreRepository(string databasePath)
        {
            string fullPath = Path.GetFullPath(databasePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            SqliteConnectionStringBuilder settings = new SqliteConnectionStringBuilder();
            settings.DataSource = fullPath;
            settings.DefaultTimeout = 2;
            settings.Pooling = false;
            connectionString = settings.ToString();
        }

        public ChorePage ReadPage(long afterChoreId, long? upperBoundId)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            if (upperBoundId == null)
            {
                command.CommandText = "SELECT COALESCE(MAX(Id), 0) FROM Chores;";
                upperBoundId = (long)command.ExecuteScalar()!;
            }
            command.CommandText = """
                SELECT Id, Name, Assignee, DueDate, IsCompleted, Version FROM Chores
                WHERE Id > $after AND Id <= $upper ORDER BY Id LIMIT $limit;
                """;
            command.Parameters.AddWithValue("$after", afterChoreId);
            command.Parameters.AddWithValue("$upper", upperBoundId.Value);
            command.Parameters.AddWithValue("$limit", ChorePage.PageSize + 1);
            List<Chore> items = new List<Chore>();
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    items.Add(ReadItem(reader));
                }
            }
            long? nextAfterId = null;
            if (items.Count > ChorePage.PageSize)
            {
                items.RemoveAt(ChorePage.PageSize);
                nextAfterId = items[items.Count - 1].Id;
            }
            transaction.Commit();
            return new ChorePage(items, upperBoundId.Value, nextAfterId);
        }

        public Chore Add(string name, string? assignee, DateOnly? dueDate, bool isCompleted)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Chores (Name, Assignee, DueDate, IsCompleted, Version)
                VALUES ($name, $assignee, $dueDate, $completed, 1)
                RETURNING Id, Name, Assignee, DueDate, IsCompleted, Version;
                """;
            SetItemParameters(command, name, assignee, dueDate, isCompleted);
            using SqliteDataReader reader = command.ExecuteReader();
            reader.Read();
            return ReadItem(reader);
        }

        public Chore Update(long id, long expectedVersion, string name, string? assignee, DateOnly? dueDate, bool isCompleted)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction transaction = connection.BeginTransaction();
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE Chores SET Name = $name, Assignee = $assignee, DueDate = $dueDate,
                    IsCompleted = $completed, Version = Version + 1
                WHERE Id = $id AND Version = $version
                RETURNING Id, Name, Assignee, DueDate, IsCompleted, Version;
                """;
            SetItemParameters(command, name, assignee, dueDate, isCompleted);
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$version", expectedVersion);
            Chore? item = null;
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    item = ReadItem(reader);
                }
            }
            if (item == null)
            {
                ThrowChangeRejected(connection, transaction, id);
            }
            transaction.Commit();
            return item!;
        }

        public void Delete(long id, long expectedVersion)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction transaction = connection.BeginTransaction();
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM Chores WHERE Id = $id AND Version = $version;";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$version", expectedVersion);
            if (command.ExecuteNonQuery() == 0)
            {
                ThrowChangeRejected(connection, transaction, id);
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

        private static Chore ReadItem(SqliteDataReader reader)
        {
            return new Chore(reader.GetInt64(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : DateOnly.ParseExact(reader.GetString(3), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                reader.GetBoolean(4), reader.GetInt64(5));
        }

        private static void SetItemParameters(SqliteCommand command, string name, string? assignee, DateOnly? dueDate, bool isCompleted)
        {
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$assignee", (object?)assignee ?? DBNull.Value);
            command.Parameters.AddWithValue("$dueDate", (object?)dueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? DBNull.Value);
            command.Parameters.AddWithValue("$completed", isCompleted);
        }

        private static void ThrowChangeRejected(SqliteConnection connection, SqliteTransaction transaction, long id)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT COUNT(*) FROM Chores WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);
            if ((long)command.ExecuteScalar()! == 0)
            {
                throw new ChoresException("CHORE_NOT_FOUND");
            }
            throw new ChoresException("CHORE_CONFLICT");
        }
    }
}
