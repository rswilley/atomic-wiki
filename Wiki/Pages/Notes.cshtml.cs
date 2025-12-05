using Wiki.Services;

namespace Wiki.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class NotesModel(IPageService pageService) : PageModel
{
    public string? Query { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }

    public List<NoteListItem> Notes { get; set; } = [];

    public async Task OnGetAsync(string? q, string? category, string? tag)
    {
        Query = q;
        Category = category;
        Tag = tag;

        Notes = await pageService.GetNotes();
    }
}

public class NoteListItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public DateTime UpdatedAt { get; set; }

    public string? Category { get; set; }
    public string? Excerpt { get; set; }

    public IEnumerable<string>? Tags { get; set; }
    public bool IsPinned { get; set; }
}
