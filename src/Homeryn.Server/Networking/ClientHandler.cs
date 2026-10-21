using System.Net.Sockets;
using System.Text.Json;
using Homeryn.Shared.Communication;
using Homeryn.Shared.Contracts;

namespace Homeryn.Server.Networking;

internal static class ClientHandler
{
    public static async Task HandleAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                while (!cancellationToken.IsCancellationRequested)
                {
                    RequestMessage? request;
                    try
                    {
                        request = await JsonMessageStream.ReadAsync<RequestMessage>(stream, cancellationToken);
                    }
                    catch (JsonException)
                    {
                        await JsonMessageStream.WriteAsync(
                            stream, new ResponseMessage("ERROR", "INVALID_REQUEST"), cancellationToken);
                        continue;
                    }

                    if (request is null)
                    {
                        return;
                    }

                    ResponseMessage response = string.IsNullOrWhiteSpace(request.Command)
                        ? new ResponseMessage("ERROR", "INVALID_REQUEST")
                        : request.Command == "PING"
                            ? new ResponseMessage("PONG")
                            : new ResponseMessage("ERROR", "UNKNOWN_COMMAND");

                    await JsonMessageStream.WriteAsync(stream, response, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception) when (exception is IOException or SocketException or InvalidDataException)
            {
                Console.Error.WriteLine($"Client connection ended: {exception.Message}");
            }
        }
    }
}
