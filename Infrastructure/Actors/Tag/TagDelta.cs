namespace Infrastructure.Actors.Tag;

public record TagDelta(
    string PageId,
    IReadOnlyList<string> AddedTags,
    IReadOnlyList<string> RemovedTags);