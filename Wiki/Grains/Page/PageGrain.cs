using Wiki.Models;
using Wiki.Services;

namespace Wiki.Grains.Page;

[Alias("PageGrain")]
public interface IPageGrain : IGrainWithStringKey
{
    [Alias("CreatePage")]
    Task CreatePage(PageWriteModel page);
}

public class PageGrain(
    [PersistentState(
        stateName: "page",
        storageName: "local")]
    IPersistentState<PageGrainState> profile,
    IGrainContext grainContext,
    IPageStore pageStore,
    ISearchStore searchStore,
    IMarkdownService markdownService)
    : IGrainBase, IPageGrain
{
    public IGrainContext GrainContext { get; } = grainContext;

    public async Task CreatePage(PageWriteModel page)
    {
        var permanentId = this.GetPrimaryKeyString();
        var title = markdownService.GetFirstHeader(page.Markdown) ?? "Untitled";
        var tags = GetTags(page.Tags);
        
        await SavePageDocument(page, permanentId, title, tags);
        SaveSearchIndex(page, permanentId, title, tags);
        await UpdateState(page, permanentId, title, tags);
    }

    private async Task UpdateState(PageWriteModel page, string permanentId, string title, List<string> tags)
    {
        profile.State.PermanentId = permanentId;
        profile.State.Title = title;
        profile.State.Type = page.Type;
        profile.State.Markdown = page.Markdown;
        profile.State.Category = page.Category;
        profile.State.Tags = tags;
        profile.State.IsPinned = page.IsPinned;
        profile.State.CreatedAtUtc = page.CreatedAt ?? DateTime.UtcNow;
        profile.State.UpdatedAtUtc = DateTime.UtcNow;
        
        await profile.WriteStateAsync();
    }

    private void SaveSearchIndex(PageWriteModel page, string permanentId, string title, List<string> tags)
    {
        searchStore.Create(new PageSearchItem
        {
            PermanentId = permanentId,
            Title = title,
            Body = page.Markdown,
            Tags = tags
        });
    }

    private async Task SavePageDocument(PageWriteModel page, string permanentId, string title, List<string> tags)
    {
        await pageStore.Save(new PageDocument
        {
            Meta = new PageFrontMatter
            {
                PermanentId = permanentId,
                Title = title,
                Type = page.Type,
                Category = page.Category,
                Tags = tags,
                Pinned = page.IsPinned,
                CreatedAt = page.CreatedAt
            },
            Markdown = page.Markdown
        });
    }

    private static List<string> GetTags(string? tags)
    {
        return string.IsNullOrEmpty(tags) 
            ? [] 
            : tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}

public class PageGrainState
{
    public string PermanentId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Type { get; set; } = nameof(PageType.Note).ToLower();
    public string Markdown { get; set; } = "";
    public string? Category { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public List<string> OutgoingLinks { get; set; } = [];
    public bool IsPinned { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}