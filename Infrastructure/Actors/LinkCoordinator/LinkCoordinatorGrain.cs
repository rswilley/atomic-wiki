using Infrastructure.Actors.Backlink;

namespace Infrastructure.Actors.LinkCoordinator;

[Alias("Actors.LinkCoordinatorGrain")]
public interface ILinkCoordinatorGrain : IGrainWithIntegerKey
{
    [Alias("ApplyDelta")]
    Task ApplyDelta(LinkDelta delta);
}

public class LinkCoordinatorGrain(
    IGrainContext grainContext,
    IGrainFactory grainFactory) : IGrainBase, ILinkCoordinatorGrain
{
    public IGrainContext GrainContext { get; } = grainContext;
    
    public async Task ApplyDelta(LinkDelta delta)
    {
        // For each removed target: tell its BacklinkGrain to remove source
        foreach (var removed in delta.RemovedTargets)
        {
            var back = grainFactory.GetGrain<IBacklinkGrain>(removed);
            await back.ApplyDelta(delta);
        }

        // For each added target: same
        foreach (var added in delta.AddedTargets)
        {
            var back = grainFactory.GetGrain<IBacklinkGrain>(added);
            await back.ApplyDelta(delta);
        }
    }
}