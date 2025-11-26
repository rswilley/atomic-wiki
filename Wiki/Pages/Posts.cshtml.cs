using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class PostsModel : PageModel
{
    public string? Query { get; set; }
    public string? Tag { get; set; }
    public string Order { get; set; } = "desc";

    public List<PostListItem> Posts { get; set; } = new();

    public void OnGet(string? q, string? tag, string? order)
    {
        Query = q;
        Tag = tag;
        Order = string.IsNullOrWhiteSpace(order) ? "desc" : order.ToLowerInvariant();

        // TODO: replace with real DB query (Type == "post")
        var sample = new List<PostListItem>
        {
            new PostListItem
            {
                Title = "Why I’m building Atomic Wiki",
                Slug = "why-im-building-atomic-wiki",
                CreatedAt = DateTime.Today.AddDays(-1),
                Summary = "Thoughts on privacy, local-first tools, and why I rolled my own wiki.",
                Tags = new[] { "atomic-wiki", "privacy" }
            },
            new PostListItem
            {
                Title = "The story behind the floating island animation",
                Slug = "floating-island-animation-story",
                CreatedAt = DateTime.Today.AddDays(-3),
                Summary = "How the mystical floating island theme evolved for my techno visuals.",
                Tags = new[] { "visuals", "ai", "music" }
            },
            new PostListItem
            {
                Title = "My Tesla charging routine and why it works",
                Slug = "tesla-charging-routine",
                CreatedAt = DateTime.Today.AddDays(-5),
                Summary = "Notes on range, battery health, and low-mileage weekly driving.",
                Tags = new[] { "tesla", "life" }
            }
        };

        IEnumerable<PostListItem> queryable = sample;

        if (!string.IsNullOrWhiteSpace(Query))
        {
            queryable = queryable.Where(p =>
                p.Title.Contains(Query, StringComparison.OrdinalIgnoreCase) ||
                (p.Summary?.Contains(Query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(Tag))
        {
            queryable = queryable.Where(p =>
                p.Tags != null &&
                p.Tags.Any(t => t.Equals(Tag, StringComparison.OrdinalIgnoreCase)));
        }

        queryable = Order == "asc"
            ? queryable.OrderBy(p => p.CreatedAt)
            : queryable.OrderByDescending(p => p.CreatedAt);

        Posts = queryable.ToList();
    }
}

public class PostListItem
{
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string? Summary { get; set; }
    public string[]? Tags { get; set; }
}
