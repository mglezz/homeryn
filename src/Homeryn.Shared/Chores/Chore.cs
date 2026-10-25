namespace Homeryn.Shared.Chores
{
    public class Chore
    {
        public long Id { get; }
        public string Name { get; }
        public string? Assignee { get; }
        public DateOnly? DueDate { get; }
        public bool IsCompleted { get; }
        public long Version { get; }

        public Chore(long id, string name, string? assignee, DateOnly? dueDate, bool isCompleted, long version)
        {
            Id = id;
            Name = name;
            Assignee = assignee;
            DueDate = dueDate;
            IsCompleted = isCompleted;
            Version = version;
        }
    }
}
