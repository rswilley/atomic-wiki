using Domain;
using Infrastructure.Actors.Page;
using Wiki.Models;

namespace Wiki.Services;

public interface IPageService
{
    Task<WikiPage?> GetPage(string id);
    Task<string> Save(PageWriteModel model);
    Task<string> Update(PageUpdateModel model);
}

public class PageService(
    IMarkdownParser markdownParser,
    IGrainFactory grainFactory) : IPageService
{
    public async Task<WikiPage?> GetPage(string id)
    {
        var pageGrain = grainFactory.GetGrain<IPageGrain>(id);
        var markdown = await pageGrain.GetContent();

        if (string.IsNullOrWhiteSpace(markdown))
        {
            return null;
        }

        var page = new WikiPage(new WikiContent(markdown, markdownParser));
        return page;
    }

    public async Task<string> Save(PageWriteModel model)
    {
        var permanentId = Guid.NewGuid().ToString();
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

        // todo handle if page name already exists

        var pageGrain = grainFactory.GetGrain<IPageGrain>(permanentId);
        await pageGrain.CreatePage(frontMatter, wikiContent.MarkdownBody);
        return permanentId;
    }

    public async Task<string> Update(PageUpdateModel model)
    {
        var wikiContent = new WikiContent(model.Markdown, markdownParser);
        var frontMatter = new ContentFrontMatter
        {
            PermanentId = model.Id,
            Title = wikiContent.GetTitle(),
            Type = model.Type.ToLower(),
            Category = model.Category,
            Tags = wikiContent.GetTags(model.Tags),
            Pinned = model.IsPinned,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var pageGrain = grainFactory.GetGrain<IPageGrain>(model.Id);
        await pageGrain.UpdatePage(frontMatter, wikiContent.MarkdownBody);
        return model.Id;
    }
}
