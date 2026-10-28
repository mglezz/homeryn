using Homeryn.Server.Shopping;
using Homeryn.Shared.Shopping;
using Microsoft.Data.Sqlite;

namespace Homeryn.Server.Persistence
{
    public class SqliteShoppingItemRepository : IShoppingItemRepository
    {
        private readonly string connectionString;

        public SqliteShoppingItemRepository(string databasePath)
        {
            string fullPath = Path.GetFullPath(databasePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            SqliteConnectionStringBuilder settings = new SqliteConnectionStringBuilder();
            settings.DataSource = fullPath;
            settings.DefaultTimeout = 2;
            settings.Pooling = false;
            connectionString = settings.ToString();
        }

        public ShoppingPage ReadPage(long afterItemId, long? upperBoundId)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            if (upperBoundId == null)
            {
                command.CommandText = "SELECT COALESCE(MAX(Id), 0) FROM ShoppingItems;";
                upperBoundId = (long)command.ExecuteScalar()!;
            }
            command.CommandText = """
                SELECT Id, Name, Quantity, IsPurchased, Version FROM ShoppingItems
                WHERE Id > $after AND Id <= $upper ORDER BY Id LIMIT $limit;
                """;
            command.Parameters.AddWithValue("$after", afterItemId);
            command.Parameters.AddWithValue("$upper", upperBoundId.Value);
            command.Parameters.AddWithValue("$limit", ShoppingPage.PageSize + 1);
            List<ShoppingItem> items = new List<ShoppingItem>();
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    items.Add(ReadItem(reader));
                }
            }
            long? nextAfterId = null;
            if (items.Count > ShoppingPage.PageSize)
            {
                items.RemoveAt(ShoppingPage.PageSize);
                nextAfterId = items[items.Count - 1].Id;
            }
            transaction.Commit();
            return new ShoppingPage(items, upperBoundId.Value, nextAfterId);
        }

        public ShoppingItem Add(string name, int quantity, bool isPurchased)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO ShoppingItems (Name, Quantity, IsPurchased, Version)
                VALUES ($name, $quantity, $purchased, 1)
                RETURNING Id, Name, Quantity, IsPurchased, Version;
                """;
            SetItemParameters(command, name, quantity, isPurchased);
            using SqliteDataReader reader = command.ExecuteReader();
            reader.Read();
            return ReadItem(reader);
        }

        public ShoppingItem Update(long id, long expectedVersion, string name, int quantity, bool isPurchased)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction transaction = connection.BeginTransaction();
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE ShoppingItems SET Name = $name, Quantity = $quantity,
                    IsPurchased = $purchased, Version = Version + 1
                WHERE Id = $id AND Version = $version
                RETURNING Id, Name, Quantity, IsPurchased, Version;
                """;
            SetItemParameters(command, name, quantity, isPurchased);
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$version", expectedVersion);
            ShoppingItem? item = null;
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
            command.CommandText = "DELETE FROM ShoppingItems WHERE Id = $id AND Version = $version;";
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

        private static ShoppingItem ReadItem(SqliteDataReader reader)
        {
            return new ShoppingItem(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2),
                reader.GetBoolean(3), reader.GetInt64(4));
        }

        private static void SetItemParameters(SqliteCommand command, string name, int quantity, bool isPurchased)
        {
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$quantity", quantity);
            command.Parameters.AddWithValue("$purchased", isPurchased);
        }

        private static void ThrowChangeRejected(SqliteConnection connection, SqliteTransaction transaction, long id)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT COUNT(*) FROM ShoppingItems WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);
            if ((long)command.ExecuteScalar()! == 0)
            {
                throw new ShoppingListException("ITEM_NOT_FOUND");
            }
            throw new ShoppingListException("ITEM_CONFLICT");
        }
    }
}
