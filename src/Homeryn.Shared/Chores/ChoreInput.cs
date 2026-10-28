namespace Homeryn.Shared.Chores
{
    public class ChoreInput
    {
        public string? Name { get; }
        public string? Assignee { get; }
        public DateOnly? DueDate { get; }
        public bool IsCompleted { get; }

        public ChoreInput(string? name, string? assignee = null, DateOnly? dueDate = null, bool isCompleted = false)
        {
            Name = name;
            Assignee = assignee;
            DueDate = dueDate;
            IsCompleted = isCompleted;
        }
    }
}
