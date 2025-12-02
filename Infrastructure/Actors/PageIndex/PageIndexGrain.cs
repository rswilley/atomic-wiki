using Domain;
using Domain.Enums;
using Domain.Repositories;

namespace Infrastructure.Actors.PageIndex;

[Alias("Actors.PageIndex")]
public interface IPageIndexGrain : IGrainWithStringKey
{
    [Alias("GetByType")]
    Task<List<PageIndexEntry>> GetByType(string type);

    [Alias("AddToIndex")]
    Task AddToIndex(PageIndexEntry entry);

    [Alias("UpdateIndex")]
    Task UpdateIndex(PageIndexEntry entry);
}

public class PageIndexGrain(
    [PersistentState(
        stateName: "page_index",
        storageName: "local")]
    IPersistentState<NoteIndexState> profile,
    IGrainContext grainContext,
    IPageRepository pageRepository,
    IMarkdownParser markdownParser) : IGrainBase, IPageIndexGrain
{
    public IGrainContext GrainContext { get; } = grainContext;

    public async Task OnActivateAsync(CancellationToken cancellationToke)
    {
        var firstBoot = profile.State.LastIndexDate == null;
        if (firstBoot)
        {
            var allContent = await pageRepository.GetAll();
            foreach (var content in allContent)
            {
                var page = new WikiPage(new WikiContent(content, markdownParser));
                profile.State.Pages.Add(page.Id, new PageIndexEntry
                {
                    Id = page.Id,
                    Title = page.Content.FrontMatter.Title,
                    Type = page.Content.FrontMatter.Type.ToLower(),
                    CreatedAt = page.Content.FrontMatter.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = page.Content.FrontMatter.UpdatedAt ?? DateTime.UtcNow,
                    Category = page.Content.FrontMatter.Category,
                    Tags = page.Content.FrontMatter.Tags ?? [],
                    IsPinned = page.Content.FrontMatter.Pinned ?? false,
                    Excerpt = page.Content.GetExcerpt()
                });
            }

            profile.State.LastIndexDate = DateTime.UtcNow;
            await profile.WriteStateAsync(cancellationToke);
        }
    }

    public Task<List<PageIndexEntry>> GetByType(string type)
    {
        var results = profile.State.Pages.Values
            .Where(e => string.Equals(e.Type, type, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.UpdatedAt)
            .ToList();
        return Task.FromResult(results);
    }

    public async Task AddToIndex(PageIndexEntry entry)
    {
        profile.State.Pages.Add(entry.Id, entry);
        await profile.WriteStateAsync();
    }

    public async Task UpdateIndex(PageIndexEntry entry)
    {
        profile.State.Pages[entry.Id] = entry;
        await profile.WriteStateAsync();
    }
}

[GenerateSerializer]
[Alias("Actors.PageIndex.PageIndexEntry")]
public class PageIndexEntry
{
    [Id(0)]
    public required string Id { get; set; }
    [Id(1)]
    public required string Title { get; set; } = "";
    [Id(2)]
    public required  string Type { get; set; } = nameof(PageType.Note).ToLower();
    [Id(3)]
    public required DateTime CreatedAt { get; set; }
    [Id(4)]
    public required DateTime UpdatedAt { get; set; }
    [Id(5)]
    public string? Category { get; set; }
    [Id(6)]
    public required List<string> Tags { get; set; } = [];
    [Id(7)]
    public required bool IsPinned { get; set; }
    [Id(8)]
    public required string Excerpt { get; set; } = "";
}

public class NoteIndexState
{
    // permanentId -> page
    public Dictionary<string, PageIndexEntry> Pages { get; set; } = [];
    public DateTime? LastIndexDate { get; set; }
}
