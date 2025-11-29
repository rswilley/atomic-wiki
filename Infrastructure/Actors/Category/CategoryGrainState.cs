namespace Infrastructure.Actors.Category;

public class CategoryGrainState
{
    // PageIds in this category
    public HashSet<string> PageIds { get; set; } = [];
}