namespace Homeryn.Server.Chores
{
    public class ChoresException : Exception
    {
        public string Code { get; }

        public ChoresException(string code) : base(code)
        {
            Code = code;
        }
    }
}
