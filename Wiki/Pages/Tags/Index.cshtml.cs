using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages.Tags;

public class TagsIndexModel : PageModel
{
    public string? Query { get; set; }
    public int? MinCount { get; set; }
    public string Order { get; set; } = "name";

    public List<TagIndexItem> Tags { get; set; } = new();

    public void OnGet(string? q, int? minCount, string? order)
    {
        Query = q;
        MinCount = minCount;
        Order = string.IsNullOrWhiteSpace(order) ? "name" : order.ToLowerInvariant();

        // TODO: Replace with real DB query aggregating tags (Tag table + PageTags)
        var sample = new List<TagIndexItem>
        {
            new TagIndexItem
            {
                Name = "orleans",
                Slug = "orleans",
                Count = 5,
                Description = "Actor model, grains, and concurrency notes.",
                LastUsed = DateTime.Today.AddDays(-1)
            },
            new TagIndexItem
            {
                Name = "telemetry",
                Slug = "telemetry",
                Count = 3,
                Description = "Engine hours, fault codes, metrics, and pipelines.",
                LastUsed = DateTime.Today.AddDays(-2)
            },
            new TagIndexItem
            {
                Name = "ai",
                Slug = "ai",
                Count = 8,
                Description = "AI visuals, prompts, and workflow experiments.",
                LastUsed = DateTime.Today.AddDays(-3)
            },
            new TagIndexItem
            {
                Name = "atomic-wiki",
                Slug = "atomic-wiki",
                Count = 4,
                Description = "Meta notes about the wiki itself.",
                LastUsed = DateTime.Today.AddDays(-5)
            },
            new TagIndexItem
            {
                Name = "tesla",
                Slug = "tesla",
                Count = 2,
                Description = "Charging habits, trips, and logs.",
                LastUsed = DateTime.Today.AddDays(-7)
            }
        };

        IEnumerable<TagIndexItem> queryable = sample;

        // Filter by name search
        if (!string.IsNullOrWhiteSpace(Query))
        {
            queryable = queryable.Where(t =>
                t.Name.Contains(Query, StringComparison.OrdinalIgnoreCase) ||
                (t.Description?.Contains(Query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // Filter by min count
        if (MinCount.HasValue && MinCount.Value > 0)
        {
            queryable = queryable.Where(t => t.Count >= MinCount.Value);
        }

        // Ordering
        queryable = Order switch
        {
            "count-desc" => queryable.OrderByDescending(t => t.Count).ThenBy(t => t.Name),
            "recent"     => queryable.OrderByDescending(t => t.LastUsed ?? DateTime.MinValue),
            _            => queryable.OrderBy(t => t.Name)
        };

        Tags = queryable.ToList();
    }
}

public class TagIndexItem
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";

    public int Count { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUsed { get; set; }
}