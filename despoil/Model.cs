namespace despoil;

public class Model
{
    public string? InlineStyles = null;
    public IssueGroupParent[]? Collections { get; set; } = null;
    public IssueGroupParent[]? Threads { get; set; } = null;
    public Issue[]? Issues { get; set; } = null;
    public EntityGroup[]? EntityGroups { get; set; } = null;
    public Item[]? Items { get; set; } = null;

    public string AllIssues { get; set; } = string.Empty;
}
