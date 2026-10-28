using Homeryn.Desktop.Networking;
using Homeryn.Server.Persistence;
using Homeryn.Shared.Chores;
using Homeryn.Shared.Shopping;
using Microsoft.Data.Sqlite;

namespace Homeryn.IntegrationTests
{
    internal static class DatabaseMigrationTests
    {
        public static async Task<int> RunAsync(string directory, CancellationToken token)
        {
            string path = Path.Combine(directory, "version-one.db");
            SqliteConnectionStringBuilder settings = new SqliteConnectionStringBuilder();
            settings.DataSource = path;
            settings.Pooling = false;
            using (SqliteConnection connection = new SqliteConnection(settings.ToString()))
            {
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = """
                    CREATE TABLE ShoppingItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL CHECK (length(Name) BETWEEN 1 AND 80),
                        Quantity INTEGER NOT NULL CHECK (Quantity BETWEEN 1 AND 999),
                        IsPurchased INTEGER NOT NULL CHECK (IsPurchased IN (0, 1)),
                        Version INTEGER NOT NULL CHECK (Version > 0)
                    );
                    INSERT INTO ShoppingItems VALUES (7, 'Existing shopping', 4, 1, 3);
                    PRAGMA user_version = 1;
                    """;
                command.ExecuteNonQuery();
            }
            await using (TestServer server = new TestServer(path))
            {
                ShoppingItem existing = (await server.Client.ReadAllAsync(token)).Single();
                if (existing.Id != 7 || existing.Name != "Existing shopping" || existing.Quantity != 4
                    || !existing.IsPurchased || existing.Version != 3)
                {
                    throw new InvalidOperationException("Migration changed existing shopping data.");
                }
                ChoresClient client = new ChoresClient(port: server.Port);
                if ((await client.ReadAllAsync(token)).Count != 0)
                {
                    throw new InvalidOperationException("Migrated chores must start empty.");
                }
                await client.AddAsync(new ChoreInput("First chore"), token);
                ShoppingItem added = await server.Client.AddAsync(new ShoppingItemInput("After migration", 1), token);
                if (added.Id <= existing.Id)
                {
                    throw new InvalidOperationException("Migration reset shopping identities.");
                }
            }
            await using (TestServer restarted = new TestServer(path))
            {
                if ((await restarted.Client.ReadAllAsync(token)).Count != 2
                    || (await new ChoresClient(port: restarted.Port).ReadAllAsync(token)).Count != 1)
                {
                    throw new InvalidOperationException("Repeated initialization lost migrated data.");
                }
            }
            Console.WriteLine("PASS: Version-one migration preserves shopping and adds independent persistent chores");

            using (SqliteConnection connection = new SqliteConnection(settings.ToString()))
            {
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = "PRAGMA user_version = 99;";
                command.ExecuteNonQuery();
            }
            try
            {
                new SqliteDatabase(path).Initialize();
            }
            catch (InvalidDataException)
            {
                Console.WriteLine("PASS: A newer database schema is rejected");
                return 2;
            }
            throw new InvalidOperationException("A newer schema must not be opened.");
        }
    }
}
