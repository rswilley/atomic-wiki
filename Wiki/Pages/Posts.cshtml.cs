using Wiki.Services;

namespace Wiki.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class PostsModel(IPageService pageService) : PageModel
{
    public string? Query { get; set; }
    public string? Tag { get; set; }
    public string Order { get; set; } = "desc";

    public List<PostListItem> Posts { get; set; } = new();

    public async Task OnGetAsync(string? q, string? tag, string? order)
    {
        Query = q;
        Tag = tag;
        Order = string.IsNullOrWhiteSpace(order) ? "desc" : order.ToLowerInvariant();

        Posts = await pageService.GetPosts();
    }
}

public class PostListItem
{
    public required string Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string? Excerpt { get; set; }
    public IEnumerable<string>? Tags { get; set; }
}
