using Domain;
using Wiki.Grains.Page;
using Wiki.Models;

namespace Wiki.Services;

public interface IPageService
{
    Task<string> Save(PageWriteModel model);
}

public class PageService(
    IIdService idService,
    IMarkdownParser markdownParser,
    IGrainFactory grainFactory) : IPageService
{
    public async Task<string> Save(PageWriteModel model)
    {
        var permanentId = idService.Generate(DateTime.UtcNow.Ticks);
        var wikiContent = new WikiContent(model.Markdown, markdownParser);
        var frontMatter = new ContentFrontMatter
        {
            PermanentId = permanentId,
            Title = wikiContent.GetTitle(),
            Type = model.Type.ToLower(),
            Category = model.Category,
            Tags = wikiContent.GetTags(model.Tags),
            Pinned = model.IsPinned,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var pageGrain = grainFactory.GetGrain<IPageGrain>(permanentId);
        await pageGrain.CreatePage(frontMatter, wikiContent.MarkdownBody);
        return permanentId;
    }
}