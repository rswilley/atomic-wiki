namespace Infrastructure.Actors.Backlink;

public class BacklinkGrainState
{
    // Set of pageIds that link to this page
    public HashSet<string> PageIds { get; set; } = [];
}