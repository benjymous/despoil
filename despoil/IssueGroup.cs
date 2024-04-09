namespace despoil;

public class IssueGroup
{
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public int index { get; set; } = 0;
    public string childdata { get; set; } = string.Empty;
    public Issue[]? Issues { get; set; } = null;
}
