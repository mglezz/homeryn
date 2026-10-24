using Homeryn.Shared.Shopping;

namespace Homeryn.Server.Shopping
{
    public interface IShoppingItemRepository
    {
        ShoppingPage ReadPage(long afterItemId, long? upperBoundId);
        ShoppingItem Add(string name, int quantity, bool isPurchased);
        ShoppingItem Update(long id, long expectedVersion, string name, int quantity, bool isPurchased);
        void Delete(long id, long expectedVersion);
    }
}
