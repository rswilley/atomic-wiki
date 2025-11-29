using Domain;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Repositories;

namespace Wiki.Grains.Page;

[Alias("PageGrain")]
public interface IPageGrain : IGrainWithStringKey
{
    [Alias("CreatePage")]
    Task CreatePage(ContentFrontMatter frontMatter, string markdownBody);
}

public class PageGrain(
    [PersistentState(
        stateName: "page",
        storageName: "local")]
    IPersistentState<PageGrainState> profile,
    IGrainContext grainContext,
    IPageRepository pageRepository,
    ISearchRepository searchRepository,
    IMarkdownParser markdownParser)
    : IGrainBase, IPageGrain
{
    public IGrainContext GrainContext { get; } = grainContext;

    public async Task CreatePage(ContentFrontMatter frontMatter, string markdownBody)
    {
        var permanentId = this.GetPrimaryKeyString();
        var fullMarkdown = markdownParser.Serialize(frontMatter, markdownBody);
        var page = new WikiPage(new WikiContent(fullMarkdown, markdownParser), null!);
        
        await SavePageDocument(page, permanentId);
        SaveSearchIndex(page, permanentId);
        await UpdateState(page, permanentId);
    }

    private async Task UpdateState(WikiPage page, string permanentId)
    {
        profile.State.PermanentId = permanentId;
        profile.State.Title = page.Content.FrontMatter.Title;
        profile.State.Type = page.Content.FrontMatter.Type;
        profile.State.Markdown = page.Content.Value;
        profile.State.Category = page.Content.FrontMatter.Category;
        profile.State.Tags = page.Content.FrontMatter.Tags?.ToHashSet() ?? [];
        profile.State.IsPinned = page.Content.FrontMatter.Pinned ?? false;
        profile.State.OutgoingLinks = page.Content.GetOutgoingLinks().ToHashSet();
        profile.State.CreatedAtUtc = page.Content.FrontMatter.CreatedAt ?? DateTime.UtcNow;
        profile.State.UpdatedAtUtc = page.Content.FrontMatter.UpdatedAt ?? DateTime.UtcNow;
        
        await profile.WriteStateAsync();
    }

    private void SaveSearchIndex(WikiPage page, string permanentId)
    {
        searchRepository.Create(new PageSearchItem
        {
            PermanentId = permanentId,
            Title = page.Content.FrontMatter.Title,
            Body = page.Content.Value,
            Tags = page.Content.FrontMatter.Tags?.ToList() ?? []
        });
    }

    private async Task SavePageDocument(WikiPage page, string permanentId)
    {
        await pageRepository.Save(page.Content.Value, page.Content.FrontMatter.Title, permanentId);
    }
}

public class PageGrainState
{
    public string PermanentId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Type { get; set; } = nameof(PageType.Note).ToLower();
    public string Markdown { get; set; } = "";
    public string? Category { get; set; } = "";
    public HashSet<string> Tags { get; set; } = [];
    public HashSet<string> OutgoingLinks { get; set; } = [];
    public bool IsPinned { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}