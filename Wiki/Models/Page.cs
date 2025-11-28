namespace Wiki.Models;

public enum PageType
{
    Note = 1, 
    Post = 2, 
    Journal = 3
}

public class Page
{
    public required string PermanentId { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    
    public string Type { get; set; } = nameof(PageType.Note).ToLower();
    public string? Category { get; set; }
    public string? Tags { get; set; }
    
    public string Markdown { get; set; } = "";
    public string RenderedHtml { get; set; } = "";

    public bool IsPinned { get; set; }
    
    // Journal-specific
    public DateTime? JournalDate { get; set; }
}