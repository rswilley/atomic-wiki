using Domain;

namespace Wiki.Grains.Page;

[GenerateSerializer]
[Alias("Page.ContentFrontMatterSurrogate")]
public struct ContentFrontMatterSurrogate
{
    [Id(0)] public string? PermanentId;
    [Id(1)] public string Title;
    [Id(2)] public string Type;
    [Id(3)] public string? Category;
    [Id(4)] public List<string>? Tags;
    [Id(5)] public bool? Pinned;
    [Id(6)] public DateTime? CreatedAt;
    [Id(7)] public DateTime? UpdatedAt;
    [Id(8)] public Dictionary<string, object>? Extra;
}

[RegisterConverter]
public sealed class ContentFrontMatterSurrogateConverter :
    IConverter<ContentFrontMatter, ContentFrontMatterSurrogate>,
    IPopulator<ContentFrontMatter, ContentFrontMatterSurrogate>
{
    public ContentFrontMatter ConvertFromSurrogate(
        in ContentFrontMatterSurrogate surrogate) =>
        new()
        {
            PermanentId = surrogate.PermanentId,
            Title = surrogate.Title,
            Type = surrogate.Type, 
            Category = surrogate.Category, 
            Tags = surrogate.Tags, 
            Pinned = surrogate.Pinned, 
            CreatedAt = surrogate.CreatedAt, 
            UpdatedAt = surrogate.UpdatedAt, 
            Extra = surrogate.Extra
        };

    public ContentFrontMatterSurrogate ConvertToSurrogate(
        in ContentFrontMatter value) =>
        new()
        {
            PermanentId = value.PermanentId,
            Title = value.Title,
            Type = value.Type, 
            Category = value.Category, 
            Tags = value.Tags, 
            Pinned = value.Pinned, 
            CreatedAt = value.CreatedAt, 
            UpdatedAt = value.UpdatedAt, 
            Extra = value.Extra
        };

    public void Populate(
        in ContentFrontMatterSurrogate surrogate, ContentFrontMatter value)
    {
        value.PermanentId = surrogate.PermanentId;
        value.Title = surrogate.Title;
        value.Type = surrogate.Type;
        value.Category = surrogate.Category;
        value.Tags = surrogate.Tags;
        value.Pinned = surrogate.Pinned;
        value.CreatedAt = surrogate.CreatedAt;
        value.UpdatedAt = surrogate.UpdatedAt;
        value.Extra = surrogate.Extra;
    }
}