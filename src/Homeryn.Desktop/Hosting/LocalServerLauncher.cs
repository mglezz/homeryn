using System.Diagnostics;
using System.Net.Sockets;
using Homeryn.Desktop.Networking;

namespace Homeryn.Desktop.Hosting
{
    public class LocalServerLauncher
    {
        private readonly TcpClientService client = new TcpClientService();

        public string? FindExecutable()
        {
            string bundledServer = Path.Combine(AppContext.BaseDirectory, "server", "Homeryn.Server.exe");
            if (File.Exists(bundledServer))
            {
                return bundledServer;
            }
            DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
            string configuration = directory.Parent?.Name ?? "Debug";
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Homeryn.slnx")))
                {
                    string executable = Path.Combine(directory.FullName, "src", "Homeryn.Server", "bin",
                        configuration, "net10.0", "Homeryn.Server.exe");
                    if (File.Exists(executable))
                    {
                        return executable;
                    }
                    return null;
                }
                directory = directory.Parent;
            }
            return null;
        }

        public async Task<bool> IsRunningAsync(CancellationToken cancellationToken)
        {
            try
            {
                await client.PingAsync(cancellationToken: cancellationToken);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public async Task StartAsync(string executable, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.GetFullPath(executable);
            startInfo.WorkingDirectory = Path.GetDirectoryName(startInfo.FileName)!;
            startInfo.UseShellExecute = true;
            using Process process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("The server process could not be started.");
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            while (!process.HasExited)
            {
                if (await IsRunningAsync(timeout.Token))
                {
                    return;
                }
                await Task.Delay(100, timeout.Token);
            }
            throw new InvalidOperationException("The server exited before it was ready. Check its console output.");
        }
    }
}
