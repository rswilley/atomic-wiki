using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages.Category;

public class CategoryListModel : PageModel
{
    [FromRoute]
    public string Slug { get; set; } = "";

    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }

    public string? Query { get; set; }
    public string? TypeFilter { get; set; }
    public string Order { get; set; } = "desc";

    public List<CategoryItem> Items { get; set; } = new();

    public void OnGet(string? q, string? type, string? order)
    {
        Query = q;
        TypeFilter = type;
        Order = string.IsNullOrWhiteSpace(order) ? "desc" : order.ToLowerInvariant();

        // Map slug -> category name (placeholder – replace with DB lookup)
        CategoryName = Slug switch
        {
            "dev" => "Development",
            "visuals" => "Visuals",
            "atomic-wiki" => "Atomic Wiki",
            _ => Slug.Replace("-", " ", StringComparison.OrdinalIgnoreCase)
        };

        Description = Slug switch
        {
            "dev" => "Technical notes, patterns, and experiments.",
            "visuals" => "AI visuals, animation workflows, and render tests.",
            "atomic-wiki" => "Meta-notes about Atomic Wiki itself.",
            _ => null
        };

        // TODO: Replace with real DB query where CategorySlug == Slug
        var sample = new List<CategoryItem>
        {
            new CategoryItem
            {
                Title = "Orleans concurrency patterns",
                Slug = "orleans-concurrency-patterns",
                Type = "note",
                Summary = "Grain reentrancy, bulkhead pattern, and ordering concerns.",
                Tags = new[] { "orleans", "dotnet", "telemetry" },
                UpdatedAt = DateTime.Today.AddDays(-1)
            },
            new CategoryItem
            {
                Title = "Why I’m building Atomic Wiki",
                Slug = "why-im-building-atomic-wiki",
                Type = "post",
                Summary = "Thoughts on privacy, local-first tools, and rolling my own wiki.",
                Tags = new[] { "atomic-wiki", "privacy" },
                UpdatedAt = DateTime.Today.AddDays(-2)
            },
            new CategoryItem
            {
                Title = "AI visuals experiment – floating island",
                Slug = "ai-visuals-floating-island-experiment",
                Type = "journal",
                Summary = "Logged tweaks to Deforum prompts and camera curves.",
                Tags = new[] { "ai", "visuals" },
                UpdatedAt = DateTime.Today.AddDays(-3)
            }
        };

        IEnumerable<CategoryItem> queryable = sample;

        // Filter by type
        if (!string.IsNullOrWhiteSpace(TypeFilter))
        {
            queryable = queryable.Where(i =>
                i.Type.Equals(TypeFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by search
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

public class CategoryItem
{
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Type { get; set; } = "note"; // note | post | journal

    public string? Summary { get; set; }
    public string[]? Tags { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string DisplayDate =>
        UpdatedAt.ToString("yyyy-MM-dd");
}