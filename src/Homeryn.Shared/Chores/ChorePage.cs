namespace Homeryn.Shared.Chores
{
    public class ChorePage
    {
        // Three chores with escaped maximum-length names and assignees fit within 4 KiB.
        public const int PageSize = 3;
        public List<Chore> Items { get; }
        public long UpperBoundId { get; }
        public long? NextAfterId { get; }

        public ChorePage(List<Chore> items, long upperBoundId, long? nextAfterId)
        {
            Items = items;
            UpperBoundId = upperBoundId;
            NextAfterId = nextAfterId;
        }
    }
}
