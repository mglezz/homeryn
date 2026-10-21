namespace Homeryn.Shared.Contracts
{
    public class ResponseMessage
    {
        public string Status { get; }
        public string? Error { get; }

        public ResponseMessage(string status, string? error = null)
        {
            Status = status;
            Error = error;
        }
    }
}
