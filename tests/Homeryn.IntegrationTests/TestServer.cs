using Homeryn.Server.Chores;
using System.Net;
using Homeryn.Desktop.Networking;
using Homeryn.Server.Networking;
using Homeryn.Server.Persistence;
using Homeryn.Server.Shopping;

namespace Homeryn.IntegrationTests
{
    internal class TestServer : IAsyncDisposable
    {
        private readonly CancellationTokenSource shutdown = new CancellationTokenSource();
        private readonly RequestDispatcher dispatcher;
        private readonly Task serverTask;
        public int Port { get; }
        public ShoppingListClient Client { get; }

        public TestServer(string databasePath)
        {
            SqliteShoppingItemRepository repository = new SqliteShoppingItemRepository(databasePath);
            new SqliteDatabase(databasePath).Initialize();
            dispatcher = new RequestDispatcher(new ShoppingListService(repository),
                new ChoresService(new SqliteChoreRepository(databasePath)));
            TcpServer server = new TcpServer(IPAddress.Loopback, 0, dispatcher);
            serverTask = server.RunAsync(shutdown.Token);
            Port = server.LocalEndpoint.Port;
            Client = new ShoppingListClient(port: Port);
        }

        public async ValueTask DisposeAsync()
        {
            await shutdown.CancelAsync();
            await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
            dispatcher.Dispose();
            shutdown.Dispose();
        }
    }
}
