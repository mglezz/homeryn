using Homeryn.Server.Chores;
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
                if (args.Length == 1 && args[0] == "--help")
                {
                    Console.WriteLine("Usage: Homeryn.Server [--database <path>] [--backup <new-file>]");
                    Console.WriteLine("--backup creates a consistent SQLite copy and exits without starting the server.");
                    return;
                }
                (string databasePath, string? backupPath) = ParseOptions(args);
                if (backupPath != null)
                {
                    DatabaseBackup.Create(databasePath, backupPath);
                    Console.WriteLine($"Backup saved: {Path.GetFullPath(backupPath)}");
                    return;
                }
                SqliteShoppingItemRepository repository = new SqliteShoppingItemRepository(databasePath);
                new SqliteDatabase(databasePath).Initialize();
                using RequestDispatcher dispatcher = new RequestDispatcher(new ShoppingListService(repository),
                new ChoresService(new SqliteChoreRepository(databasePath)));
                TcpServer server = new TcpServer(IPAddress.Loopback, 5000, dispatcher);
                Console.WriteLine($"Homeryn database: {Path.GetFullPath(databasePath)}");
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
                Console.Error.WriteLine($"Unable to complete the server operation: {exception.Message}");
                Environment.ExitCode = 1;
            }
        }

        private static (string DatabasePath, string? BackupPath) ParseOptions(string[] args)
        {
            string? databasePath = null;
            string? backupPath = null;
            for (int index = 0; index < args.Length; index += 2)
            {
                if (index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
                {
                    throw new ArgumentException("Usage: Homeryn.Server [--database <path>] [--backup <new-file>]");
                }
                if (args[index] == "--database" && databasePath == null)
                {
                    databasePath = args[index + 1];
                }
                else if (args[index] == "--backup" && backupPath == null)
                {
                    backupPath = args[index + 1];
                }
                else
                {
                    throw new ArgumentException("Unknown or repeated option. Use --help for usage.");
                }
            }
            databasePath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Homeryn", "server", "homeryn.db");
            return (databasePath, backupPath);
        }
    }
}
