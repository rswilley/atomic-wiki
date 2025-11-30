using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages;

public class PageEditModel : PageModel
{
    private static readonly string[] AllowedTypes = { "note", "post", "journal" };

    [FromRoute]
    public string Slug { get; set; } = "";

    public bool NotFound { get; set; }

    // Core editable fields
    [BindProperty]
    [Required]
    public string Type { get; set; } = "note"; // note | post | journal

    [BindProperty]
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = "";

    [BindProperty]
    [DataType(DataType.Date)]
    public DateTime? JournalDate { get; set; }

    [BindProperty]
    [DataType(DataType.Time)]
    public TimeSpan? JournalTime { get; set; }

    [BindProperty]
    [StringLength(300)]
    public string? Summary { get; set; }

    [BindProperty]
    [Required]
    public string Content { get; set; } = "";

    [BindProperty]
    [StringLength(100)]
    public string? Category { get; set; }

    // Comma-separated tags
    [BindProperty]
    public string? Tags { get; set; }

    [BindProperty]
    public bool IsPinned { get; set; }

    // Read-only metadata for display
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void OnGet()
    {
        // TODO: replace with EF/SQLite lookup
        var page = GetSamplePages().FirstOrDefault(p =>
            p.Slug.Equals(Slug, StringComparison.OrdinalIgnoreCase));

        if (page == null)
        {
            NotFound = true;
            return;
        }

        Type = page.Type;
        Title = page.Title;
        Summary = page.Summary ?? "";
        Content = page.Content ?? "";
        Category = page.Category;
        IsPinned = page.IsPinned;

        if (page.Tags != null && page.Tags.Any())
        {
            Tags = string.Join(", ", page.Tags.Select(t => t.Name));
        }

        CreatedAt = page.CreatedAt;
        UpdatedAt = page.UpdatedAt;

        JournalDate = page.JournalDate;
        JournalTime = page.JournalTime;
    }

    public IActionResult OnPost()
    {
        if (!AllowedTypes.Contains(Type?.ToLowerInvariant() ?? ""))
        {
            ModelState.AddModelError(nameof(Type), "Invalid type.");
        }
        else
        {
            Type = Type.ToLowerInvariant();
        }

        if (Type == "journal" && !JournalDate.HasValue)
        {
            ModelState.AddModelError(nameof(JournalDate), "Journal entries require a date.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: update existing entity in DB:
        /*
        var page = await _db.Pages
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Slug == Slug);
        if (page == null) { NotFound = true; return Page(); }

        page.Type = Type;
        page.Title = Title;
        page.Summary = Summary;
        page.Content = Content;
        page.Category = Category;
        page.IsPinned = Type == "journal" ? false : IsPinned;

        page.JournalDate = (Type == "journal") ? JournalDate : null;
        page.JournalTime = (Type == "journal") ? JournalTime : null;

        // Tags: parse Tags string and update join table
        var tagNames = (Tags ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // ...sync tag entities...

        page.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        */

        // For now, pretend save succeeded and redirect to view page
        return RedirectToPage("/Page", new { slug = Slug });
    }

    // Sample backing data (same shape as in Page.cshtml.cs)
    private IEnumerable<PageEditData> GetSamplePages() =>
        new[]
        {
            new PageEditData
            {
                Title = "Orleans concurrency patterns",
                Slug = "orleans-concurrency-patterns",
                Type = "note",
                IsPinned = true,
                Summary = "Notes on grains, reentrancy, bulkhead pattern and message ordering.",
                Content = "# Orleans concurrency patterns\n\nSome markdown content.",
                Category = "Development",
                Tags = new List<PageTag>
                {
                    new PageTag { Name = "orleans", Slug = "orleans" },
                    new PageTag { Name = "telemetry", Slug = "telemetry" }
                },
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new PageEditData
            {
                Title = "2025-03-01 – Late night mix session",
                Slug = "2025-03-01-late-night-mix-session",
                Type = "journal",
                Summary = "Quick log of track combos.",
                Content = "## Tracks\n\n- Track A → Track B",
                Category = "Music",
                Tags = new List<PageTag>
                {
                    new PageTag { Name = "music", Slug = "music" },
                    new PageTag { Name = "dj", Slug = "dj" }
                },
                JournalDate = DateTime.Today,
                JournalTime = new TimeSpan(22, 15, 0),
                CreatedAt = DateTime.UtcNow.AddHours(-10),
                UpdatedAt = DateTime.UtcNow.AddHours(-9)
            }
        };
}

public class PageEditData
{
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Type { get; set; } = "note";
    public bool IsPinned { get; set; }

    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
    public List<PageTag> Tags { get; set; } = new();

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateTime? JournalDate { get; set; }
    public TimeSpan? JournalTime { get; set; }
}