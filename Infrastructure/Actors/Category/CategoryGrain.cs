namespace Infrastructure.Actors.Category;

[Alias("Actors.CategoryGrain")]
public interface ICategoryGrain : IGrainWithStringKey
{
    [Alias("AddPage")]
    Task AddPage(string pageId);
    [Alias("RemovePage")]
    Task RemovePage(string pageId);
    [Alias("GetPages")]
    Task<IReadOnlyList<string>> GetPages();
}

public class CategoryGrain(IGrainContext grainContext) : IGrainBase, ICategoryGrain
{
    public IGrainContext GrainContext { get; } = grainContext;
    
    public Task AddPage(string pageId)
    {
        throw new NotImplementedException();
    }

    public Task RemovePage(string pageId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<string>> GetPages()
    {
        throw new NotImplementedException();
    }
}

// One state object per category grain (category slug as key)