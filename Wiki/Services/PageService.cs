using Wiki.Models;

namespace Wiki.Services;

public interface IPageService
{
    Task<string> Save(PageWriteModel page);
}

public class PageService(
    IPageStore pageStore,
    ISearchStore searchStore,
    IIdService idService,
    IMarkdownService markdownService) : IPageService
{
    public async Task<string> Save(PageWriteModel page)
    {
        var permanentId = idService.Generate(DateTime.UtcNow.Ticks);
        var title = markdownService.GetFirstHeader(page.Markdown) ?? "Untitled";
        var tags = GetTags(page.Tags);
        
        await pageStore.Save(new PageDocument
        {
            Meta = new PageFrontMatter
            {
                PermanentId = permanentId,
                Title = title,
                Type = page.Type,
                Category = page.Category,
                Tags = tags,
                Pinned = page.IsPinned,
                CreatedAt = page.CreatedAt
            },
            Markdown = page.Markdown
        });
        
        searchStore.Create(new PageSearchItem
        {
            PermanentId = permanentId,
            Title = title,
            Body = page.Markdown,
            Tags = tags ?? []
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