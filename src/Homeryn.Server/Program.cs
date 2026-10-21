using System.Net;
using System.Net.Sockets;
using Homeryn.Server.Networking;

namespace Homeryn.Server
{
    internal class Program
    {
        private static async Task Main()
        {
            using CancellationTokenSource shutdown = new CancellationTokenSource();

            // Keep Ctrl+C from terminating the process before connections are closed.
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                shutdown.Cancel();
            };

            try
            {
                TcpServer server = new TcpServer(IPAddress.Loopback, 5000);
                await server.RunAsync(shutdown.Token);
            }
            catch (SocketException exception)
            {
                Console.Error.WriteLine($"Unable to run the TCP server on 127.0.0.1:5000: {exception.Message}");
                Environment.ExitCode = 1;
            }
        }
    }
}
