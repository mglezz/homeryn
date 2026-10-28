using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Homeryn.Desktop.Networking;
using Homeryn.Shared.Communication;
using Homeryn.Shared.Contracts;
using Homeryn.Shared.Shopping;
using Homeryn.Shared.Chores;

namespace Homeryn.IntegrationTests
{
    internal static class ClientResponseTests
    {
        public static async Task<int> RunAsync(CancellationToken token)
        {
            int passed = 0;
            foreach (bool chores in new[] { false, true })
            {
                string kind = chores ? "Chore" : "Shopping";
                JsonObject item = CreateItem(chores, 1);
                foreach (string field in item.Select(property => property.Key).ToArray())
                {
                    JsonObject incomplete = (JsonObject)item.DeepClone();
                    incomplete.Remove(field);
                    await RejectAsync(chores, Page(chores, incomplete), token);
                }
                Console.WriteLine($"PASS: {kind} client rejects missing stored record fields");
                passed++;

                foreach (string field in new[] { "items", "upperBoundId", "nextAfterId" })
                {
                    JsonObject incomplete = Page(chores, item.DeepClone());
                    GetPage(chores, incomplete).Remove(field);
                    await RejectAsync(chores, incomplete, token);
                }
                JsonObject invalid = Page(chores, null);
                await RejectAsync(chores, invalid, token);
                invalid = Page(chores, item.DeepClone());
                GetPage(chores, invalid)["nextAfterId"] = 1;
                await RejectAsync(chores, invalid, token);
                invalid = Page(chores, item.DeepClone());
                GetPage(chores, invalid)["upperBoundId"] = 0;
                await RejectAsync(chores, invalid, token);
                invalid = Page(chores, item.DeepClone());
                GetPage(chores, invalid)["items"]!.AsArray().Add(item.DeepClone());
                await RejectAsync(chores, invalid, token);
                JsonObject invalidName = (JsonObject)item.DeepClone();
                invalidName["name"] = "Invalid\nname";
                await RejectAsync(chores, Page(chores, invalidName), token);
                Console.WriteLine($"PASS: {kind} client rejects incomplete pages, null records, invalid names and cursors");
                passed++;

                JsonObject added = (JsonObject)item.DeepClone();
                added["version"] = 2;
                JsonObject response = new JsonObject { ["status"] = "OK", [chores ? "chore" : "item"] = added };
                await WithResponseAsync(response, async port =>
                {
                    await ExpectJsonErrorAsync(async () =>
                    {
                        if (chores)
                        {
                            await new ChoresClient(port: port).AddAsync(new ChoreInput("Test"), token);
                        }
                        else
                        {
                            await new ShoppingListClient(port: port).AddAsync(new ShoppingItemInput("Test", 1), token);
                        }
                    });
                }, token);
                Console.WriteLine($"PASS: {kind} client rejects an unexpected creation version");
                passed++;

                await CheckReadDeadlineAsync(chores, token);
                Console.WriteLine($"PASS: {kind} pagination shares one five-second operation deadline");
                passed++;
            }
            return passed;
        }

        private static JsonObject CreateItem(bool chores, long id)
        {
            JsonObject item = new JsonObject { ["id"] = id, ["name"] = "Test", ["version"] = 1 };
            if (chores)
            {
                item["assignee"] = null;
                item["dueDate"] = null;
                item["isCompleted"] = false;
            }
            else
            {
                item["quantity"] = 1;
                item["isPurchased"] = false;
            }
            return item;
        }

        private static JsonObject Page(bool chores, JsonNode? item)
        {
            return new JsonObject
            {
                ["status"] = "OK",
                [chores ? "chorePage" : "page"] = new JsonObject
                {
                    ["items"] = new JsonArray(item), ["upperBoundId"] = 1, ["nextAfterId"] = null
                }
            };
        }

        private static JsonObject GetPage(bool chores, JsonObject response)
        {
            return response[chores ? "chorePage" : "page"]!.AsObject();
        }

        private static Task ReadAsync(bool chores, int port, CancellationToken token)
        {
            if (chores)
            {
                return new ChoresClient(port: port).ReadAllAsync(token);
            }
            return new ShoppingListClient(port: port).ReadAllAsync(token);
        }

        private static Task RejectAsync(bool chores, JsonObject response, CancellationToken token)
        {
            return WithResponseAsync(response, port => ExpectJsonErrorAsync(() => ReadAsync(chores, port, token)), token);
        }

        private static async Task WithResponseAsync(JsonObject response, Func<int, Task> operation, CancellationToken token)
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            Task result = operation(((IPEndPoint)listener.LocalEndpoint).Port);
            using TcpClient connection = await listener.AcceptTcpClientAsync(token);
            await JsonMessageStream.ReadAsync<RequestMessage>(connection.GetStream(), token);
            await WriteResponseAsync(connection, response, token);
            await result;
        }

        private static async Task CheckReadDeadlineAsync(bool chores, CancellationToken token)
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            Task read = ReadAsync(chores, ((IPEndPoint)listener.LocalEndpoint).Port, token);
            using (TcpClient first = await listener.AcceptTcpClientAsync(token))
            {
                await JsonMessageStream.ReadAsync<RequestMessage>(first.GetStream(), token);
                await Task.Delay(TimeSpan.FromSeconds(3), token);
                JsonObject response = Page(chores, CreateItem(chores, 1));
                GetPage(chores, response)["upperBoundId"] = 2;
                GetPage(chores, response)["nextAfterId"] = 1;
                await WriteResponseAsync(first, response, token);
            }
            using TcpClient second = await listener.AcceptTcpClientAsync(token);
            RequestMessage? next = await JsonMessageStream.ReadAsync<RequestMessage>(second.GetStream(), token);
            long? cursor = chores ? next?.AfterChoreId : next?.AfterItemId;
            if (cursor != 1 || next?.UpperBoundId != 2)
            {
                throw new InvalidOperationException("The next page request lost its cursor or boundary.");
            }
            // The first page consumed three seconds. A fresh per-page timeout would take five more.
            try
            {
                await read.WaitAsync(TimeSpan.FromSeconds(3.5), token);
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                return;
            }
            throw new InvalidOperationException("Pagination did not honor the overall deadline.");
        }

        private static async Task WriteResponseAsync(TcpClient connection, JsonObject response, CancellationToken token)
        {
            byte[] payload = Encoding.UTF8.GetBytes(response.ToJsonString());
            byte[] header = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);
            await connection.GetStream().WriteAsync(header, token);
            await connection.GetStream().WriteAsync(payload, token);
        }

        private static async Task ExpectJsonErrorAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (JsonException)
            {
                return;
            }
            throw new InvalidOperationException("The client accepted an invalid server response.");
        }
    }
}
