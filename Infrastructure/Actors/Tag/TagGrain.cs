namespace Infrastructure.Actors.Tag;

[Alias("Actors.TagGrain")]
public interface ITagGrain : IGrainWithStringKey
{
    [Alias("ApplyDelta")]
    Task ApplyDelta(TagDelta delta);
    [Alias("GetPages")]
    Task<IReadOnlyList<string>> GetPages();
}

public class TagGrain(IGrainContext grainContext) : IGrainBase, ITagGrain
{
    public IGrainContext GrainContext { get; } = grainContext;
    
    public Task ApplyDelta(TagDelta delta)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<string>> GetPages()
    {
        throw new NotImplementedException();
    }
}