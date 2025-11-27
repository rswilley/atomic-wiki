using Wiki.Models;

namespace Wiki.Services;

public interface IPageService
{
    Task<string> Save(PageWriteModel page);
}

public class PageService(
    IPageStore pageStore,
    IIdService idService,
    IMarkdownService markdownService) : IPageService
{
    public async Task<string> Save(PageWriteModel page)
    {
        var permanentId = idService.Generate(DateTime.UtcNow.Ticks);
        await pageStore.Save(new PageDocument
        {
            Meta = new PageFrontMatter
            {
                PermanentId = permanentId,
                Title = markdownService.GetFirstHeader(page.Markdown) ?? "Untitled",
                Type = page.Type,
                Category = page.Category,
                Tags = GetTags(page.Tags),
                Pinned = page.IsPinned,
                CreatedAt = page.CreatedAt
            },
            Markdown = page.Markdown
        });
        return permanentId;
    }

    private static List<string>? GetTags(string? tags)
    {
        return string.IsNullOrEmpty(tags) 
            ? null 
            : tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}