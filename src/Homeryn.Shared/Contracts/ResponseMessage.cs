using Homeryn.Shared.Shopping;

namespace Homeryn.Shared.Contracts
{
    public class ResponseMessage
    {
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
