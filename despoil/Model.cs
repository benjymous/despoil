namespace despoil
{
    public class Model
    {
        public IssueGroup[]? Collections { get; set; } = null;
        public IssueGroup[]? Threads { get; set; } = null;
        public Issue[]? Issues { get; set; } = null;
        public Entity[]? Entities { get; set; } = null;
        public Item[]? Items { get; set; } = null;

        public string AllIssues { get; set; } = string.Empty;
    }
}
