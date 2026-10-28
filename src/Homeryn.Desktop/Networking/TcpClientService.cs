using System.Net.Sockets;
using System.Text.Json;
using Homeryn.Shared.Communication;
using Homeryn.Shared.Contracts;

namespace Homeryn.Desktop.Networking
{
    public class TcpClientService
    {
        public static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(5);

        public async Task PingAsync(
            string host = "127.0.0.1",
            int port = 5000,
            CancellationToken cancellationToken = default)
        {
            ResponseMessage response = await SendAsync(new RequestMessage("PING"), host, port, cancellationToken);
            if (response.Status != "PONG" || response.Error != null)
            {
                throw new JsonException("The server did not return a valid PONG response.");
            }
        }

        public async Task<ResponseMessage> SendAsync(
            RequestMessage request,
            string host = "127.0.0.1",
            int port = 5000,
            CancellationToken cancellationToken = default)
        {
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(OperationTimeout);

            using TcpClient client = new TcpClient();
            await client.ConnectAsync(host, port, timeout.Token);
            NetworkStream stream = client.GetStream();

            await JsonMessageStream.WriteAsync(stream, request, timeout.Token);
            ResponseMessage? response = await JsonMessageStream.ReadAsync<ResponseMessage>(stream, timeout.Token);
            if (response == null)
            {
                throw new JsonException("The server closed the connection without a response.");
            }
            return response;
        }
    }
}
