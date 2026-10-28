using System.Text.Json;
using Homeryn.Shared.Contracts;
using Homeryn.Shared.Chores;

namespace Homeryn.Desktop.Networking
{
    public class ChoresClient
    {
        private readonly TcpClientService transport = new TcpClientService();
        private readonly string host;
        private readonly int port;

        public ChoresClient(string host = "127.0.0.1", int port = 5000)
        {
            this.host = host;
            this.port = port;
        }

        public async Task<List<Chore>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TcpClientService.OperationTimeout);
            List<Chore> items = new List<Chore>();
            RequestMessage request = new RequestMessage(ChoreCommands.List);
            while (true)
            {
                ResponseMessage response = await SendAsync(request, timeout.Token);
                ChorePage? page = response.ChorePage;
                if (page == null || page.Items == null || page.Items.Count > ChorePage.PageSize
                    || page.UpperBoundId < 0
                    || (request.UpperBoundId != null && page.UpperBoundId != request.UpperBoundId))
                {
                    throw new JsonException("The server returned an invalid chore page.");
                }
                long lastId = request.AfterChoreId;
                foreach (Chore item in page.Items)
                {
                    ValidateItem(item);
                    if (item.Id <= lastId || item.Id > page.UpperBoundId)
                    {
                        throw new JsonException("Chore identifiers must be ordered within the page boundary.");
                    }
                    items.Add(item);
                    lastId = item.Id;
                }
                if (page.NextAfterId == null)
                {
                    return items;
                }
                if (page.Items.Count == 0 || page.NextAfterId != lastId || lastId <= request.AfterChoreId
                    || lastId >= page.UpperBoundId)
                {
                    throw new JsonException("The chore page cursor did not advance.");
                }
                request.AfterChoreId = lastId;
                request.UpperBoundId = page.UpperBoundId;
            }
        }

        public async Task<Chore> AddAsync(ChoreInput input, CancellationToken cancellationToken = default)
        {
            RequestMessage request = new RequestMessage(ChoreCommands.Add);
            request.Chore = input;
            ResponseMessage response = await SendAsync(request, cancellationToken);
            ValidateItem(response.Chore);
            if (response.Chore!.Version != 1)
            {
                throw new JsonException("The server returned an unexpected initial chore version.");
            }
            return response.Chore!;
        }

        public async Task<Chore> UpdateAsync(Chore item, ChoreInput input,
            CancellationToken cancellationToken = default)
        {
            RequestMessage request = new RequestMessage(ChoreCommands.Update);
            request.ChoreId = item.Id;
            request.ExpectedVersion = item.Version;
            request.Chore = input;
            ResponseMessage response = await SendAsync(request, cancellationToken);
            ValidateItem(response.Chore);
            if (response.Chore!.Id != item.Id || response.Chore.Version != item.Version + 1)
            {
                throw new JsonException("The server returned an unexpected chore version.");
            }
            return response.Chore;
        }

        public async Task DeleteAsync(Chore item, CancellationToken cancellationToken = default)
        {
            RequestMessage request = new RequestMessage(ChoreCommands.Delete);
            request.ChoreId = item.Id;
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
                throw new JsonException("The server returned an invalid chore response.");
            }
            return response;
        }

        private static void ValidateItem(Chore? item)
        {
            if (item == null || item.Id <= 0 || item.Version <= 0 || string.IsNullOrWhiteSpace(item.Name)
                || item.Name.Length > 80 || item.Name.Any(char.IsControl)
                || (item.Assignee != null && (string.IsNullOrWhiteSpace(item.Assignee)
                    || item.Assignee.Length > 60 || item.Assignee.Any(char.IsControl)))
                || item.DueDate < new DateOnly(1753, 1, 1) || item.DueDate > new DateOnly(9998, 12, 31))
            {
                throw new JsonException("The server returned an invalid chore item.");
            }
        }
    }
}
