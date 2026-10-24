using System.Net.Sockets;
using System.Text.Json;
using Homeryn.Shared.Communication;
using Homeryn.Shared.Contracts;

namespace Homeryn.Server.Networking
{
    internal static class ClientHandler
    {
        public static async Task HandleAsync(TcpClient client, RequestDispatcher dispatcher, CancellationToken cancellationToken)
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

                        if (request == null)
                        {
                            return;
                        }

                        ResponseMessage response = await dispatcher.DispatchAsync(request, cancellationToken);

                        await JsonMessageStream.WriteAsync(stream, response, cancellationToken);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // The server is stopping, so no error response is needed.
                }
                catch (IOException exception)
                {
                    Console.Error.WriteLine($"Client connection ended: {exception.Message}");
                }
                catch (SocketException exception)
                {
                    Console.Error.WriteLine($"Client connection ended: {exception.Message}");
                }
                catch (InvalidDataException exception)
                {
                    Console.Error.WriteLine($"Client connection ended: {exception.Message}");
                }
            }
        }
    }
}
