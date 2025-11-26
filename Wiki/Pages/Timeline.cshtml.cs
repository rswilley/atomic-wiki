using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages;

public class TimelineModel : PageModel
{
    public string? Query { get; set; }
    public List<TimelineGroup> Groups { get; set; } = new();

    public void OnGet(string? q)
    {
        Query = q;

        // TODO: Replace with real data (filtered & sorted)
        var sample = new List<TimelineItem>
        {
            new TimelineItem
            {
                Date = DateTime.Today,
                Time = new TimeSpan(9, 30, 0),
                Title = "Telemetry cluster design notes",
                Slug = "telemetry-cluster-design",
                Type = "note",
                Tags = new[] { "telemetry", "orleans" },
                Summary = "Refinement of Orleans grains, bulkhead pattern, and event flow.",
                IsPinned = false
            },
            new TimelineItem
            {
                Date = DateTime.Today,
                Time = new TimeSpan(21, 15, 0),
                Title = "AI visual workflow for techno mixes",
                Slug = "ai-visual-workflow",
                Type = "note",
                Tags = new[] { "ai", "visuals" },
                Summary = "Updated Deforum prompt strategy and camera keyframe patterns.",
                IsPinned = true
            },
            new TimelineItem
            {
                Date = DateTime.Today.AddDays(-1),
                Time = new TimeSpan(18, 0, 0),
                Title = "Atomic Wiki architecture",
                Slug = "atomic-wiki-architecture",
                Type = "note",
                Tags = new[] { "wiki", "architecture" },
                Summary = "SQLite + FTS5, Razor Pages, local-only setup.",
                IsPinned = false
            }
        };

        Groups = sample
            .GroupBy(x => x.Date.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new TimelineGroup
            {
                Date = g.Key,
                Items = g.OrderByDescending(x => x.Time).ToList()
            })
            .ToList();
    }
}

public class TimelineGroup
{
    public DateTime Date { get; set; }
    public List<TimelineItem> Items { get; set; } = new();
}

public class TimelineItem
{
    public DateTime Date { get; set; }
    public TimeSpan? Time { get; set; }

    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Type { get; set; } = "note";

    public string? Summary { get; set; }
    public bool IsPinned { get; set; }

    public string[]? Tags { get; set; }
}