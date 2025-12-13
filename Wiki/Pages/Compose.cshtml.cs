using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Infrastructure.Actors.PageIndex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wiki.Models;
using Wiki.Services;

namespace Wiki.Pages;

public class ComposeModel(
    IGrainFactory grainFactory,
    IPageService pageService) : PageModel
{
    private static readonly string[] AllowedTypes = [
        nameof(PageType.Note).ToLower(),
        nameof(PageType.Post).ToLower(),
        nameof(PageType.Journal).ToLower()
    ];

    [BindProperty]
    public string? PermanentId { get; set; } = "";

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

    public async Task<IActionResult> OnGetAsync(string? type, string? permanentId)
    {
        PermanentId = permanentId;
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

        if (!string.IsNullOrWhiteSpace(PermanentId))
        {
            var page = await pageService.GetPage(PermanentId);
            if (page != null)
            {
                Type = page.Content.FrontMatter.Type;
                JournalDate = Type.Equals(nameof(PageType.Journal), StringComparison.CurrentCultureIgnoreCase)
                    ? page.Content.FrontMatter.CreatedAt!
                    : null;
                Markdown = page.Content.MarkdownBody;
                Category = page.Content.FrontMatter.Category;
                Tags = string.Join(',', page.Content.FrontMatter.Tags ?? []);
                IsPinned = page.Content.FrontMatter.Pinned ?? false;
            }
            else
            {
                return RedirectToPage("View");
            }
        }

        return Page();
    }

    public IActionResult OnGetCancel(string type, string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return RedirectBasedOnType(type);
        }

        return RedirectToPage("View", new { slug });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Normalize type
        if (!AllowedTypes.Contains(Type.ToLowerInvariant()))
        {
            ModelState.AddModelError(nameof(Type), "Invalid entry type.");
        }
        else
        {
            Type = Type.ToLowerInvariant();
        }

        // Journal must have date
        if (Type.Equals(nameof(PageType.Journal), StringComparison.CurrentCultureIgnoreCase) && !JournalDate.HasValue)
        {
            ModelState.AddModelError(nameof(JournalDate), "Journal entries require a date.");
        }

        if (!ModelState.IsValid)
        {
            // Ensure default date/time if coming back with errors
            if (Type.Equals(nameof(PageType.Journal), StringComparison.CurrentCultureIgnoreCase) && !JournalDate.HasValue)
                JournalDate = DateTime.Today;

            return Page();
        }

        PageModelBase page;
        var isPinned = !Type.Equals(nameof(PageType.Journal), StringComparison.CurrentCultureIgnoreCase) && IsPinned;

        if (string.IsNullOrEmpty(PermanentId))
        {
            page = new PageWriteModel
            {
                Markdown = Markdown,
                Type = Type,
                IsPinned = isPinned,
                Category = Category,
                Tags = Tags
            };
        }
        else
        {
            page = new PageUpdateModel
            {
                Id = PermanentId,
                Markdown = Markdown,
                Type = Type,
                IsPinned = isPinned,
                Category = Category,
                Tags = Tags
            };
        }

        if (Type.Equals(nameof(PageType.Journal), StringComparison.CurrentCultureIgnoreCase))
        {
            page.CreatedAt = JournalDate;
        }

        if (string.IsNullOrEmpty(PermanentId))
        {
            await pageService.Save((PageWriteModel)page);
        }
        else
        {
            await pageService.Update((PageUpdateModel)page);
        }
        return RedirectBasedOnType(Type);
    }
    
    public async Task<IActionResult> OnGetTagSuggestions(string? q)
    {
        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        var all = (await pageIndexGrain.GetTags()).Select(t => t.Name).ToArray();

        var filtered = string.IsNullOrWhiteSpace(q)
            ? all
            : all.Where(t => t.Contains(q, StringComparison.OrdinalIgnoreCase)).ToArray();

        return new JsonResult(filtered);
    }
    
    public async Task<IActionResult> OnGetCategorySuggestions(string? q)
    {
        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        var all = (await pageIndexGrain.GetCategories()).Select(t => t.Name).ToArray();

        var filtered = string.IsNullOrWhiteSpace(q)
            ? all
            : all.Where(t => t.Contains(q, StringComparison.OrdinalIgnoreCase)).ToArray();

        return new JsonResult(filtered);
    }

    private RedirectToPageResult RedirectBasedOnType(string type)
    {
        if (type.Equals(nameof(PageType.Journal), StringComparison.CurrentCultureIgnoreCase))
        {
            return RedirectToPage("/Journal");
        }

        if (type.Equals(nameof(PageType.Post), StringComparison.CurrentCultureIgnoreCase))
        {
            return RedirectToPage("/Posts");
        }

        if (type.Equals(nameof(PageType.Note), StringComparison.CurrentCultureIgnoreCase))
        {
            return RedirectToPage("/Notes");
        }

        return RedirectToPage("/Index");
    }
}
