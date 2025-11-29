namespace Infrastructure.Actors.Backlink;

[Alias("Actors.BacklinkGrain")]
public interface IBacklinkGrain : IGrainWithStringKey
{
    [Alias("ApplyDelta")]
    Task ApplyDelta(LinkDelta delta);
    [Alias("GetBacklinks")]
    Task<IReadOnlyList<string>> GetBacklinks();
}

public class BacklinkGrain(
    IGrainContext grainContext,
    [PersistentState(
        stateName: "backlink",
        storageName: "local")]
    IPersistentState<BacklinkGrainState> profile) : IGrainBase, IBacklinkGrain
{
    public IGrainContext GrainContext { get; } = grainContext;

    public async Task ApplyDelta(LinkDelta delta)
    {
        // Check if this page is in the delta's added/removed targets
        var targetPageId = this.GetPrimaryKeyString();
        if (delta.AddedTargets.Contains(targetPageId, StringComparer.OrdinalIgnoreCase))
        {
            profile.State.PageIds.Add(delta.SourcePageId);
        }

        if (delta.RemovedTargets.Contains(targetPageId, StringComparer.OrdinalIgnoreCase))
        {
            profile.State.PageIds.Remove(delta.SourcePageId);
        }
        
        await profile.WriteStateAsync();
    }

    public Task<IReadOnlyList<string>> GetBacklinks()
    {
        IReadOnlyList<string> results = profile.State.PageIds.ToList();
        return Task.FromResult(results);
    }
}