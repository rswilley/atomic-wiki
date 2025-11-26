using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class JournalModel : PageModel
{
    public string? Query { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public List<JournalDayGroup> Groups { get; set; } = new();

    public void OnGet(string? q, DateTime? from, DateTime? to)
    {
        Query = q;
        From = from;
        To = to;

        // TODO: Replace with real DB query (where Type == "journal")
        var sample = new List<JournalEntryItem>
        {
            new JournalEntryItem
            {
                Id = 1,
                Date = DateTime.Today,
                Time = new TimeSpan(9, 15, 0),
                Title = "Morning thoughts on Atomic Wiki",
                Snippet = "Sketched the Razor Pages layout and decided on SQLite + FTS5.",
                Tags = new[] { "atomic-wiki", "dev" }
            },
            new JournalEntryItem
            {
                Id = 2,
                Date = DateTime.Today,
                Time = new TimeSpan(22, 10, 0),
                Title = "Late night mix session",
                Snippet = "Tried new transitions on the CDJ-3000s and noted some track combos.",
                Tags = new[] { "music", "dj" }
            },
            new JournalEntryItem
            {
                Id = 3,
                Date = DateTime.Today.AddDays(-1),
                Time = new TimeSpan(18, 30, 0),
                Title = "AI visuals experiment",
                Snippet = "Tweaked Deforum camera curves for smoother zooms in techno visuals.",
                Tags = new[] { "ai", "visuals" }
            },
            new JournalEntryItem
            {
                Id = 4,
                Date = DateTime.Today.AddDays(-3),
                Time = new TimeSpan(7, 45, 0),
                Title = "Tesla charging log",
                Snippet = "Logged kWh used this week; 20 miles total driving.",
                Tags = new[] { "tesla", "life" }
            }
        };

        IEnumerable<JournalEntryItem> queryable = sample;

        // Filter by date range
        if (From.HasValue)
        {
            queryable = queryable.Where(e => e.Date.Date >= From.Value.Date);
        }

        if (To.HasValue)
        {
            queryable = queryable.Where(e => e.Date.Date <= To.Value.Date);
        }

        // Filter by text query
        if (!string.IsNullOrWhiteSpace(Query))
        {
            queryable = queryable.Where(e =>
                (e.Title?.Contains(Query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (e.Snippet?.Contains(Query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // Group by day, newest first
        Groups = queryable
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Time)
            .GroupBy(e => e.Date.Date)
            .Select(g => new JournalDayGroup
            {
                Date = g.Key,
                Entries = g.ToList()
            })
            .ToList();
    }
}

public class JournalDayGroup
{
    public DateTime Date { get; set; }
    public List<JournalEntryItem> Entries { get; set; } = new();
}

public class JournalEntryItem
{
    public int Id { get; set; }

    public DateTime Date { get; set; }
    public TimeSpan? Time { get; set; }

    public string? Title { get; set; }
    public string? Snippet { get; set; }

    public string[]? Tags { get; set; }
}
