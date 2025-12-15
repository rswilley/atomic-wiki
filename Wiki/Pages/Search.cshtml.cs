using Domain.ValueObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wiki.Services;

namespace Wiki.Pages;

public class SearchModel(ISearchService searchService) : PageModel
{
    // Inputs
    public string? Query { get; set; }
    public string? TypeFilter { get; set; }
    public string? CategoryFilter { get; set; }
    public string Order { get; set; } = "relevance";

    // Outputs
    public List<SearchResultItem> Results { get; set; } = new();
    public int TotalResults => Results.Count;

    public void OnGet(string? q, string? type, string? category, string? order)
    {
        Query = q?.Trim();
        TypeFilter = string.IsNullOrWhiteSpace(type) ? null : type.ToLowerInvariant();
        CategoryFilter = category;
        Order = string.IsNullOrWhiteSpace(order) ? "relevance" : order.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(Query))
        {
            // No query: show nothing (or you could show recent items)
            Results = new List<SearchResultItem>();
            return;
        }

        // TODO: Replace with FTS5 query like:
        // SELECT PageId, rank FROM PagesIndex WHERE PagesIndex MATCH @Query
        // JOIN Pages ON Page.Id = PagesIndex.PageId
        var sample = GetSampleData();

        IEnumerable<SearchResultItem> queryable = sample;

        // Type filter
        if (!string.IsNullOrWhiteSpace(TypeFilter))
        {
            queryable = queryable.Where(r =>
                r.Type.Equals(TypeFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Category filter (simple contains for now)
        if (!string.IsNullOrWhiteSpace(CategoryFilter))
        {
            queryable = queryable.Where(r =>
                r.Category != null &&
                r.Category.Contains(CategoryFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Fake relevance scoring based on title / snippet hits
        foreach (var item in queryable)
        {
            item.Score = ComputeFakeScore(item, Query!);
        }

        // Order
        queryable = Order switch
        {
            "newest" => queryable
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt ?? DateTime.MinValue),
            "oldest" => queryable
                .OrderBy(r => r.UpdatedAt ?? r.CreatedAt ?? DateTime.MaxValue),
            _ => queryable // relevance
                .OrderByDescending(r => r.Score ?? 0)
        };

        Results = queryable.ToList();
    }
    
    public IActionResult OnPostSearchPages(string query)
    {
        var results = searchService.Search(query);
        return new JsonResult(results.Select(r => new
        {
            permanentId = r.PermanentId,
            snippet = r.Snippet,
            title = r.Title,
            slug = new Slug(r.Title).SlugValue
        }));
    }

    private List<SearchResultItem> GetSampleData() =>
        new()
        {
            new SearchResultItem
            {
                Title = "Orleans concurrency patterns",
                Slug = "orleans-concurrency-patterns",
                Type = "note",
                Category = "Development",
                CategorySlug = "dev",
                Snippet = "Notes on grains, reentrancy, bulkhead pattern and message ordering.",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Tags = new List<SearchTag>
                {
                    new SearchTag { Name = "orleans", Slug = "orleans" },
                    new SearchTag { Name = "telemetry", Slug = "telemetry" }
                }
            },
            new SearchResultItem
            {
                Title = "AI visual workflow for techno mixes",
                Slug = "ai-visual-workflow",
                Type = "post",
                Category = "Visuals",
                CategorySlug = "visuals",
                Snippet = "Deforum / SD pipeline, camera keyframes, and render settings.",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                Tags = new List<SearchTag>
                {
                    new SearchTag { Name = "ai", Slug = "ai" },
                    new SearchTag { Name = "visuals", Slug = "visuals" }
                }
            },
            new SearchResultItem
            {
                Title = "2025-03-01 – Late night mix session",
                Slug = "2025-03-01-late-night-mix-session",
                Type = "journal",
                Category = "Music",
                CategorySlug = "music",
                Snippet = "Quick log of track combos and transitions that worked well.",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-10),
                Tags = new List<SearchTag>
                {
                    new SearchTag { Name = "music", Slug = "music" },
                    new SearchTag { Name = "dj", Slug = "dj" }
                }
            }
        };

    private double ComputeFakeScore(SearchResultItem item, string query)
    {
        double score = 0;

        if (!string.IsNullOrWhiteSpace(item.Title) &&
            item.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            score += 2.0;
        }

        if (!string.IsNullOrWhiteSpace(item.Snippet) &&
            item.Snippet.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            score += 1.0;
        }

        return score;
    }
}

public class SearchResultItem
{
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Type { get; set; } = "note"; // note | post | journal

    public string? Category { get; set; }
    public string? CategorySlug { get; set; }

    public string? Snippet { get; set; }
    public List<SearchTag>? Tags { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public double? Score { get; set; }

    public string DisplayDate =>
        (UpdatedAt ?? CreatedAt ?? DateTime.MinValue).ToString("yyyy-MM-dd");
}

public class SearchTag
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
}