using Domain.ValueObject;
using Infrastructure.Actors.PageIndex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages.Tags;

public class TagListModel(IGrainFactory grainFactory) : PageModel
{
    [FromRoute]
    public string Slug { get; set; } = "";

    public string TagName { get; set; } = "";
    public string? Description { get; set; }

    public string? Query { get; set; }
    public string? TypeFilter { get; set; }
    public string Order { get; set; } = "desc";

    public List<TagItem> Items { get; set; } = new();

    public async Task OnGetAsync(string? q, string? type, string? order)
    {
        Query = q;
        TypeFilter = type;
        Order = string.IsNullOrWhiteSpace(order) ? "desc" : order.ToLowerInvariant();

        // Map slug -> tag name (placeholder – replace with DB lookup)
        TagName = Slug.Replace("-", " ", StringComparison.OrdinalIgnoreCase);

        // Filter down to items that actually have this tag
        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        var state = await pageIndexGrain.GetState();
        var queryable = state.Pages.Values
            .Where(e => Slug == "untagged" ? e.Tags is [] : e.Tags.Contains(Slug))
            .Select(e => new TagItem
            {
                PermanentId = e.Id,
                Title = e.Title,
                Slug = new Slug(e.Title).SlugValue,
                Type = e.Type,
                Summary = e.Excerpt,
                Category = e.Category,
                Tags = e.Tags,
                UpdatedAt = e.UpdatedAt
            });

        // Filter by type
        if (!string.IsNullOrWhiteSpace(TypeFilter))
        {
            queryable = queryable.Where(i =>
                i.Type.Equals(TypeFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(Query))
        {
            queryable = queryable.Where(i =>
                i.Title.Contains(Query, StringComparison.OrdinalIgnoreCase) ||
                (i.Summary?.Contains(Query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // Sort
        queryable = Order == "asc"
            ? queryable.OrderBy(i => i.UpdatedAt)
            : queryable.OrderByDescending(i => i.UpdatedAt);

        Items = queryable.ToList();
    }
}

public class TagItem
{
    public required string PermanentId { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Type { get; set; } = "note"; // note | post | journal

    public string? Summary { get; set; }
    public string? Category { get; set; }
    public IEnumerable<string>? Tags { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string DisplayDate => UpdatedAt.ToString("yyyy-MM-dd");
}
