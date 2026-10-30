using Homeryn.Server.Chores;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Homeryn.Desktop.Networking;
using Homeryn.Server.Networking;
using Homeryn.Server.Persistence;
using Homeryn.Server.Shopping;
using Homeryn.IntegrationTests;

using CancellationTokenSource deadline = new CancellationTokenSource(TimeSpan.FromSeconds(90));
using CancellationTokenSource shutdown = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token);
CancellationToken token = deadline.Token;
string testDirectory = Path.Combine(Path.GetTempPath(), "Homeryn.Tests", Guid.NewGuid().ToString("N"));
SqliteShoppingItemRepository repository = new SqliteShoppingItemRepository(Path.Combine(testDirectory, "protocol.db"));
new SqliteDatabase(Path.Combine(testDirectory, "protocol.db")).Initialize();
using RequestDispatcher dispatcher = new RequestDispatcher(new ShoppingListService(repository),
    new ChoresService(new SqliteChoreRepository(Path.Combine(testDirectory, "protocol.db"))));
TcpServer server = new TcpServer(IPAddress.Loopback, 0, dispatcher);
Task serverTask = server.RunAsync(shutdown.Token);
int port = server.LocalEndpoint.Port;
int passed = 0;

try
{
    await CheckAsync("Desktop client receives PONG", () =>
        new TcpClientService().PingAsync(port: port, cancellationToken: token));

    await CheckAsync("Fragmented header and payload", async () =>
    {
        using TcpClient client = await ConnectAsync();
        byte[] frame = Frame("""{"command":"PING"}""");
        foreach (byte value in frame)
        {
            await client.GetStream().WriteAsync(new byte[] { value }, token);
            await Task.Delay(1, token);
        }
        await ExpectAsync(client, "PONG");
    });

    await CheckAsync("Coalesced requests on one connection", async () =>
    {
        using TcpClient client = await ConnectAsync();
        byte[] frame = Frame("""{"command":"PING"}""");
        byte[] combinedFrames = new byte[frame.Length * 2];
        frame.CopyTo(combinedFrames, 0);
        frame.CopyTo(combinedFrames, frame.Length);
        await client.GetStream().WriteAsync(combinedFrames, token);
        await ExpectAsync(client, "PONG");
        await ExpectAsync(client, "PONG");
    });

    await CheckAsync("Invalid requests do not poison a connection", async () =>
    {
        using TcpClient client = await ConnectAsync();
        foreach (string json in new[] { "{", "null", "{}", "[]", """{"command":1}""", """{"command":""}""" })
        {
            await client.GetStream().WriteAsync(Frame(json), token);
            await ExpectAsync(client, "ERROR", "INVALID_REQUEST");
        }
        await client.GetStream().WriteAsync(Frame("""{"command":"UNKNOWN"}"""), token);
        await ExpectAsync(client, "ERROR", "UNKNOWN_COMMAND");
        await client.GetStream().WriteAsync(Frame("""{"command":"PING"}"""), token);
        await ExpectAsync(client, "PONG");
    });

    await CheckAsync("Invalid frame lengths close only the offending connection", async () =>
    {
        foreach (int length in new[] { 0, -1, 4097 })
        {
            using TcpClient client = await ConnectAsync();
            byte[] header = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(header, length);
            await client.GetStream().WriteAsync(header, token);
            await ExpectClosedAsync(client.GetStream());
        }
        await new TcpClientService().PingAsync(port: port, cancellationToken: token);
    });

    await CheckAsync("Truncated frames do not stop the server", async () =>
    {
        foreach (byte[] bytes in new[] { new byte[] { 0, 0 }, new byte[] { 0, 0, 0, 10, 123 } })
        {
            using TcpClient client = await ConnectAsync();
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(bytes, token);
            client.Client.Shutdown(SocketShutdown.Send);
            await ExpectClosedAsync(stream);
        }
        await new TcpClientService().PingAsync(port: port, cancellationToken: token);
    });

    await CheckAsync("32 active clients progress while another client is idle", async () =>
    {
        using TcpClient idle = await ConnectAsync();
        List<TcpClient> clients = new List<TcpClient>();
        try
        {
            for (int index = 0; index < 32; index++)
            {
                clients.Add(await ConnectAsync());
            }
            List<Task> requests = new List<Task>();
            foreach (TcpClient client in clients)
            {
                requests.Add(SendThreePingsAsync(client));
            }
            await Task.WhenAll(requests);
        }
        finally
        {
            foreach (TcpClient client in clients)
            {
                client.Dispose();
            }
        }
    });

    await CheckAsync("Desktop client honors caller cancellation", async () =>
    {
        using CancellationTokenSource cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await ExpectExceptionAsync<OperationCanceledException>(() =>
            new TcpClientService().PingAsync(port: port, cancellationToken: cancelled.Token));
    });

    await CheckAsync("Desktop client reports a refused connection", async () =>
    {
        using Socket reserved = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reserved.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int unavailablePort = ((IPEndPoint)reserved.LocalEndPoint!).Port;
        await ExpectExceptionAsync<SocketException>(() =>
            new TcpClientService().PingAsync(port: unavailablePort, cancellationToken: token));
    });

    await CheckAsync("Desktop client rejects an oversized response frame", async () =>
    {
        using TcpListener fakeServer = new TcpListener(IPAddress.Loopback, 0);
        fakeServer.Start();
        Task ping = new TcpClientService().PingAsync(
            port: ((IPEndPoint)fakeServer.LocalEndpoint).Port, cancellationToken: token);
        using TcpClient client = await fakeServer.AcceptTcpClientAsync(token);
        using JsonDocument request = await ReadJsonAsync(client);
        byte[] header = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(header, 4097);
        await client.GetStream().WriteAsync(header, token);
        await ExpectExceptionAsync<InvalidDataException>(() => ping);
    });

    await CheckAsync("Desktop client rejects an invalid response", async () =>
    {
        using TcpListener fakeServer = new TcpListener(IPAddress.Loopback, 0);
        fakeServer.Start();
        Task ping = new TcpClientService().PingAsync(
            port: ((IPEndPoint)fakeServer.LocalEndpoint).Port, cancellationToken: token);
        using TcpClient client = await fakeServer.AcceptTcpClientAsync(token);
        using JsonDocument request = await ReadJsonAsync(client);
        await client.GetStream().WriteAsync(Frame("""{"status":"UNEXPECTED"}"""), token);
        await ExpectExceptionAsync<JsonException>(() => ping);
    });

    await CheckAsync("Desktop client times out when a server does not respond", async () =>
    {
        using TcpListener fakeServer = new TcpListener(IPAddress.Loopback, 0);
        fakeServer.Start();
        Task ping = new TcpClientService().PingAsync(
            port: ((IPEndPoint)fakeServer.LocalEndpoint).Port, cancellationToken: token);
        using TcpClient client = await fakeServer.AcceptTcpClientAsync(token);
        using JsonDocument request = await ReadJsonAsync(client);
        await ExpectExceptionAsync<OperationCanceledException>(() => ping.WaitAsync(TimeSpan.FromSeconds(8)));
        await ExpectClosedAsync(client.GetStream());
    });

    await CheckAsync("Shutdown cancels idle and partial reads and releases the port", async () =>
    {
        using TcpClient idle = await ConnectAsync();
        using TcpClient partial = await ConnectAsync();
        await partial.GetStream().WriteAsync(new byte[] { 0, 0, 0, 10, 123 }, token);
        await shutdown.CancelAsync();
        await serverTask.WaitAsync(TimeSpan.FromSeconds(3));
        using TcpListener replacement = new TcpListener(IPAddress.Loopback, port);
        replacement.Start();
    });

    passed += await ShoppingListTests.RunAsync(testDirectory, token);
    passed += await ChoresTests.RunAsync(testDirectory, token);
    passed += await DatabaseMigrationTests.RunAsync(testDirectory, token);
    passed += await ClientResponseTests.RunAsync(token);
    passed += await DesktopEditorTests.RunAsync(testDirectory, token);
    passed += await BackupTests.RunAsync(testDirectory, token);
    passed += await SecurityTests.RunAsync(testDirectory, token);
    Console.WriteLine($"PASS: {passed} integration checks.");
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    Environment.ExitCode = 1;
}
finally
{
    await shutdown.CancelAsync();
    await serverTask.WaitAsync(TimeSpan.FromSeconds(3));
    Directory.Delete(testDirectory, recursive: true);
}

