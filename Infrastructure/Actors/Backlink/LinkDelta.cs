namespace Infrastructure.Actors.Backlink;

public record LinkDelta(
    string SourcePageId,
    IReadOnlyList<string> AddedTargets,
    IReadOnlyList<string> RemovedTargets);