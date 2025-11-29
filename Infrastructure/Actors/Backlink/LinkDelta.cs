namespace Infrastructure.Actors.Backlink;

[GenerateSerializer]
[Alias("Actors.Backlink.LinkDelta")]
public record LinkDelta(
    string SourcePageId,
    IReadOnlyList<string> AddedTargets,
    IReadOnlyList<string> RemovedTargets);