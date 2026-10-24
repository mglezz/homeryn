using Homeryn.Desktop.Networking;
using Homeryn.Shared.Contracts;
using Homeryn.Shared.Shopping;
using Microsoft.Data.Sqlite;

namespace Homeryn.IntegrationTests
{
    internal static class ShoppingListTests
    {
        public static async Task<int> RunAsync(string directory, CancellationToken token)
        {
            int passed = 0;
            string database = Path.Combine(directory, "shopping.db");
            ShoppingItem saved;
            long removedId;
            await using (TestServer server = new TestServer(database))
            {
                ShoppingListClient client = server.Client;
                Assert((await client.ReadAllAsync(token)).Count == 0, "New list must be empty.");
                ShoppingItem item = await client.AddAsync(new ShoppingItemInput("  Café d'avoine 🍎  ", 2), token);
                Assert(item.Name == "Café d'avoine 🍎" && item.Version == 1 && !item.IsPurchased,
                    "Names must be trimmed and Unicode preserved.");
                saved = await client.UpdateAsync(item, new ShoppingItemInput("Oats", 3, true), token);
                Assert(saved.Quantity == 3 && saved.IsPurchased && saved.Version == 2, "Update was not persisted.");
                removedId = (await client.AddAsync(new ShoppingItemInput("Temporary", 1), token)).Id;
                ShoppingItem removed = (await client.ReadAllAsync(token)).Single(value => value.Id == removedId);
                await client.DeleteAsync(removed, token);
                Assert((await client.ReadAllAsync(token)).Count == 1, "Deletion failed.");
                Pass("Shopping CRUD, trimmed Unicode names, and purchase state", ref passed);

                foreach (string? name in new[] { null, " ", new string('a', 81), "milk\nwater" })
                {
                    await ExpectErrorAsync("INVALID_NAME", () => client.AddAsync(new ShoppingItemInput(name, 1), token));
                }
                foreach (int quantity in new[] { 0, -1, 1000 })
                {
                    await ExpectErrorAsync("INVALID_QUANTITY", () => client.AddAsync(new ShoppingItemInput("Milk", quantity), token));
                }
                TcpClientService transport = new TcpClientService();
                ResponseMessage missing = await transport.SendAsync(new RequestMessage(ShoppingCommands.Add), port: server.Port, cancellationToken: token);
                Assert(missing.Error == "INVALID_ITEM", "A missing item must be rejected.");
                RequestMessage badPage = new RequestMessage(ShoppingCommands.List);
                badPage.AfterItemId = -1;
                ResponseMessage invalidPage = await transport.SendAsync(badPage, port: server.Port, cancellationToken: token);
                Assert(invalidPage.Error == "INVALID_PAGE", "A negative cursor must be rejected.");
                await ExpectErrorAsync("INVALID_ITEM_VERSION", () => client.DeleteAsync(new ShoppingItem(0, "Milk", 1, false, 0), token));
                Assert((await client.ReadAllAsync(token)).Count == 1, "Invalid requests changed storage.");
                Pass("Server rejects invalid names, quantities, identities, and cursors", ref passed);

                ShoppingListClient secondClient = new ShoppingListClient(port: server.Port);
                Task<string> firstUpdate = TryUpdateAsync(client, saved, "First edit", token);
                Task<string> secondUpdate = TryUpdateAsync(secondClient, saved, "Second edit", token);
                string[] outcomes = await Task.WhenAll(firstUpdate, secondUpdate);
                Assert(outcomes.Count(value => value == "OK") == 1 && outcomes.Count(value => value == "ITEM_CONFLICT") == 1,
                    "Exactly one concurrent edit must succeed.");
                await ExpectErrorAsync("ITEM_CONFLICT", () => client.DeleteAsync(saved, token));
                saved = (await client.ReadAllAsync(token)).Single();
                Assert(saved.Version == 3, "A rejected edit must not increment the version.");
                await ExpectErrorAsync("ITEM_NOT_FOUND", () => client.DeleteAsync(removed, token));
                Pass("Concurrent edits and stale deletes cannot overwrite newer changes", ref passed);

                List<Task<ShoppingItem>> additions = new List<Task<ShoppingItem>>();
                for (int index = 0; index < 24; index++)
                {
                    additions.Add(secondClient.AddAsync(new ShoppingItemInput(new string('é', 78) + index.ToString("D2"), 1), token));
                }
                ShoppingItem[] added = await Task.WhenAll(additions);
                List<ShoppingItem> all = await client.ReadAllAsync(token);
                Assert(all.Count == 25 && added.Select(value => value.Id).Distinct().Count() == 24,
                    "Concurrent additions or pagination lost items.");
                Assert(added.All(value => value.Id > removedId), "Deleted identifiers must not be reused.");
                Pass("Concurrent additions and multi-page maximum-length Unicode responses", ref passed);

                RequestMessage firstPageRequest = new RequestMessage(ShoppingCommands.List);
                ResponseMessage firstPage = await transport.SendAsync(firstPageRequest, port: server.Port, cancellationToken: token);
                ShoppingPage page = firstPage.Page!;
                ShoppingItem lateItem = await client.AddAsync(new ShoppingItemInput("Later addition", 1), token);
                await client.DeleteAsync(page.Items[0], token);
                List<long> remainingIds = new List<long>();
                while (page.NextAfterId != null)
                {
                    RequestMessage next = new RequestMessage(ShoppingCommands.List);
                    next.AfterItemId = page.NextAfterId.Value;
                    next.UpperBoundId = page.UpperBoundId;
                    page = (await transport.SendAsync(next, port: server.Port, cancellationToken: token)).Page!;
                    remainingIds.AddRange(page.Items.Select(value => value.Id));
                }
                Assert(remainingIds.Count == 20 && !remainingIds.Contains(lateItem.Id),
                    "Pagination must survive deletion and exclude additions beyond its initial boundary.");
                saved = await client.UpdateAsync(lateItem, new ShoppingItemInput("Persisted item", 7, true), token);
                Pass("Cursor pagination survives deletion and has a stable upper boundary", ref passed);

                SqliteConnectionStringBuilder settings = new SqliteConnectionStringBuilder();
                settings.DataSource = database;
                settings.Pooling = false;
                using (SqliteConnection locked = new SqliteConnection(settings.ToString()))
                {
                    locked.Open();
                    using SqliteTransaction transaction = locked.BeginTransaction();
                    Task blocked = ExpectErrorAsync("STORAGE_UNAVAILABLE", () => client.AddAsync(new ShoppingItemInput("Locked", 1), token));
                    await transport.PingAsync(port: server.Port, cancellationToken: token).WaitAsync(TimeSpan.FromSeconds(1), token);
                    await blocked;
                }
                await client.AddAsync(new ShoppingItemInput("After lock", 1), token);
                Pass("A locked database returns a bounded error while PING stays responsive", ref passed);
            }

            await using (TestServer restarted = new TestServer(database))
            {
                List<ShoppingItem> items = await restarted.Client.ReadAllAsync(token);
                ShoppingItem restored = items.Single(value => value.Id == saved.Id);
                Assert(restored.Name == saved.Name && restored.Quantity == 7 && restored.IsPurchased
                    && restored.Version == saved.Version && items.All(value => value.Id != removedId),
                    "Restart changed committed data.");
                Pass("Server restart preserves item fields, versions, and deletions", ref passed);
            }
            return passed;
        }

        private static async Task<string> TryUpdateAsync(ShoppingListClient client, ShoppingItem item, string name, CancellationToken token)
        {
            try
            {
                await client.UpdateAsync(item, new ShoppingItemInput(name, 3, true), token);
                return "OK";
            }
            catch (ServerRequestException exception)
            {
                return exception.Code;
            }
        }

        private static async Task ExpectErrorAsync(string code, Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (ServerRequestException exception) when (exception.Code == code)
            {
                return;
            }
            throw new InvalidOperationException($"Expected server error {code}.");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void Pass(string name, ref int passed)
        {
            passed++;
            Console.WriteLine($"PASS: {name}");
        }
    }
}
