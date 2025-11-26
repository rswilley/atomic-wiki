using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages.Tags;

public class TagListModel : PageModel
{
    [FromRoute]
    public string Slug { get; set; } = "";

    public string TagName { get; set; } = "";
    public string? Description { get; set; }

    public string? Query { get; set; }
    public string? TypeFilter { get; set; }
    public string Order { get; set; } = "desc";

    public List<TagItem> Items { get; set; } = new();

    public void OnGet(string? q, string? type, string? order)
    {
        Query = q;
        TypeFilter = type;
        Order = string.IsNullOrWhiteSpace(order) ? "desc" : order.ToLowerInvariant();

        // Map slug -> tag name (placeholder – replace with DB lookup)
        TagName = Slug.Replace("-", " ", StringComparison.OrdinalIgnoreCase);

        // Optional: human descriptions for some tags
        Description = Slug switch
        {
            "orleans"      => "All notes, posts, and journal entries related to Orleans and actor patterns.",
            "telemetry"    => "Anything about telemetry pipelines, metrics, and event ingestion.",
            "ai"           => "AI visuals, generation workflows, and experiments.",
            "atomic-wiki"  => "Meta notes and posts about Atomic Wiki itself.",
            _ => null
        };

        // TODO: Replace with real DB query where TagSlug == Slug
        var sample = new List<TagItem>
        {
            new TagItem
            {
                Title = "Orleans concurrency patterns",
                Slug = "orleans-concurrency-patterns",
                Type = "note",
                Summary = "Grain reentrancy, bulkhead pattern, and ordering concerns.",
                Category = "Development",
                UpdatedAt = DateTime.Today.AddDays(-1),
                Tags = new[] { "orleans", "telemetry" }
            },
            new TagItem
            {
                Title = "Telemetry cluster design notes",
                Slug = "telemetry-cluster-design",
                Type = "note",
                Summary = "Cluster layout, queues, and Orleans grains for telemetry.",
                Category = "Development",
                UpdatedAt = DateTime.Today.AddDays(-3),
                Tags = new[] { "telemetry" }
            },
            new TagItem
            {
                Title = "AI visual workflow for techno mixes",
                Slug = "ai-visual-workflow",
                Type = "post",
                Summary = "Deforum / SD pipeline, keyframes, and render settings.",
                Category = "Visuals",
                UpdatedAt = DateTime.Today.AddDays(-5),
                Tags = new[] { "ai", "visuals", "music" }
            }
        };

        // Filter down to items that actually have this tag
        IEnumerable<TagItem> queryable = sample.Where(i =>
            i.Tags != null &&
            i.Tags.Any(t => t.Equals(TagName, StringComparison.OrdinalIgnoreCase) ||
                            t.Equals(Slug, StringComparison.OrdinalIgnoreCase)));

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
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Type { get; set; } = "note"; // note | post | journal

    public string? Summary { get; set; }
    public string? Category { get; set; }
    public string[]? Tags { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string DisplayDate => UpdatedAt.ToString("yyyy-MM-dd");
}