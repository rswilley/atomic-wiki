namespace Infrastructure.Actors.Graph;

public class GraphGrainState
{
    // Cached neighbors for the last requested center
    public string? CenterPageId { get; set; }
    public List<string> Outgoing { get; set; } = [];
    public List<string> Incoming { get; set; } = [];
    public DateTime CachedAtUtc { get; set; }
}