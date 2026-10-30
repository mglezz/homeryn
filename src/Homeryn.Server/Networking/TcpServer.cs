using System.Net;
using System.Net.Sockets;

namespace Homeryn.Server.Networking
{
    public class TcpServer
    {
        private readonly TcpListener listener;
        private readonly RequestDispatcher dispatcher;
        private readonly int maximumClients;
        private readonly TimeSpan requestTimeout;

        public TcpServer(IPAddress address, int port, RequestDispatcher dispatcher,
            int maximumClients = 64, TimeSpan? requestTimeout = null)
        {
            if (!IPAddress.IsLoopback(address))
            {
                throw new ArgumentException("Only loopback addresses are supported.", nameof(address));
            }
            if (maximumClients < 1 || maximumClients > 64)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumClients));
            }
            this.requestTimeout = requestTimeout ?? TimeSpan.FromSeconds(10);
            if (this.requestTimeout <= TimeSpan.Zero || this.requestTimeout > TimeSpan.FromSeconds(10))
            {
                throw new ArgumentOutOfRangeException(nameof(requestTimeout));
            }
            this.maximumClients = maximumClients;
            listener = new TcpListener(address, port);
            this.dispatcher = dispatcher;
        }

        public IPEndPoint LocalEndpoint
        {
            get
            {
                return (IPEndPoint)listener.LocalEndpoint;
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using CancellationTokenSource shutdown = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            List<Task> clientTasks = new List<Task>();
            listener.Start();

            try
            {
                Console.WriteLine($"Homeryn server listening on {LocalEndpoint}. Press Ctrl+C to stop.");
                while (!shutdown.IsCancellationRequested)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(shutdown.Token);
                    // Remove finished handlers so the list does not keep growing.
                    for (int index = clientTasks.Count - 1; index >= 0; index--)
                    {
                        if (clientTasks[index].IsCompletedSuccessfully)
                        {
                            clientTasks.RemoveAt(index);
                        }
                    }

                    if (clientTasks.Count >= maximumClients)
                    {
                        client.Dispose();
                        continue;
                    }
                    Task clientTask = ClientHandler.HandleAsync(client, dispatcher, requestTimeout, shutdown.Token);
                    clientTasks.Add(clientTask);
                }
            }
            catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
            {
                // Cancellation is the expected result when the server stops.
            }
            finally
            {
                listener.Stop();
                await shutdown.CancelAsync();
                await Task.WhenAll(clientTasks);
            }
        }
    }
}
