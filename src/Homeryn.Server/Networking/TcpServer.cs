using System.Net;
using System.Net.Sockets;

namespace Homeryn.Server.Networking;

public sealed class TcpServer(IPAddress address, int port)
{
    private readonly TcpListener listener = new(address, port);

    public IPEndPoint LocalEndpoint => (IPEndPoint)listener.LocalEndpoint;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using CancellationTokenSource shutdown = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        HashSet<Task> clients = [];
        listener.Start();

        try
        {
            Console.WriteLine($"Homeryn server listening on {LocalEndpoint}. Press Ctrl+C to stop.");
            while (!shutdown.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(shutdown.Token);
                clients.RemoveWhere(task => task.IsCompletedSuccessfully);
                clients.Add(ClientHandler.HandleAsync(client, shutdown.Token));
            }
        }
        catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
        {
        }
        finally
        {
            listener.Stop();
            await shutdown.CancelAsync();
            await Task.WhenAll(clients);
        }
    }
}