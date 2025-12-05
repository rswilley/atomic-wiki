using Wiki.Services;

namespace Wiki.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class JournalModel(IPageService pageService) : PageModel
{
    public string? Query { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public List<JournalDayGroup> Groups { get; set; } = new();

    public async Task OnGetAsync(string? q, DateTime? from, DateTime? to)
    {
        Query = q;
        From = from;
        To = to;

        var journals = await pageService.GetJournals();

        // Group by day, newest first
        Groups = journals
            .OrderByDescending(e => e.Date)
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
    public required string Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public DateTime Date { get; set; }
    public string? Excerpt { get; set; }
    public IEnumerable<string>? Tags { get; set; }
}
