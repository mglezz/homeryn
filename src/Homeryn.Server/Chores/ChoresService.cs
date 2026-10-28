using Homeryn.Shared.Chores;

namespace Homeryn.Server.Chores
{
    public class ChoresService
    {
        private readonly IChoreRepository repository;

        public ChoresService(IChoreRepository repository)
        {
            this.repository = repository;
        }

        public ChorePage ReadPage(long afterChoreId, long? upperBoundId)
        {
            if (afterChoreId < 0 || upperBoundId < 0 || afterChoreId > upperBoundId)
            {
                throw new ChoresException("INVALID_PAGE");
            }
            return repository.ReadPage(afterChoreId, upperBoundId);
        }

        public Chore Add(ChoreInput? input)
        {
            string name = ValidateInput(input);
            return repository.Add(name, NormalizeAssignee(input!.Assignee), input.DueDate, input.IsCompleted);
        }

        public Chore Update(long id, long expectedVersion, ChoreInput? input)
        {
            ValidateIdentity(id, expectedVersion);
            string name = ValidateInput(input);
            return repository.Update(id, expectedVersion, name, NormalizeAssignee(input!.Assignee), input.DueDate, input.IsCompleted);
        }

        public void Delete(long id, long expectedVersion)
        {
            ValidateIdentity(id, expectedVersion);
            repository.Delete(id, expectedVersion);
        }

        private static string ValidateInput(ChoreInput? input)
        {
            if (input == null)
            {
                throw new ChoresException("INVALID_CHORE");
            }
            string name = (input.Name ?? "").Trim();
            if (name.Length == 0 || name.Length > 80 || name.Any(char.IsControl))
            {
                throw new ChoresException("INVALID_NAME");
            }
            string? assignee = NormalizeAssignee(input.Assignee);
            if (assignee != null && (assignee.Length > 60 || assignee.Any(char.IsControl)))
            {
                throw new ChoresException("INVALID_ASSIGNEE");
            }
            // Match the date range supported by the desktop date picker.
            if (input.DueDate < new DateOnly(1753, 1, 1) || input.DueDate > new DateOnly(9998, 12, 31))
            {
                throw new ChoresException("INVALID_DUE_DATE");
            }
            return name;
        }

        private static string? NormalizeAssignee(string? assignee)
        {
            if (string.IsNullOrWhiteSpace(assignee))
            {
                return null;
            }
            return assignee.Trim();
        }

        private static void ValidateIdentity(long id, long expectedVersion)
        {
            if (id <= 0 || expectedVersion <= 0 || expectedVersion == long.MaxValue)
            {
                throw new ChoresException("INVALID_CHORE_VERSION");
            }
        }
    }
}
