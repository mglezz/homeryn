namespace Homeryn.Server.Shopping
{
    public class ShoppingListException : Exception
    {
        public string Code { get; }

        public ShoppingListException(string code) : base(code)
        {
            Code = code;
        }
    }
}
