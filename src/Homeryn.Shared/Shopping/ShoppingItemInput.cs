namespace Homeryn.Shared.Shopping
{
    public class ShoppingItemInput
    {
        public string? Name { get; }
        public int Quantity { get; }
        public bool IsPurchased { get; }

        public ShoppingItemInput(string? name, int quantity, bool isPurchased = false)
        {
            Name = name;
            Quantity = quantity;
            IsPurchased = isPurchased;
        }
    }
}
