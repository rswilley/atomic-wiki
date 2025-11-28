using Wiki.Models;
using Wiki.Services;

namespace Wiki.Grains.PageIndex;

[Alias("Wiki.Grains.PageIndex.IPageIndexGrain")]
public interface IPageIndexGrain : IGrainWithStringKey
{
    [Alias("GetByType")]
    Task<List<NoteIndexEntry>> GetByType(string type);
}

public class PageIndexGrain(
    [PersistentState(
        stateName: "page_index",
        storageName: "local")]
    IPersistentState<NoteIndexState> profile,
    IGrainContext grainContext,
    IPageStore pageStore,
    IRenderService renderService) : IGrainBase, IPageIndexGrain
{
    public IGrainContext GrainContext { get; } = grainContext;

    public async Task OnActivateAsync(CancellationToken cancellationToke)
    {
        var firstBoot = profile.State.LastIndexDate == null;
        if (firstBoot)
        {
            var allPages = await pageStore.GetAll();
            foreach (var page in allPages)
            {
                profile.State.Pages.Add(page.Meta.PermanentId, new NoteIndexEntry
                {
                    Title = page.Meta.Title,
                    Type = page.Meta.Type.ToLower(),
                    CreatedAt = page.Meta.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = page.Meta.UpdatedAt ?? DateTime.UtcNow,
                    Category = page.Meta.Category,
                    Tags = page.Meta.Tags ?? [],
                    IsPinned = page.Meta.Pinned ?? false,
                    Excerpt = renderService.GetExcerpt(page.Html)
                });
            }
            
            profile.State.LastIndexDate = DateTime.UtcNow;
            await profile.WriteStateAsync(cancellationToke);
        }
    }
    
    public Task<List<NoteIndexEntry>> GetByType(string type)
    {
        var results = profile.State.Pages.Values
            .Where(e => string.Equals(e.Type, type, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.UpdatedAt)
            .ToList();
        return Task.FromResult(results);
    }
}

[GenerateSerializer]
[Alias("Wiki.Grains.PageIndex.NoteIndexEntry")]
public class NoteIndexEntry
{
    [Id(0)]
    public required string Title { get; set; } = "";
    [Id(1)]
    public required  string Type { get; set; } = nameof(PageType.Note).ToLower();
    [Id(2)]
    public required DateTime CreatedAt { get; set; }
    [Id(3)]
    public required DateTime UpdatedAt { get; set; }
    [Id(4)]
    public string? Category { get; set; }
    [Id(5)]
    public required List<string> Tags { get; set; } = [];
    [Id(6)]
    public required bool IsPinned { get; set; }
    [Id(7)]
    public required string Excerpt { get; set; } = "";
}

public class NoteIndexState
{
    // permanentId -> post
    public Dictionary<string, NoteIndexEntry> Pages { get; set; } = [];
    public DateTime? LastIndexDate { get; set; }
}