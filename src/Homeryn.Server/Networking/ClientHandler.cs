using System.Net.Sockets;
using System.Text.Json;
using Homeryn.Shared.Communication;
using Homeryn.Shared.Contracts;

namespace Homeryn.Server.Networking
{
    internal static class ClientHandler
    {
        public static async Task HandleAsync(TcpClient client, RequestDispatcher dispatcher,
            TimeSpan requestTimeout, CancellationToken cancellationToken)
        {
            using (client)
            {
                using CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                try
                {
                    NetworkStream stream = client.GetStream();
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // Bound the whole exchange, including partial frames, storage waits, and writes.
                        // Receiving another byte does not restart this deadline.
                        deadline.CancelAfter(requestTimeout);
                        RequestMessage? request;
                        try
                        {
                            request = await JsonMessageStream.ReadAsync<RequestMessage>(stream, deadline.Token);
                        }
                        catch (JsonException)
                        {
                            await JsonMessageStream.WriteAsync(
                                stream, new ResponseMessage("ERROR", "INVALID_REQUEST"), deadline.Token);
                            continue;
                        }

                        if (request == null)
                        {
                            return;
                        }

                        ResponseMessage response = await dispatcher.DispatchAsync(request, deadline.Token);

                        await JsonMessageStream.WriteAsync(stream, response, deadline.Token);
                    }
                }
                catch (OperationCanceledException) when (deadline.IsCancellationRequested)
                {
                    // Shutdown or an expired request closes only this connection.
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
