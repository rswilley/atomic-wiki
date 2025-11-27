namespace Wiki.Services;

public interface ISearchService
{
    IEnumerable<SearchSnippet> Search(string searchQuery);
}

public class SearchService(ISearchStore searchStore) : ISearchService
{
    public IEnumerable<SearchSnippet> Search(string searchQuery)
    {
        var result = searchStore.Search(searchQuery);
        if (result?.Snippets?.Any() == true)
        {
            var searchResults = result.Snippets.Select(r => new SearchSnippet
            {
                PermanentId = r.PermanentId,
                Title = r.Title,
                Snippet = r.Snippet
                //Snippet = _noteService.RemoveTitle(_formatService.ToHtml(r.Snippet))
            });
            return searchResults;
        }

        return [];
    }
}