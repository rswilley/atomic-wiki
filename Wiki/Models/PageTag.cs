using System.ComponentModel.DataAnnotations;

namespace Wiki.Models;

public class PageTag
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required, MaxLength(100)]
    public string Slug { get; set; } = "";

    public Guid ExternalId { get; set; } = Guid.NewGuid();
}