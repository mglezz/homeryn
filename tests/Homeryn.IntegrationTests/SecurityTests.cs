using System.Net;
using System.Net.Sockets;
using Homeryn.Desktop.Networking;
using Homeryn.Server.Networking;
using Homeryn.Shared.Communication;
using Homeryn.Shared.Contracts;

namespace Homeryn.IntegrationTests
{
    internal static class SecurityTests
    {
        public static async Task<int> RunAsync(string directory, CancellationToken token)
        {
            foreach (IPAddress address in new[] { IPAddress.Any, IPAddress.IPv6Any, IPAddress.Parse("192.0.2.1") })
            {
                try
                {
                    _ = new TcpServer(address, 0, null!);
                    throw new InvalidOperationException("A non-loopback listener was accepted.");
                }
                catch (ArgumentException)
                {
                }
            }
            Console.WriteLine("PASS: Server refuses non-loopback listeners");

            await using (TestServer server = new TestServer(Path.Combine(directory, "deadlines.db"),
                requestTimeout: TimeSpan.FromMilliseconds(500)))
            {
                foreach (byte[] fragment in new[] { Array.Empty<byte>(), new byte[] { 0, 0 },
                    new byte[] { 0, 0, 0, 10, 123 } })
                {
                    using TcpClient client = new TcpClient();
                    await client.ConnectAsync(IPAddress.Loopback, server.Port, token);
                    await client.GetStream().WriteAsync(fragment, token);
                    await ExpectClosedAsync(client.GetStream(), token);
                    await new TcpClientService().PingAsync(port: server.Port, cancellationToken: token);
                }
            }
            Console.WriteLine("PASS: Idle and incomplete frames expire without stopping the server");

            await using (TestServer server = new TestServer(Path.Combine(directory, "capacity.db"), maximumClients: 2))
            {
                using TcpClient first = new TcpClient();
                using TcpClient second = new TcpClient();
                await first.ConnectAsync(IPAddress.Loopback, server.Port, token);
                await PingAsync(first, token);
                await second.ConnectAsync(IPAddress.Loopback, server.Port, token);
                await PingAsync(second, token);
                using TcpClient excess = new TcpClient();
                await excess.ConnectAsync(IPAddress.Loopback, server.Port, token);
                await ExpectClosedAsync(excess.GetStream(), token);
                await PingAsync(first, token);
                // EOF acknowledges that the handler has disposed its accepted socket.
                NetworkStream firstStream = first.GetStream();
                first.Client.Shutdown(SocketShutdown.Send);
                await ExpectClosedAsync(firstStream, token);
                await new TcpClientService().PingAsync(port: server.Port, cancellationToken: token);
                await PingAsync(second, token);
            }
            Console.WriteLine("PASS: Connection capacity rejects excess clients and recovers released slots");
            return 3;
        }

        private static async Task PingAsync(TcpClient client, CancellationToken token)
        {
            await JsonMessageStream.WriteAsync(client.GetStream(), new RequestMessage("PING"), token);
            ResponseMessage? response = await JsonMessageStream.ReadAsync<ResponseMessage>(client.GetStream(), token);
            if (response?.Status != "PONG")
            {
                throw new InvalidDataException("An admitted client stopped responding.");
            }
        }

        private static async Task ExpectClosedAsync(NetworkStream stream, CancellationToken token)
        {
            using CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
            deadline.CancelAfter(TimeSpan.FromSeconds(3));
            try
            {
                if (await stream.ReadAsync(new byte[1], deadline.Token) != 0)
                {
                    throw new InvalidOperationException("Expected the connection to close.");
                }
            }
            catch (IOException)
            {
                // Reset and orderly EOF both mean the connection was rejected or expired.
            }
        }
    }
}
