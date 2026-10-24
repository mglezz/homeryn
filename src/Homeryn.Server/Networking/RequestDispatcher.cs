using Homeryn.Server.Shopping;
using Homeryn.Shared.Contracts;
using Homeryn.Shared.Shopping;
using Microsoft.Data.Sqlite;

namespace Homeryn.Server.Networking
{
    public class RequestDispatcher : IDisposable
    {
        private readonly ShoppingListService shoppingList;
        private readonly SemaphoreSlim storageGate = new SemaphoreSlim(1, 1);

        public RequestDispatcher(ShoppingListService shoppingList)
        {
            this.shoppingList = shoppingList;
        }

        public async Task<ResponseMessage> DispatchAsync(RequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Command))
            {
                return new ResponseMessage("ERROR", "INVALID_REQUEST");
            }
            if (request.Command == "PING")
            {
                return new ResponseMessage("PONG");
            }
            if (request.Command != ShoppingCommands.List && request.Command != ShoppingCommands.Add
                && request.Command != ShoppingCommands.Update && request.Command != ShoppingCommands.Delete)
            {
                return new ResponseMessage("ERROR", "UNKNOWN_COMMAND");
            }

            await storageGate.WaitAsync(cancellationToken);
            try
            {
                // SQLite uses synchronous I/O. Run one storage operation at a time off the accept loop.
                return await Task.Run(() => HandleShoppingRequest(request), cancellationToken);
            }
            catch (ShoppingListException exception)
            {
                return new ResponseMessage("ERROR", exception.Code);
            }
            catch (SqliteException exception)
            {
                Console.Error.WriteLine($"Shopping storage failed: {exception.Message}");
                return new ResponseMessage("ERROR", "STORAGE_UNAVAILABLE");
            }
            finally
            {
                storageGate.Release();
            }
        }

        private ResponseMessage HandleShoppingRequest(RequestMessage request)
        {
            ResponseMessage response = new ResponseMessage("OK");
            switch (request.Command)
            {
                case ShoppingCommands.List:
                    response.Page = shoppingList.ReadPage(request.AfterItemId, request.UpperBoundId);
                    break;
                case ShoppingCommands.Add:
                    response.Item = shoppingList.Add(request.Item);
                    break;
                case ShoppingCommands.Update:
                    response.Item = shoppingList.Update(request.ItemId, request.ExpectedVersion, request.Item);
                    break;
                case ShoppingCommands.Delete:
                    shoppingList.Delete(request.ItemId, request.ExpectedVersion);
                    break;
            }
            return response;
        }

        public void Dispose()
        {
            storageGate.Dispose();
        }
    }
}
