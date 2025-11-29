namespace Infrastructure.Actors.Tag;

[GenerateSerializer]
[Alias("Actors.Tag.TagDelta")]
public record TagDelta(
    string PageId,
    IReadOnlyList<string> AddedTags,
    IReadOnlyList<string> RemovedTags);