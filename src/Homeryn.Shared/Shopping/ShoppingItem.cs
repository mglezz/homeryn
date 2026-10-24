namespace Homeryn.Shared.Shopping
{
    public class ShoppingItem
    {
        public long Id { get; }
        public string Name { get; }
        public int Quantity { get; }
        public bool IsPurchased { get; }
        public long Version { get; }

        public ShoppingItem(long id, string name, int quantity, bool isPurchased, long version)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            IsPurchased = isPurchased;
            Version = version;
        }
    }
}
