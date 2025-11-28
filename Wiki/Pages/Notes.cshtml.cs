using Wiki.Grains.PageIndex;
using Wiki.Models;

namespace Wiki.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class NotesModel(IGrainFactory grainFactory) : PageModel
{
    public string? Query { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }

    public List<NoteListItem> Notes { get; set; } = [];

    public async Task OnGetAsync(string? q, string? category, string? tag)
    {
        Query = q;
        Category = category;
        Tag = tag;

        // var queryable = db.Pages
        //     .AsNoTracking()
        //     .Where(p => p.Type == PageType.Note);

        // if (!string.IsNullOrWhiteSpace(Query))
        // {
        //     queryable = queryable.Where(n =>
        //         n.Title.Contains(Query, StringComparison.OrdinalIgnoreCase) ||
        //         (n.Summary?.Contains(Query, StringComparison.OrdinalIgnoreCase) ?? false));
        // }
        //
        // if (!string.IsNullOrWhiteSpace(Category))
        // {
        //     queryable = queryable.Where(n =>
        //         n.Category != null &&
        //         n.Category.Contains(Category, StringComparison.OrdinalIgnoreCase));
        // }
        //
        // if (!string.IsNullOrWhiteSpace(Tag))
        // {
        //     queryable = queryable.Where(n =>
        //         n.Tags != null &&
        //         n.Tags.Any(t => t.Equals(Tag, StringComparison.OrdinalIgnoreCase)));
        // }

        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        Notes = (await pageIndexGrain.GetByType(nameof(PageType.Note).ToLower())).Select(p => new NoteListItem
        {
            Title = p.Title,
            Slug = "",
            UpdatedAt = p.UpdatedAt,
            Category = p.Category,
            Excerpt = p.Excerpt,
            Tags = p.Tags,
            IsPinned = p.IsPinned
        }).ToList();
    }
}

public class NoteListItem
{
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public DateTime UpdatedAt { get; set; }

    public string? Category { get; set; }
    public string? Excerpt { get; set; }

    public IEnumerable<string>? Tags { get; set; }
    public bool IsPinned { get; set; }
}
