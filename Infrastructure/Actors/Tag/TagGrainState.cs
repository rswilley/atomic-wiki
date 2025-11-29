namespace Infrastructure.Actors.Tag;

public class TagGrainState
{
    // Pages that have this tag
    public HashSet<string> PageIds { get; set; } = [];
}