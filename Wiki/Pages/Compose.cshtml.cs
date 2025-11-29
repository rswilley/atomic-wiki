using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wiki.Models;
using Wiki.Services;

namespace Wiki.Pages;

public class ComposeModel(IPageService pageService) : PageModel
{
    private static readonly string[] AllowedTypes = [
        nameof(PageType.Note).ToLower(), 
        nameof(PageType.Post).ToLower(), 
        nameof(PageType.Journal).ToLower()
    ];

    [BindProperty]
    [Required]
    public string Type { get; set; } = nameof(PageType.Note).ToLower();

    // Journal-specific
    [BindProperty]
    [DataType(DataType.Date)]
    public DateTime? JournalDate { get; set; }

    [BindProperty]
    [Required]
    public string Markdown { get; set; } = "# Untitled";

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
            Type = nameof(PageType.Note).ToLower();
        }

        if (Type.Equals(nameof(PageType.Journal), StringComparison.CurrentCultureIgnoreCase))
        {
            JournalDate ??= DateTime.Today;
        }
    }

    public async Task<IActionResult> OnPostAsync()
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
        if (Type == nameof(PageType.Journal).ToLower() && !JournalDate.HasValue)
        {
            ModelState.AddModelError(nameof(JournalDate), "Journal entries require a date.");
        }

        if (!ModelState.IsValid)
        {
            // Ensure default date/time if coming back with errors
            if (Type == nameof(PageType.Journal).ToLower() && !JournalDate.HasValue)
                JournalDate = DateTime.Today;

            return Page();
        }

        var page = new PageWriteModel
        {
            Markdown = Markdown,
            Type = Type!,
            IsPinned = Type != nameof(PageType.Journal).ToLower() && IsPinned,
            Category = Category,
            Tags = Tags
        };

        if (Type == nameof(PageType.Journal).ToLower())
        {
            page.CreatedAt = JournalDate;
        }
        
        await pageService.Save(page);
        return RedirectBasedOnType();
    }
    
    private RedirectToPageResult RedirectBasedOnType()
    {
        if (Type.Equals(nameof(PageType.Journal), StringComparison.CurrentCultureIgnoreCase))
        {
            return RedirectToPage("/Journal");
        }

        if (Type.Equals(nameof(PageType.Post), StringComparison.CurrentCultureIgnoreCase))
        {
            return RedirectToPage("/Posts");
        }

        if (Type.Equals(nameof(PageType.Note), StringComparison.CurrentCultureIgnoreCase))
        {
            return RedirectToPage("/Notes");
        }
        
        return RedirectToPage("/Index");
    }
}