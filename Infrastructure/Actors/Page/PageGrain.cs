using Domain;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Actors.Backlink;
using Infrastructure.Actors.LinkCoordinator;
using Infrastructure.Actors.Tag;
using Infrastructure.Repositories;

namespace Infrastructure.Actors.Page;

[Alias("Actors.PageGrain")]
public interface IPageGrain : IGrainWithStringKey
{
    [Alias("CreatePage")]
    Task CreatePage(ContentFrontMatter frontMatter, string markdownBody);
    [Alias("GetOutgoingLinks")]
    Task<IReadOnlyList<string>> GetOutgoingLinks();
}

public class PageGrain(
    [PersistentState(
        stateName: "page",
        storageName: "local")]
    IPersistentState<PageGrainState> profile,
    IGrainContext grainContext,
    IGrainFactory grainFactory,
    IPageRepository pageRepository,
    ISearchRepository searchRepository,
    IMarkdownParser markdownParser)
    : IGrainBase, IPageGrain
{
    public IGrainContext GrainContext { get; } = grainContext;

    public Task OnActivateAsync(CancellationToken cancellationToke)
    {
        _pageId = this.GetPrimaryKeyString();
        return Task.CompletedTask;
    }

    public async Task CreatePage(ContentFrontMatter frontMatter, string markdownBody)
    {
        var fullMarkdown = markdownParser.Serialize(frontMatter, markdownBody);
        var page = new WikiPage(new WikiContent(fullMarkdown, markdownParser), null!);
        
        await SavePageDocument(page);
        SaveSearchIndex(page);
        
        var tags = page.Content.FrontMatter.Tags ?? [];
        await HandleSetTags(tags);
        
        var newLinks = page.Content.GetOutgoingLinks();
        await HandleUpdatedOutgoingLinks(newLinks);
        
        await UpdateState(page);
    }

    public Task<IReadOnlyList<string>> GetOutgoingLinks()
    {
        IReadOnlyList<string> results = profile.State.OutgoingLinks.ToList();
        return Task.FromResult(results);
    }

    private string? _pageId;
    
    private async Task HandleUpdatedOutgoingLinks(IReadOnlyCollection<string> newLinks)
    {
        var oldLinks = profile.State.OutgoingLinks.ToHashSet(StringComparer.OrdinalIgnoreCase);
        profile.State.OutgoingLinks = newLinks.ToHashSet();
        
        var added = newLinks.Except(oldLinks).ToList();
        var removed = oldLinks.Except(newLinks).ToList();

        if (added.Count != 0 || removed.Count != 0)
        {
            var delta = new LinkDelta(_pageId!, added, removed);
            var coordinator = grainFactory.GetGrain<ILinkCoordinatorGrain>(0);
            await coordinator.ApplyDelta(delta);
        }
    }
    
    private async Task HandleSetTags(List<string> tags)
    {
        var oldTags = profile.State.Tags.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newTags = tags.Select(t => t.Trim()).Where(t => t.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var added = newTags.Except(oldTags).ToList();
        var removed = oldTags.Except(newTags).ToList();

        profile.State.Tags = tags.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (added.Count != 0 || removed.Count != 0)
        {
            var delta = new TagDelta(_pageId!, added, removed);
            foreach (var tagSlug in added.Concat(removed).Distinct())
            {
                var tagGrain = grainFactory.GetGrain<ITagGrain>(tagSlug);
                await tagGrain.ApplyDelta(delta);
            }
        }
    }

    private async Task UpdateState(WikiPage page)
    {
        profile.State.PermanentId = _pageId!;
        profile.State.Title = page.Content.FrontMatter.Title;
        profile.State.Type = page.Content.FrontMatter.Type;
        profile.State.Markdown = page.Content.Value;
        profile.State.Category = page.Content.FrontMatter.Category;
        profile.State.IsPinned = page.Content.FrontMatter.Pinned ?? false;
        profile.State.CreatedAtUtc = page.Content.FrontMatter.CreatedAt ?? DateTime.UtcNow;
        profile.State.UpdatedAtUtc = page.Content.FrontMatter.UpdatedAt ?? DateTime.UtcNow;
        
        await profile.WriteStateAsync();
    }

    private void SaveSearchIndex(WikiPage page)
    {
        searchRepository.Create(new PageSearchItem
        {
            PermanentId = _pageId!,
            Title = page.Content.FrontMatter.Title,
            Body = page.Content.Value,
            Tags = page.Content.FrontMatter.Tags?.ToList() ?? []
        });
    }

    private async Task SavePageDocument(WikiPage page)
    {
        await pageRepository.Save(page.Content.Value, page.Content.FrontMatter.Title, _pageId!);
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