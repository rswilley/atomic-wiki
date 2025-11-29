namespace Infrastructure.Actors.Graph;

[Alias("Actors.GraphGrain")]
public interface IGraphGrain : IGrainWithGuidKey
{
    [Alias("GetNeighbors")]
    Task<GraphNeighbors> GetNeighbors(string pageId);
}

public class GraphGrain(IGrainContext grainContext) : IGrainBase, IGraphGrain
{
    public IGrainContext GrainContext { get; } = grainContext;
    
    public Task<GraphNeighbors> GetNeighbors(string pageId)
    {
        throw new NotImplementedException();
    }
}