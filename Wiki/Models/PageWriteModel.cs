using Domain.Enums;

namespace Wiki.Models;

public class PageWriteModel
{
    public string Type { get; set; } = nameof(PageType.Note).ToLower();
    public string Markdown { get; set; } = "";
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? CreatedAt { get; set; }
}