namespace Homeryn.Desktop.Networking
{
    public class ServerRequestException : Exception
    {
        public string Code { get; }

        public ServerRequestException(string code) : base(code)
        {
            Code = code;
        }
    }
}
