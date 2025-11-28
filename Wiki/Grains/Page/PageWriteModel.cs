using Wiki.Models;

namespace Wiki.Grains.Page;

[GenerateSerializer]
[Alias("PageWriteModel")]
public class PageWriteModel
{
    [Id(0)]
    public string Type { get; set; } = nameof(PageType.Note).ToLower();
    [Id(1)]
    public string Markdown { get; set; } = "";
    [Id(2)]
    public string? Category { get; set; }
    [Id(3)]
    public string? Tags { get; set; }
    [Id(4)]
    public bool IsPinned { get; set; }
    [Id(5)]
    public DateTime? CreatedAt { get; set; }
}