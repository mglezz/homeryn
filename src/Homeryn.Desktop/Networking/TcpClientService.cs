using System.Net.Sockets;
using System.Text.Json;
using Homeryn.Shared.Communication;
using Homeryn.Shared.Contracts;

namespace Homeryn.Desktop.Networking
{
    public class TcpClientService
    {
        public async Task PingAsync(
            string host = "127.0.0.1",
            int port = 5000,
            CancellationToken cancellationToken = default)
        {
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));

            using TcpClient client = new TcpClient();
            await client.ConnectAsync(host, port, timeout.Token);
            NetworkStream stream = client.GetStream();

            await JsonMessageStream.WriteAsync(stream, new RequestMessage("PING"), timeout.Token);
            ResponseMessage? response = await JsonMessageStream.ReadAsync<ResponseMessage>(stream, timeout.Token);
            if (response == null || response.Status != "PONG" || response.Error != null)
            {
                throw new JsonException("The server did not return a valid PONG response.");
            }
        }
    }
}
