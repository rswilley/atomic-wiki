using Infrastructure.Actors.Backlink;
using Infrastructure.Actors.Page;

namespace Infrastructure.Actors.Graph;

[Alias("Actors.GraphGrain")]
public interface IGraphGrain : IGrainWithGuidKey
{
    [Alias("GetNeighbors")]
    Task<GraphNeighbors> GetNeighbors(string pageId);
}

public class GraphGrain(
    IGrainContext grainContext,
    IGrainFactory grainFactory) : IGrainBase, IGraphGrain
{
    public IGrainContext GrainContext { get; } = grainContext;
    
    public async Task<GraphNeighbors> GetNeighbors(string pageId)
    {
        var pageGrain = grainFactory.GetGrain<IPageGrain>(pageId);
        var backLinkGrain = grainFactory.GetGrain<IBacklinkGrain>(pageId);

        var outgoingTask = pageGrain.GetOutgoingLinks();
        var incomingTask = backLinkGrain.GetBacklinks();

        await Task.WhenAll(outgoingTask, incomingTask);

        var neighbors = new GraphNeighbors(
            pageId,
            outgoingTask.Result,
            incomingTask.Result);
        
        return neighbors;
    }
}