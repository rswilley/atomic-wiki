using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages;

public class ComposeModel : PageModel
{
    private static readonly string[] AllowedTypes = { "note", "post", "journal" };

    [BindProperty]
    [Required]
    public string Type { get; set; } = "note"; // note | post | journal

    [BindProperty]
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = "";

    // Journal-specific
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

    // Comma-separated tags input; you’ll parse to Tag entities later
    [BindProperty]
    public string? Tags { get; set; }

    [BindProperty]
    public bool IsPinned { get; set; }

    public void OnGet(string? type)
    {
        if (!string.IsNullOrWhiteSpace(type) &&
            AllowedTypes.Contains(type.ToLowerInvariant()))
        {
            Type = type.ToLowerInvariant();
        }
        else
        {
            Type = "note";
        }

        if (Type == "journal")
        {
            JournalDate ??= DateTime.Today;
            // optional: default time to now
            JournalTime ??= DateTime.Now.TimeOfDay;
        }
    }

    public IActionResult OnPost()
    {
        // Normalize type
        if (!AllowedTypes.Contains(Type?.ToLowerInvariant() ?? ""))
        {
            ModelState.AddModelError(nameof(Type), "Invalid entry type.");
        }
        else
        {
            Type = Type.ToLowerInvariant();
        }

        // Journal must have date
        if (Type == "journal" && !JournalDate.HasValue)
        {
            ModelState.AddModelError(nameof(JournalDate), "Journal entries require a date.");
        }

        if (!ModelState.IsValid)
        {
            // Ensure default date/time if coming back with errors
            if (Type == "journal" && !JournalDate.HasValue)
                JournalDate = DateTime.Today;

            return Page();
        }

        // TODO: map this to your Page entity and persist (EF/SQLite)
        // Pseudo-code:
        /*
    var page = new Page
    {
        Title = Title,
        Slug = SlugGenerator.FromTitle(Title),
        Content = Content,
        Type = Type, // "note", "post", or "journal"
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsPinned = Type == "journal" ? false : IsPinned,
        Summary = Summary,
        Category = Category,
        Tags = ParseTags(Tags)
    };

    if (Type == "journal")
    {
        page.JournalDate = JournalDate.Value;
        page.JournalTime = JournalTime;
    }

    _db.Pages.Add(page);
    await _db.SaveChangesAsync();
    */

        // For now, just redirect somewhere; later redirect to the created page
        return RedirectToPage("/Index");
    }
}