using System.Net;
using System.Net.Sockets;
using Homeryn.Server.Networking;
using Homeryn.Server.Persistence;
using Homeryn.Server.Shopping;
using Microsoft.Data.Sqlite;

namespace Homeryn.Server
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using CancellationTokenSource shutdown = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                shutdown.Cancel();
            };

            try
            {
                string databasePath = GetDatabasePath(args);
                SqliteShoppingItemRepository repository = new SqliteShoppingItemRepository(databasePath);
                repository.Initialize();
                using RequestDispatcher dispatcher = new RequestDispatcher(new ShoppingListService(repository));
                TcpServer server = new TcpServer(IPAddress.Loopback, 5000, dispatcher);
                Console.WriteLine($"Shopping database: {Path.GetFullPath(databasePath)}");
                await server.RunAsync(shutdown.Token);
            }
            catch (SocketException exception)
            {
                Console.Error.WriteLine($"Unable to run the TCP server on 127.0.0.1:5000: {exception.Message}");
                Environment.ExitCode = 1;
            }
            catch (Exception exception) when (exception is SqliteException || exception is IOException
                || exception is UnauthorizedAccessException || exception is ArgumentException
                || exception is InvalidDataException)
            {
                Console.Error.WriteLine($"Unable to start the server: {exception.Message}");
                Environment.ExitCode = 1;
            }
        }

        private static string GetDatabasePath(string[] args)
        {
            if (args.Length == 2 && args[0] == "--database" && !string.IsNullOrWhiteSpace(args[1]))
            {
                return args[1];
            }
            if (args.Length != 0)
            {
                throw new ArgumentException("Usage: Homeryn.Server [--database <path>]");
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Homeryn", "server", "homeryn.db");
        }
    }
}
