using Homeryn.Desktop.Networking;
using Homeryn.Shared.Contracts;
using Homeryn.Shared.Chores;
using Microsoft.Data.Sqlite;

namespace Homeryn.IntegrationTests
{
    internal static class ChoresTests
    {
        public static async Task<int> RunAsync(string directory, CancellationToken token)
        {
            int passed = 0;
            string database = Path.Combine(directory, "chore.db");
            Chore saved;
            long removedId;
            await using (TestServer server = new TestServer(database))
            {
                ChoresClient client = new ChoresClient(port: server.Port);
                Assert((await client.ReadAllAsync(token)).Count == 0, "New list must be empty.");
                Chore item = await client.AddAsync(new ChoreInput("  Limpiar cocina 🍎  ", "  Ana  ", new DateOnly(2026, 10, 31)), token);
                Assert(item.Name == "Limpiar cocina 🍎" && item.Version == 1 && !item.IsCompleted && item.Assignee == "Ana" && item.DueDate == new DateOnly(2026, 10, 31),
                    "Names must be trimmed and Unicode preserved.");
                saved = await client.UpdateAsync(item, new ChoreInput("Kitchen", "Alex", new DateOnly(2026, 11, 1), true), token);
                Assert(saved.Assignee == "Alex" && saved.DueDate == new DateOnly(2026, 11, 1) && saved.IsCompleted && saved.Version == 2, "Update was not persisted.");
                Chore reopened = await client.UpdateAsync(saved, new ChoreInput(saved.Name, "  "), token);
                Assert(!reopened.IsCompleted && reopened.Assignee == null && reopened.DueDate == null,
                    "Reopening, unassigning and clearing a due date failed.");
                saved = await client.UpdateAsync(reopened, new ChoreInput("Kitchen", "Alex", new DateOnly(2026, 11, 1), true), token);
                removedId = (await client.AddAsync(new ChoreInput("Temporary"), token)).Id;
                Chore removed = (await client.ReadAllAsync(token)).Single(value => value.Id == removedId);
                await client.DeleteAsync(removed, token);
                Assert((await client.ReadAllAsync(token)).Count == 1, "Deletion failed.");
                Pass("Chores CRUD, trimmed Unicode names, and completion state", ref passed);

                foreach (string? name in new[] { null, " ", new string('a', 81), "milk\nwater" })
                {
                    await ExpectErrorAsync("INVALID_NAME", () => client.AddAsync(new ChoreInput(name), token));
                }
                foreach (string assignee in new[] { new string('a', 61), "Ana\nAlex" })
                {
                    await ExpectErrorAsync("INVALID_ASSIGNEE", () => client.AddAsync(new ChoreInput("Kitchen", assignee), token));
                }
                foreach (DateOnly date in new[] { DateOnly.MinValue, new DateOnly(1752, 12, 31), DateOnly.MaxValue })
                {
                    await ExpectErrorAsync("INVALID_DUE_DATE", () => client.AddAsync(new ChoreInput("Kitchen", dueDate: date), token));
                }
                TcpClientService transport = new TcpClientService();
                ResponseMessage missing = await transport.SendAsync(new RequestMessage(ChoreCommands.Add), port: server.Port, cancellationToken: token);
                Assert(missing.Error == "INVALID_CHORE", "A missing item must be rejected.");
                RequestMessage badPage = new RequestMessage(ChoreCommands.List);
                badPage.AfterChoreId = -1;
                ResponseMessage invalidPage = await transport.SendAsync(badPage, port: server.Port, cancellationToken: token);
                Assert(invalidPage.Error == "INVALID_PAGE", "A negative cursor must be rejected.");
                await ExpectErrorAsync("INVALID_CHORE_VERSION", () => client.DeleteAsync(new Chore(0, "Kitchen", null, null, false, 0), token));
                Assert((await client.ReadAllAsync(token)).Count == 1, "Invalid requests changed storage.");
                Pass("Server rejects invalid names, assignees, dates, identities, and cursors", ref passed);

                ChoresClient secondClient = new ChoresClient(port: server.Port);
                Task<string> firstUpdate = TryUpdateAsync(client, saved, "First edit", token);
                Task<string> secondUpdate = TryUpdateAsync(secondClient, saved, "Second edit", token);
                string[] outcomes = await Task.WhenAll(firstUpdate, secondUpdate);
                Assert(outcomes.Count(value => value == "OK") == 1 && outcomes.Count(value => value == "CHORE_CONFLICT") == 1,
                    "Exactly one concurrent edit must succeed.");
                await ExpectErrorAsync("CHORE_CONFLICT", () => client.DeleteAsync(saved, token));
                saved = (await client.ReadAllAsync(token)).Single();
                Assert(saved.Version == 5, "A rejected edit must not increment the version.");
                await ExpectErrorAsync("CHORE_NOT_FOUND", () => client.DeleteAsync(removed, token));
                Pass("Concurrent edits and stale deletes cannot overwrite newer changes", ref passed);

                List<Task<Chore>> additions = new List<Task<Chore>>();
                for (int index = 0; index < 24; index++)
                {
                    additions.Add(secondClient.AddAsync(new ChoreInput(new string('é', 78) + index.ToString("D2"), new string('ñ', 60), new DateOnly(9998, 12, 31)), token));
                }
                Chore[] added = await Task.WhenAll(additions);
                List<Chore> all = await client.ReadAllAsync(token);
                Assert(all.Count == 25 && added.Select(value => value.Id).Distinct().Count() == 24,
                    "Concurrent additions or pagination lost items.");
                Assert(added.All(value => value.Id > removedId), "Deleted identifiers must not be reused.");
                Pass("Concurrent additions and multi-page maximum-length Unicode responses", ref passed);

                RequestMessage firstPageRequest = new RequestMessage(ChoreCommands.List);
                ResponseMessage firstPage = await transport.SendAsync(firstPageRequest, port: server.Port, cancellationToken: token);
                ChorePage page = firstPage.ChorePage!;
                Chore lateItem = await client.AddAsync(new ChoreInput("Later addition"), token);
                await client.DeleteAsync(page.Items[0], token);
                List<long> remainingIds = new List<long>();
                while (page.NextAfterId != null)
                {
                    RequestMessage next = new RequestMessage(ChoreCommands.List);
                    next.AfterChoreId = page.NextAfterId.Value;
                    next.UpperBoundId = page.UpperBoundId;
                    page = (await transport.SendAsync(next, port: server.Port, cancellationToken: token)).ChorePage!;
                    remainingIds.AddRange(page.Items.Select(value => value.Id));
                }
                Assert(remainingIds.Count == 25 - ChorePage.PageSize && !remainingIds.Contains(lateItem.Id),
                    "Pagination must survive deletion and exclude additions beyond its initial boundary.");
                saved = await client.UpdateAsync(lateItem, new ChoreInput("Persisted chore", "Sam", new DateOnly(2026, 12, 1), true), token);
                Pass("Cursor pagination survives deletion and has a stable upper boundary", ref passed);

                SqliteConnectionStringBuilder settings = new SqliteConnectionStringBuilder();
                settings.DataSource = database;
                settings.Pooling = false;
                using (SqliteConnection locked = new SqliteConnection(settings.ToString()))
                {
                    locked.Open();
                    using SqliteTransaction transaction = locked.BeginTransaction();
                    Task blocked = ExpectErrorAsync("STORAGE_UNAVAILABLE", () => client.AddAsync(new ChoreInput("Locked"), token));
                    await transport.PingAsync(port: server.Port, cancellationToken: token).WaitAsync(TimeSpan.FromSeconds(1), token);
                    await blocked;
                }
                await client.AddAsync(new ChoreInput("After lock"), token);
                Pass("A locked database returns a bounded error while PING stays responsive", ref passed);
            }

            await using (TestServer restarted = new TestServer(database))
            {
                List<Chore> items = await new ChoresClient(port: restarted.Port).ReadAllAsync(token);
                Chore restored = items.Single(value => value.Id == saved.Id);
                Assert(restored.Name == saved.Name && restored.Assignee == "Sam" && restored.DueDate == new DateOnly(2026, 12, 1) && restored.IsCompleted
                    && restored.Version == saved.Version && items.All(value => value.Id != removedId),
                    "Restart changed committed data.");
                Pass("Server restart preserves item fields, versions, and deletions", ref passed);
            }
            return passed;
        }

        private static async Task<string> TryUpdateAsync(ChoresClient client, Chore item, string name, CancellationToken token)
        {
            try
            {
                await client.UpdateAsync(item, new ChoreInput(name, "Alex", new DateOnly(2026, 11, 1), true), token);
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
