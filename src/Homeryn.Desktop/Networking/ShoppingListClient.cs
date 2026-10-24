using System.Text.Json;
using Homeryn.Shared.Contracts;
using Homeryn.Shared.Shopping;

namespace Homeryn.Desktop.Networking
{
    public class ShoppingListClient
    {
        private readonly TcpClientService transport = new TcpClientService();
        private readonly string host;
        private readonly int port;

        public ShoppingListClient(string host = "127.0.0.1", int port = 5000)
        {
            this.host = host;
            this.port = port;
        }

        public async Task<List<ShoppingItem>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            List<ShoppingItem> items = new List<ShoppingItem>();
            RequestMessage request = new RequestMessage(ShoppingCommands.List);
            while (true)
            {
                ResponseMessage response = await SendAsync(request, cancellationToken);
                ShoppingPage? page = response.Page;
                if (page == null || page.Items == null || page.Items.Count > ShoppingPage.PageSize
                    || page.UpperBoundId < 0
                    || (request.UpperBoundId != null && page.UpperBoundId != request.UpperBoundId))
                {
                    throw new JsonException("The server returned an invalid shopping page.");
                }
                long lastId = request.AfterItemId;
                foreach (ShoppingItem item in page.Items)
                {
                    ValidateItem(item);
                    if (item.Id <= lastId || item.Id > page.UpperBoundId)
                    {
                        throw new JsonException("Shopping item identifiers must be ordered within the page boundary.");
                    }
                    items.Add(item);
                    lastId = item.Id;
                }
                if (page.NextAfterId == null)
                {
                    return items;
                }
                if (page.Items.Count == 0 || page.NextAfterId != lastId || lastId <= request.AfterItemId)
                {
                    throw new JsonException("The shopping page cursor did not advance.");
                }
                request.AfterItemId = lastId;
                request.UpperBoundId = page.UpperBoundId;
            }
        }

        public async Task<ShoppingItem> AddAsync(ShoppingItemInput input, CancellationToken cancellationToken = default)
        {
            RequestMessage request = new RequestMessage(ShoppingCommands.Add);
            request.Item = input;
            ResponseMessage response = await SendAsync(request, cancellationToken);
            ValidateItem(response.Item);
            return response.Item!;
        }

        public async Task<ShoppingItem> UpdateAsync(ShoppingItem item, ShoppingItemInput input,
            CancellationToken cancellationToken = default)
        {
            RequestMessage request = new RequestMessage(ShoppingCommands.Update);
            request.ItemId = item.Id;
            request.ExpectedVersion = item.Version;
            request.Item = input;
            ResponseMessage response = await SendAsync(request, cancellationToken);
            ValidateItem(response.Item);
            if (response.Item!.Id != item.Id || response.Item.Version != item.Version + 1)
            {
                throw new JsonException("The server returned an unexpected item version.");
            }
            return response.Item;
        }

        public async Task DeleteAsync(ShoppingItem item, CancellationToken cancellationToken = default)
        {
            RequestMessage request = new RequestMessage(ShoppingCommands.Delete);
            request.ItemId = item.Id;
            request.ExpectedVersion = item.Version;
            await SendAsync(request, cancellationToken);
        }

        private async Task<ResponseMessage> SendAsync(RequestMessage request, CancellationToken cancellationToken)
        {
            ResponseMessage response = await transport.SendAsync(request, host, port, cancellationToken);
            if (response.Status == "ERROR" && !string.IsNullOrWhiteSpace(response.Error))
            {
                throw new ServerRequestException(response.Error);
            }
            if (response.Status != "OK" || response.Error != null)
            {
                throw new JsonException("The server returned an invalid shopping response.");
            }
            return response;
        }

        private static void ValidateItem(ShoppingItem? item)
        {
            if (item == null || item.Id <= 0 || item.Version <= 0 || string.IsNullOrWhiteSpace(item.Name)
                || item.Name.Length > 80 || item.Quantity < 1 || item.Quantity > 999)
            {
                throw new JsonException("The server returned an invalid shopping item.");
            }
        }
    }
}
