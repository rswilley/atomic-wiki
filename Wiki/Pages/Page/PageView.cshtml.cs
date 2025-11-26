using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages.Page;

public class PageViewModel : PageModel
{
    [FromRoute]
    public string Slug { get; set; } = "";

    // Core fields
    public bool NotFound { get; set; }

    public string? Title { get; set; }
    public string Type { get; set; } = "note"; // note | post | journal
    public bool IsPinned { get; set; }

    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? RenderedContent { get; set; }

    public string? Category { get; set; }
    public string? CategorySlug { get; set; }

    public List<PageTag> Tags { get; set; } = new();

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Journal-specific
    public DateTime? JournalDate { get; set; }
    public TimeSpan? JournalTime { get; set; }

    public void OnGet()
    {
        // TODO: Replace with EF/SQLite lookup like:
        // var entity = _db.Pages.Include(p => p.Tags).FirstOrDefault(p => p.Slug == Slug);
        var sample = GetSamplePages().FirstOrDefault(p =>
            p.Slug.Equals(Slug, StringComparison.OrdinalIgnoreCase));

        if (sample == null)
        {
            NotFound = true;
            return;
        }

        Title = sample.Title;
        Type = sample.Type;
        IsPinned = sample.IsPinned;
        Summary = sample.Summary;
        Content = sample.Content;

        Category = sample.Category;
        CategorySlug = sample.CategorySlug;
        Tags = sample.Tags;

        CreatedAt = sample.CreatedAt;
        UpdatedAt = sample.UpdatedAt;

        JournalDate = sample.JournalDate;
        JournalTime = sample.JournalTime;

        // TODO: plug in real Markdown renderer (Markdig, etc.)
        RenderedContent = RenderMarkdown(Content ?? "");
    }

    // Stub: sample data
    private IEnumerable<PageViewData> GetSamplePages() =>
        new[]
        {
            new PageViewData
            {
                Title = "Orleans concurrency patterns",
                Slug = "orleans-concurrency-patterns",
                Type = "note",
                IsPinned = true,
                Summary = "Notes on grains, reentrancy, bulkhead pattern and message ordering.",
                Content = "# Orleans concurrency patterns\n\nSome markdown content here.",
                Category = "Development",
                CategorySlug = "dev",
                Tags = new List<PageTag>
                {
                    new PageTag { Name = "orleans", Slug = "orleans" },
                    new PageTag { Name = "telemetry", Slug = "telemetry" }
                },
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new PageViewData
            {
                Title = "2025-03-01 – Late night mix session",
                Slug = "2025-03-01-late-night-mix-session",
                Type = "journal",
                Summary = "Quick log of track combos and transitions that worked well.",
                Content = "## Tracks\n\n- Track A → Track B\n- Track C → Track D",
                JournalDate = DateTime.Today,
                JournalTime = new TimeSpan(22, 15, 0),
                Category = "Music",
                CategorySlug = "music",
                Tags = new List<PageTag>
                {
                    new PageTag { Name = "music", Slug = "music" },
                    new PageTag { Name = "dj", Slug = "dj" }
                },
                CreatedAt = DateTime.UtcNow.AddHours(-10),
                UpdatedAt = DateTime.UtcNow.AddHours(-9)
            }
        };

    // Stub markdown renderer – replace with a real one
    private string RenderMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return "";

        // You’ll likely call into Markdig here, e.g.:
        // var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        // return Markdown.ToHtml(markdown, pipeline);

        // For now, super naive placeholder:
        return markdown
            .Replace("\r\n", "\n")
            .Replace("\n\n", "<br/><br/>");
    }
}

public class PageViewData
{
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Type { get; set; } = "note";
    public bool IsPinned { get; set; }

    public string? Summary { get; set; }
    public string? Content { get; set; }

    public string? Category { get; set; }
    public string? CategorySlug { get; set; }
    public List<PageTag> Tags { get; set; } = new();

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateTime? JournalDate { get; set; }
    public TimeSpan? JournalTime { get; set; }
}

public class PageTag
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
}