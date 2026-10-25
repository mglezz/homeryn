using Homeryn.Shared.Chores;

namespace Homeryn.Server.Chores
{
    public interface IChoreRepository
    {
        ChorePage ReadPage(long afterChoreId, long? upperBoundId);
        Chore Add(string name, string? assignee, DateOnly? dueDate, bool isCompleted);
        Chore Update(long id, long expectedVersion, string name, string? assignee, DateOnly? dueDate, bool isCompleted);
        void Delete(long id, long expectedVersion);
    }
}
