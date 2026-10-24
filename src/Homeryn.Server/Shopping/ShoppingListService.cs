using Homeryn.Shared.Shopping;

namespace Homeryn.Server.Shopping
{
    public class ShoppingListService
    {
        private readonly IShoppingItemRepository repository;

        public ShoppingListService(IShoppingItemRepository repository)
        {
            this.repository = repository;
        }

        public ShoppingPage ReadPage(long afterItemId, long? upperBoundId)
        {
            if (afterItemId < 0 || upperBoundId < 0 || afterItemId > upperBoundId)
            {
                throw new ShoppingListException("INVALID_PAGE");
            }
            return repository.ReadPage(afterItemId, upperBoundId);
        }

        public ShoppingItem Add(ShoppingItemInput? input)
        {
            string name = ValidateInput(input);
            return repository.Add(name, input!.Quantity, input.IsPurchased);
        }

        public ShoppingItem Update(long id, long expectedVersion, ShoppingItemInput? input)
        {
            ValidateIdentity(id, expectedVersion);
            string name = ValidateInput(input);
            return repository.Update(id, expectedVersion, name, input!.Quantity, input.IsPurchased);
        }

        public void Delete(long id, long expectedVersion)
        {
            ValidateIdentity(id, expectedVersion);
            repository.Delete(id, expectedVersion);
        }

        private static string ValidateInput(ShoppingItemInput? input)
        {
            if (input == null)
            {
                throw new ShoppingListException("INVALID_ITEM");
            }
            string name = (input.Name ?? "").Trim();
            if (name.Length == 0 || name.Length > 80 || name.Any(char.IsControl))
            {
                throw new ShoppingListException("INVALID_NAME");
            }
            if (input.Quantity < 1 || input.Quantity > 999)
            {
                throw new ShoppingListException("INVALID_QUANTITY");
            }
            return name;
        }

        private static void ValidateIdentity(long id, long expectedVersion)
        {
            if (id <= 0 || expectedVersion <= 0 || expectedVersion == long.MaxValue)
            {
                throw new ShoppingListException("INVALID_ITEM_VERSION");
            }
        }
    }
}
