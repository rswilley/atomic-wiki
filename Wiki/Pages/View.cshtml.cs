using Domain.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wiki.Services;

namespace Wiki.Pages;

public class PageViewModel(
    IPageService pageService) : PageModel
{
    [FromRoute]
    public string? PermanentId { get; set; } = "";

    [FromRoute]
    public string Slug { get; set; } = "";

    // Core fields
    public bool NotFound { get; set; }

    public string? Title { get; set; }
    public string Type { get; set; } = "note"; // note | post | journal
    public bool IsPinned { get; set; }

    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? RenderedContent { get; set; }

    public string? Category { get; set; }
    public string? CategorySlug { get; set; }

    public List<PageTag> Tags { get; set; } = new();

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public async Task OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(PermanentId))
        {
            NotFound = true;
            return;
        }

        var page = await pageService.GetPage(PermanentId);
        if (page == null)
        {
            NotFound = true;
            return;
        }

        Title = page.Content.FrontMatter.Title;
        Type = page.Content.FrontMatter.Type;
        IsPinned = page.Content.FrontMatter.Pinned ?? false;
        Excerpt = page.Content.GetExcerpt();
        Content = page.Content.MarkdownBody;

        Category = page.Content.FrontMatter.Category;
        CategorySlug = page.Content.FrontMatter.Category?.ToSlug();
        Tags = page.Content.FrontMatter.Tags?.Select(tag => new PageTag
        {
            Name = tag,
            Slug = tag.ToSlug()
        }).ToList() ?? [];

        CreatedAt = page.Content.FrontMatter.CreatedAt;
        UpdatedAt = page.Content.FrontMatter.UpdatedAt;

        RenderedContent = page.Content.RemoveTitleFromHtml();
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
