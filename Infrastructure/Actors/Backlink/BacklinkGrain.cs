namespace Infrastructure.Actors.Backlink;

[Alias("Actors.BacklinkGrain")]
public interface IBacklinkGrain : IGrainWithStringKey
{
    [Alias("ApplyDelta")]
    Task ApplyDelta(LinkDelta delta);
    [Alias("GetBacklinks")]
    Task<IReadOnlyList<string>> GetBacklinks();
}

public class BacklinkGrain(IGrainContext grainContext) : IGrainBase, IBacklinkGrain
{
    public IGrainContext GrainContext { get; } = grainContext;

    public Task ApplyDelta(LinkDelta delta)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<string>> GetBacklinks()
    {
        throw new NotImplementedException();
    }
}