async Task SendThreePingsAsync(TcpClient client)
{
    for (int request = 0; request < 3; request++)
    {
        await client.GetStream().WriteAsync(Frame("""{"command":"PING"}"""), token);
        await ExpectAsync(client, "PONG");
    }
}

async Task CheckAsync(string name, Func<Task> check)
{
    await check();
    passed++;
    Console.WriteLine($"PASS: {name}");
}

async Task<TcpClient> ConnectAsync()
{
    TcpClient client = new TcpClient();
    try
    {
        await client.ConnectAsync(IPAddress.Loopback, port, token);
        return client;
    }
    catch
    {
        client.Dispose();
        throw;
    }
}

static byte[] Frame(string json)
{
    byte[] payload = Encoding.UTF8.GetBytes(json);
    byte[] frame = new byte[4 + payload.Length];
    BinaryPrimitives.WriteInt32BigEndian(frame, payload.Length);
    payload.CopyTo(frame, 4);
    return frame;
}

async Task<JsonDocument> ReadJsonAsync(TcpClient client)
{
    byte[] header = new byte[4];
    await client.GetStream().ReadExactlyAsync(header, token);
    int length = BinaryPrimitives.ReadInt32BigEndian(header);
    if (length <= 0 || length > 4096)
    {
        throw new InvalidDataException($"Unexpected response length: {length}");
    }
    byte[] payload = new byte[length];
    await client.GetStream().ReadExactlyAsync(payload, token);
    return JsonDocument.Parse(payload);
}

async Task ExpectAsync(TcpClient client, string status, string? error = null)
{
    using JsonDocument response = await ReadJsonAsync(client);
    if (response.RootElement.GetProperty("status").GetString() != status
        || response.RootElement.GetProperty("error").GetString() != error)
    {
        throw new InvalidDataException($"Unexpected response: {response.RootElement}");
    }
}

async Task ExpectClosedAsync(NetworkStream stream)
{
    try
    {
        int count = await stream.ReadAsync(new byte[1], token);
        if (count != 0)
        {
            throw new InvalidDataException("The connection remained open.");
        }
    }
    catch (IOException)
    {
        // A connection reset is also a valid close after a rejected frame.
    }
}

static async Task ExpectExceptionAsync<TException>(Func<Task> action) where TException : Exception
{
    try
    {
        await action();
    }
    catch (TException)
    {
        return;
    }
    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}
