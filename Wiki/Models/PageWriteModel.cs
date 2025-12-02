using Domain.Enums;
using Domain.ValueObject;

namespace Wiki.Models;

public abstract class PageModelBase
{
    public string Type { get; init; } = nameof(PageType.Note).ToLower();
    public string Markdown { get; init; } = "";
    public string? Category { get; init; }
    public string? Tags { get; init; }
    public bool IsPinned { get; init; }
    public DateTime? CreatedAt { get; set; }
}

public class PageWriteModel : PageModelBase;

public class PageUpdateModel : PageModelBase
{
    public required string Id { get; init; }
}
