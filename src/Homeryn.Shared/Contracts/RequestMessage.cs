namespace Homeryn.Shared.Contracts
{
    public class RequestMessage
    {
        public string Command { get; }

        public RequestMessage(string command)
        {
            Command = command;
        }
    }
}
