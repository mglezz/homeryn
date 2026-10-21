using System.Net;
using System.Net.Sockets;
using Homeryn.Server.Networking;

using CancellationTokenSource shutdown = new();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

try
{
    await new TcpServer(IPAddress.Loopback, 5000).RunAsync(shutdown.Token);
}
catch (SocketException exception)
{
    Console.Error.WriteLine($"Unable to run the TCP server on 127.0.0.1:5000: {exception.Message}");
    Environment.ExitCode = 1;
}