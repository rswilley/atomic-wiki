using System.ComponentModel.DataAnnotations;

namespace Wiki.Models;

public enum PageType
{
    Note = 1, 
    Post = 2, 
    Journal = 3
}

public class Page
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = "";
    
    [Required]
    public PageType Type { get; set; } = PageType.Note;

    [MaxLength(300)]
    public string? Summary { get; set; }

    public string Content { get; set; } = "";
    public string RenderedContent { get; set; } = "";
    
    public int? CategoryId { get; set; }

    public bool IsPinned { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Journal-specific
    public DateTime? JournalDate { get; set; }
    public TimeSpan? JournalTime { get; set; }
    
    public Guid ExternalId { get; set; } = Guid.NewGuid();
    public ICollection<PageTag> Tags { get; set; } = new List<PageTag>();
}

public class PageTag
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required, MaxLength(100)]
    public string Slug { get; set; } = "";

    public Guid ExternalId { get; set; } = Guid.NewGuid();
    public ICollection<Page> Pages { get; set; } = new List<Page>();
}