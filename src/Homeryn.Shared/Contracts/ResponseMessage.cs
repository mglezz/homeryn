using Homeryn.Shared.Chores;
using Homeryn.Shared.Shopping;

namespace Homeryn.Shared.Contracts
{
    public class ResponseMessage
    {
        public Chore? Chore { get; set; }
        public ChorePage? ChorePage { get; set; }

        public string Status { get; }
        public string? Error { get; }
        public ShoppingItem? Item { get; set; }
        public ShoppingPage? Page { get; set; }

        public ResponseMessage(string status, string? error = null)
        {
            Status = status;
            Error = error;
        }
    }
}
