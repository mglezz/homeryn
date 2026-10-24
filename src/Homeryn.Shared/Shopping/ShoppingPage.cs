namespace Homeryn.Shared.Shopping
{
    public class ShoppingPage
    {
        // Five maximum-length names still fit within the 4 KiB JSON frame limit.
        public const int PageSize = 5;
        public List<ShoppingItem> Items { get; }
        public long UpperBoundId { get; }
        public long? NextAfterId { get; }

        public ShoppingPage(List<ShoppingItem> items, long upperBoundId, long? nextAfterId)
        {
            Items = items;
            UpperBoundId = upperBoundId;
            NextAfterId = nextAfterId;
        }
    }
}
