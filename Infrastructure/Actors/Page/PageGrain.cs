using Domain;
using Domain.Repositories;
using Infrastructure.Actors.Backlink;
using Infrastructure.Actors.LinkCoordinator;
using Infrastructure.Actors.PageIndex;
using Infrastructure.Actors.Tag;
using Infrastructure.Repositories;

namespace Infrastructure.Actors.Page;

[Alias("Actors.PageGrain")]
public interface IPageGrain : IGrainWithStringKey
{
    [Alias("CreatePage")]
    Task CreatePage(PageMeta frontMatter, string markdownBody);

    [Alias("UpdatePage")]
    Task UpdatePage(PageMeta frontMatter, string markdownBody);

    [Alias("GetOutgoingLinks")]
    Task<IReadOnlyList<string>> GetOutgoingLinks();

    [Alias("GetContent")]
    Task<string?> GetContent();
}

public class PageGrain(
    [PersistentState(
        stateName: "pages",
        storageName: "local")]
    IPersistentState<PageGrainState> profile,
    IGrainContext grainContext,
    IGrainFactory grainFactory,
    IPageRepository pageRepository,
    ISearchRepository searchRepository,
    IMarkdownParser markdownParser,
    IIdService idService)
    : IGrainBase, IPageGrain
{
    public IGrainContext GrainContext { get; } = grainContext;

    public async Task OnActivateAsync(CancellationToken cancellationToke)
    {
        if (string.IsNullOrEmpty(profile.State.FileName))
        {
            return;
        }

        var pageMarkdown = await pageRepository.Get(profile.State.FileName);

        if (!string.IsNullOrEmpty(pageMarkdown))
        {
            _page = new WikiPage(new WikiContent(pageMarkdown, markdownParser));
        }
    }

    public async Task CreatePage(PageMeta frontMatter, string markdownBody)
    {
        var fullMarkdown = markdownParser.Serialize(frontMatter, markdownBody);
        _page = new WikiPage(new WikiContent(fullMarkdown, markdownParser));

        var fileName = GetSafeFileName(_page.Title, "md");
        var existingPage = await pageRepository.Get(fileName);
        if (!string.IsNullOrEmpty(existingPage))
        {
            fileName = DetermineFileNameOnConflict(_page.Title);
        }

        await SavePageDocument(fileName);
        SaveSearchIndex();

        await HandleSetTags([], _page.Content.FrontMatter.Tags ?? []);
        await HandleUpdatedOutgoingLinks([], _page.Content.GetOutgoingLinks());

        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        await pageIndexGrain.AddToIndex(new PageIndexEntry
        {
            Id = _page.Id,
            Title = _page.Title,
            Type = _page.Content.FrontMatter.Type,
            CreatedAt = _page.Content.FrontMatter.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = _page.Content.FrontMatter.UpdatedAt ?? DateTime.UtcNow,
            Tags = _page.Content.FrontMatter.Tags ?? [],
            IsPinned = _page.Content.FrontMatter.Pinned ?? false,
            Excerpt = _page.Content.GetExcerpt()
        });

        profile.State.Id = _page.Id;
        profile.State.FileName = fileName;
        await profile.WriteStateAsync();
    }

    public async Task UpdatePage(PageMeta frontMatter, string markdownBody)
    {
        var fullMarkdown = markdownParser.Serialize(frontMatter, markdownBody);
        var previousPage = _page;
        _page = new WikiPage(new WikiContent(fullMarkdown, markdownParser));

        if (previousPage != null && previousPage.Title != _page.Title)
        {
            var incomingFileName = GetSafeFileName(_page.Title, "md");
            await DeletePageByFileName(profile.State.FileName);
            var existingPage = await pageRepository.Get(incomingFileName);
            if (!string.IsNullOrEmpty(existingPage))
            {
                incomingFileName = DetermineFileNameOnConflict(_page.Title);
            }
            await SavePageDocument(incomingFileName);
            profile.State.FileName = incomingFileName;
        }
        else
        {
            await SavePageDocument(profile.State.FileName!);
        }

        UpdateSearchIndex();

        // Note: Tag updates are currently disabled
        //await HandleSetTags(previousPage?.Content.FrontMatter.Tags ?? [], _page.Content.FrontMatter.Tags ?? []);
        await HandleUpdatedOutgoingLinks(previousPage?.Content.GetOutgoingLinks() ?? [], _page.Content.GetOutgoingLinks());

        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        await pageIndexGrain.UpdateIndex(new PageIndexEntry
        {
            Id = _page.Id,
            Title = _page.Title,
            Type = _page.Content.FrontMatter.Type,
            CreatedAt = _page.Content.FrontMatter.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = _page.Content.FrontMatter.UpdatedAt ?? DateTime.UtcNow,
            Tags = _page.Content.FrontMatter.Tags ?? [],
            IsPinned = _page.Content.FrontMatter.Pinned ?? false,
            Excerpt = _page.Content.GetExcerpt()
        });

        profile.State.Id = _page.Id;
        await profile.WriteStateAsync();
    }

    public Task<IReadOnlyList<string>> GetOutgoingLinks()
    {
        IReadOnlyList<string> results = _page!.Content.GetOutgoingLinks().ToList();
        return Task.FromResult(results);
    }

    public Task<string?> GetContent()
    {
        return Task.FromResult(_page?.Content.Value);
    }

    private WikiPage? _page;

    private async Task HandleUpdatedOutgoingLinks(IReadOnlyCollection<string> old, IReadOnlyCollection<string> @new)
    {
        var oldLinks = old.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var added = @new.Except(oldLinks).ToList();
        var removed = oldLinks.Except(@new).ToList();

        if (added.Count != 0 || removed.Count != 0)
        {
            var delta = new LinkDelta(_page!.Id, added, removed);
            var coordinator = grainFactory.GetGrain<ILinkCoordinatorGrain>(0);
            await coordinator.ApplyDelta(delta);
        }
    }

    private async Task HandleSetTags(List<string> old, List<string> @new)
    {
        var oldTags = old.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newTags = @new.Select(t => t.Trim()).Where(t => t.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var added = newTags.Except(oldTags).ToList();
        var removed = oldTags.Except(newTags).ToList();

        if (added.Count != 0 || removed.Count != 0)
        {
            var delta = new TagDelta(_page!.Id, added, removed);
            foreach (var tagSlug in added.Concat(removed).Distinct())
            {
                var tagGrain = grainFactory.GetGrain<ITagGrain>(tagSlug);
                await tagGrain.ApplyDelta(delta);
            }
        }
    }

    private void SaveSearchIndex()
    {
        searchRepository.Create(new PageSearchItem
        {
            PermanentId = _page!.Id,
            Title = _page.Title,
            Body = _page.Content.Value,
            Tags = _page.Content.FrontMatter.Tags?.ToList() ?? []
        });
    }

    private void UpdateSearchIndex()
    {
        searchRepository.Update(new PageSearchItem
        {
            PermanentId = _page!.Id,
            Title = _page.Title,
            Body = _page.Content.Value,
            Tags = _page.Content.FrontMatter.Tags?.ToList() ?? []
        });
    }

    private async Task SavePageDocument(string fileName)
    {
        await pageRepository.Save(_page!.Content.Value, fileName);
    }

    private string DetermineFileNameOnConflict(string title)
    {
        var fileName = GetSafeFileName(title, "");
        fileName = $"{fileName}-{idService.Generate(DateTime.UtcNow.Ticks)}.md";
        return fileName;
    }

    private static string GetSafeFileName(string title, string extension)
    {
        var fileName = $"{string.Join(" ", title.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))}";
        if (!string.IsNullOrWhiteSpace(extension))
        {
            fileName = $"{fileName}.{extension}";
        }

        return fileName;
    }

    private async Task DeletePageByFileName(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return;

        await pageRepository.Delete(fileName);
    }
}

public class PageGrainState
{
    public required string? Id { get; set; }
    public required string? FileName { get; set; }
}
