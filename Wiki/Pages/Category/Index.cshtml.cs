using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages.Category;

public class CategoriesIndexModel : PageModel
{
    public string? Query { get; set; }
    public int? MinCount { get; set; }
    public string Order { get; set; } = "name";

    public List<CategoryIndexItem> Categories { get; set; } = new();

    public void OnGet(string? q, int? minCount, string? order)
    {
        Query = q;
        MinCount = minCount;
        Order = string.IsNullOrWhiteSpace(order) ? "name" : order.ToLowerInvariant();

        // TODO: replace with real DB query aggregating categories
        var sample = new List<CategoryIndexItem>
        {
            new CategoryIndexItem
            {
                Name = "Development",
                Slug = "dev",
                Count = 12,
                Description = "C#, Orleans, telemetry pipelines, and other engineering notes.",
                LastUpdated = DateTime.Today.AddDays(-1)
            },
            new CategoryIndexItem
            {
                Name = "Visuals",
                Slug = "visuals",
                Count = 7,
                Description = "AI visuals, Deforum experiments, and animation workflows.",
                LastUpdated = DateTime.Today.AddDays(-3)
            },
            new CategoryIndexItem
            {
                Name = "Atomic Wiki",
                Slug = "atomic-wiki",
                Count = 5,
                Description = "Meta-notes and posts about Atomic Wiki itself.",
                LastUpdated = DateTime.Today.AddDays(-2)
            },
            new CategoryIndexItem
            {
                Name = "Life",
                Slug = "life",
                Count = 4,
                Description = "Tesla logs, HOA notes, and other personal life stuff.",
                LastUpdated = DateTime.Today.AddDays(-7)
            }
        };

        IEnumerable<CategoryIndexItem> queryable = sample;

        // Filter by name/description
        if (!string.IsNullOrWhiteSpace(Query))
        {
            queryable = queryable.Where(c =>
                c.Name.Contains(Query, StringComparison.OrdinalIgnoreCase) ||
                (c.Description?.Contains(Query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // Filter by minimum count
        if (MinCount.HasValue && MinCount.Value > 0)
        {
            queryable = queryable.Where(c => c.Count >= MinCount.Value);
        }

        // Ordering
        queryable = Order switch
        {
            "count-desc" => queryable.OrderByDescending(c => c.Count).ThenBy(c => c.Name),
            "recent"     => queryable.OrderByDescending(c => c.LastUpdated ?? DateTime.MinValue),
            _            => queryable.OrderBy(c => c.Name)
        };

        Categories = queryable.ToList();
    }
}

public class CategoryIndexItem
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";

    public int Count { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdated { get; set; }
}