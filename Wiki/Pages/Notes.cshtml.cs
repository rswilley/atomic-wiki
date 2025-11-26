using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class NotesModel : PageModel
{
    public string? Query { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }

    public List<NoteListItem> Notes { get; set; } = new();

    public void OnGet(string? q, string? category, string? tag)
    {
        Query = q;
        Category = category;
        Tag = tag;

        // TODO: replace with real DB query where Type == "note"
        var sample = new List<NoteListItem>
        {
            new NoteListItem
            {
                Title = "Orleans concurrency patterns",
                Slug = "orleans-concurrency-patterns",
                UpdatedAt = DateTime.Today,
                Category = "Development",
                Summary = "Notes on grains, reentrancy, bulkhead pattern and message ordering.",
                Tags = new[] { "orleans", "dotnet", "telemetry" },
                IsPinned = true
            },
            new NoteListItem
            {
                Title = "AI visual workflow for techno mixes",
                Slug = "ai-visual-workflow",
                UpdatedAt = DateTime.Today.AddDays(-1),
                Category = "Visuals",
                Summary = "Deforum / SD pipeline, keyframes, and render settings.",
                Tags = new[] { "ai", "visuals", "music" }
            },
            new NoteListItem
            {
                Title = "Atomic Wiki architecture",
                Slug = "atomic-wiki-architecture",
                UpdatedAt = DateTime.Today.AddDays(-3),
                Category = "Atomic Wiki",
                Summary = "SQLite + FTS5, Razor Pages layout, local-only design.",
                Tags = new[] { "atomic-wiki", "architecture" }
            }
        };

        IEnumerable<NoteListItem> queryable = sample;

        if (!string.IsNullOrWhiteSpace(Query))
        {
            queryable = queryable.Where(n =>
                n.Title.Contains(Query, StringComparison.OrdinalIgnoreCase) ||
                (n.Summary?.Contains(Query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(Category))
        {
            queryable = queryable.Where(n =>
                n.Category != null &&
                n.Category.Contains(Category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(Tag))
        {
            queryable = queryable.Where(n =>
                n.Tags != null &&
                n.Tags.Any(t => t.Equals(Tag, StringComparison.OrdinalIgnoreCase)));
        }

        Notes = queryable
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .ToList();
    }
}

public class NoteListItem
{
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public DateTime UpdatedAt { get; set; }

    public string? Category { get; set; }
    public string? Summary { get; set; }

    public string[]? Tags { get; set; }
    public bool IsPinned { get; set; }
}
