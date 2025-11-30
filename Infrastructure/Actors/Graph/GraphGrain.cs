using Infrastructure.Actors.Backlink;
using Infrastructure.Actors.Page;

namespace Infrastructure.Actors.Graph;

[Alias("Actors.GraphGrain")]
public interface IGraphGrain : IGrainWithGuidKey
{
    [Alias("GetNeighbors")]
    Task<GraphNeighbors> GetNeighbors(string slug);
}

public class GraphGrain(
    IGrainContext grainContext,
    IGrainFactory grainFactory) : IGrainBase, IGraphGrain
{
    public IGrainContext GrainContext { get; } = grainContext;
    
    public async Task<GraphNeighbors> GetNeighbors(string slug)
    {
        var pageGrain = grainFactory.GetGrain<IPageGrain>(slug);
        var backLinkGrain = grainFactory.GetGrain<IBacklinkGrain>(slug);

        var outgoingTask = pageGrain.GetOutgoingLinks();
        var incomingTask = backLinkGrain.GetBacklinks();

        await Task.WhenAll(outgoingTask, incomingTask);

        var neighbors = new GraphNeighbors(
            slug,
            outgoingTask.Result,
            incomingTask.Result);
        
        return neighbors;
    }
}