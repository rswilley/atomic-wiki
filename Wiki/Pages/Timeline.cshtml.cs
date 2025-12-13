using Microsoft.AspNetCore.Mvc.RazorPages;
using Wiki.Services;

namespace Wiki.Pages;

public class TimelineModel(IPageService pageService) : PageModel
{
    public string? Query { get; set; }
    public List<TimelineGroup> Groups { get; set; } = new();

    public async Task OnGetAsync(string? q)
    {
        Query = q;

        Groups = await pageService.GetTimeline();
    }
}

public class TimelineGroup
{
    public DateTime Date { get; set; }
    public List<TimelineItem> Items { get; set; } = new();
}

public class TimelineItem
{
    public required string Id { get; set; }
    public DateTime Date { get; set; }

    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Type { get; set; } = "note";

    public string? Excerpt { get; set; }
    public bool IsPinned { get; set; }

    public IEnumerable<string>? Tags { get; set; }
}