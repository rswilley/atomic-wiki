using Domain;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Actors.Page;
using Infrastructure.Actors.PageIndex;
using Wiki.Models;
using Wiki.Pages;

namespace Wiki.Services;

public interface IPageService
{
    Task<WikiPage?> GetPage(string id);
    Task<List<NoteListItem>> GetNotes();
    Task<List<JournalEntryItem>> GetJournals();
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

    public async Task<List<NoteListItem>> GetNotes()
    {
        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        return (await pageIndexGrain.GetByType(nameof(PageType.Note).ToLower())).Select(p => new NoteListItem
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Title.ToSlug(),
            UpdatedAt = p.UpdatedAt,
            Category = p.Category,
            Excerpt = p.Excerpt,
            Tags = p.Tags,
            IsPinned = p.IsPinned
        }).ToList();
    }

    public async Task<List<JournalEntryItem>> GetJournals()
    {
        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        return (await pageIndexGrain.GetByType(nameof(PageType.Journal).ToLower())).Select(p => new JournalEntryItem
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Title.ToSlug(),
            Date = p.CreatedAt,
            Excerpt = p.Excerpt,
            Tags = p.Tags
        }).ToList();
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

        var pageGrain = grainFactory.GetGrain<IPageGrain>(permanentId);
        await pageGrain.CreatePage(frontMatter, wikiContent.MarkdownBody);
        return permanentId;
    }

    public async Task<string> Update(PageUpdateModel model)
    {
        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        var existingPageIndex = await pageIndexGrain.GetById(model.Id);
        
        var wikiContent = new WikiContent(model.Markdown, markdownParser);
        var frontMatter = new ContentFrontMatter
        {
            PermanentId = model.Id,
            Title = wikiContent.GetTitle(),
            Type = model.Type.ToLower(),
            Category = model.Category,
            Tags = wikiContent.GetTags(model.Tags),
            Pinned = model.IsPinned,
            CreatedAt = existingPageIndex?.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        var pageGrain = grainFactory.GetGrain<IPageGrain>(model.Id);
        await pageGrain.UpdatePage(frontMatter, wikiContent.MarkdownBody);
        return model.Id;
    }
}
