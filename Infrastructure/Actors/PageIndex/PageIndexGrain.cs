using Domain.Enums;

namespace Infrastructure.Actors.PageIndex;

[Alias("Actors.PageIndex")]
public interface IPageIndexGrain : IGrainWithStringKey
{
    [Alias("GetRecentPages")]
    Task<List<PageIndexEntry>> GetRecentPages(bool isPinned);

    [Alias("GetCount")]
    Task<PageIndexCount> GetCount();
    
    [Alias("GetById")]
    Task<PageIndexEntry?> GetById(string id);
    
    [Alias("GetByType")]
    Task<List<PageIndexEntry>> GetByType(string type);

    [Alias("AddToIndex")]
    Task AddToIndex(PageIndexEntry entry);

    [Alias("UpdateIndex")]
    Task UpdateIndex(PageIndexEntry entry);

    [Alias("GetState")]
    Task<PageIndexState> GetState();
}

public class PageIndexGrain(
    [PersistentState(
        stateName: "page_index",
        storageName: "local")]
    IPersistentState<PageIndexState> profile,
    IGrainContext grainContext) : IGrainBase, IPageIndexGrain
{
    public IGrainContext GrainContext { get; } = grainContext;

    public Task<List<PageIndexEntry>> GetRecentPages(bool isPinned)
    {
        return Task.FromResult(
            profile.State.Pages
                .Where(e => e.Value.IsPinned == isPinned)
                .OrderByDescending(e => e.Value.UpdatedAt)
                .Select(e => e.Value)
                .Take(5)
                .ToList()
        );
    }

    public Task<PageIndexCount> GetCount()
    {
        var noteCount = profile.State.Pages.Values.Count(e => e.Type.Equals(nameof(PageType.Note), StringComparison.CurrentCultureIgnoreCase));
        var postCount = profile.State.Pages.Values.Count(e => e.Type.Equals(nameof(PageType.Post), StringComparison.CurrentCultureIgnoreCase));
        var journalCount = profile.State.Pages.Values.Count(e => e.Type.Equals(nameof(PageType.Journal), StringComparison.CurrentCultureIgnoreCase));
        var categoryCount = profile.State.Pages.Values.SelectMany(e => e.Category ?? "").Distinct().Count();
        var tagCount = profile.State.Pages.Values.SelectMany(e => e.Tags).Distinct().Count();

        return Task.FromResult(new PageIndexCount
        {
            NoteCount = noteCount,
            PostCount = postCount,
            JournalCount = journalCount,
            CategoryCount = categoryCount,
            TagCount = tagCount
        });
    }

    public Task<PageIndexEntry?> GetById(string id)
    {
        return Task.FromResult(profile.State.Pages.TryGetValue(id, out var entry) ? entry : null);
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

    public Task<PageIndexState> GetState()
    {
        return Task.FromResult(profile.State);
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

[GenerateSerializer]
[Alias("Actors.PageIndex.PageIndexCount")]
public class PageIndexCount
{
    [Id(0)]
    public int NoteCount { get; init; }
    [Id(1)]
    public int PostCount { get; init; }
    [Id(2)]
    public int JournalCount { get; init; }
    [Id(3)]
    public int CategoryCount { get; init; }
    [Id(4)]
    public int TagCount { get; init; }
}

[GenerateSerializer]
[Alias("Actors.PageIndex.PageIndexState")]
public class PageIndexState
{
    // permanentId -> page
    [Id(0)]
    public Dictionary<string, PageIndexEntry> Pages { get; set; } = [];
}
