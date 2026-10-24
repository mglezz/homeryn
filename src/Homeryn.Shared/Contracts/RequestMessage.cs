using Homeryn.Shared.Shopping;

namespace Homeryn.Shared.Contracts
{
    public class RequestMessage
    {
        public string Command { get; }
        public ShoppingItemInput? Item { get; set; }
        public long ItemId { get; set; }
        public long ExpectedVersion { get; set; }
        public long AfterItemId { get; set; }
        public long? UpperBoundId { get; set; }

        public RequestMessage(string command)
        {
            Command = command;
        }
    }
}
