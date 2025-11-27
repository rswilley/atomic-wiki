namespace Wiki.Models;

public class PageDocument
{
    public PageFrontMatter Meta { get; set; } = new();
    public string Markdown { get; set; } = "";
    public string Html { get; set; } = "";  // after rendering
    public string FilePath { get; set; } = "";
}

public class PageFrontMatter
{
    public string PermanentId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string Type { get; set; } = nameof(PageType.Note).ToLower();

    public string? Category { get; set; }

    public List<string>? Tags { get; set; }
    public bool? Pinned { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Dictionary<string, object>? Extra { get; set; }
}