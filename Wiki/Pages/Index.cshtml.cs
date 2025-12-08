using Domain.ValueObject;
using Infrastructure.Actors.PageIndex;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages;

public class IndexModel(IGrainFactory grainFactory) : PageModel
{
    public HomeResponse HomeModel { get; set; } = new();
    
    public async Task OnGetAsync()
    {
        HomeModel = await new Handler(grainFactory).Handle();
    }

    public class Handler(IGrainFactory grainFactory)
    {
        public async Task<HomeResponse> Handle()
        {
            var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
            var recentTask = pageIndexGrain.GetRecentPages(false);
            var pinnedTask = pageIndexGrain.GetRecentPages(true);
            var countTask = pageIndexGrain.GetCount();

            await Task.WhenAll(recentTask, pinnedTask, countTask);

            return new HomeResponse
            {
                Recent = recentTask.Result.Select(ToRecentPage),
                Pinned = pinnedTask.Result.Select(ToRecentPage),
                NoteCount = countTask.Result.NoteCount,
                PostCount = countTask.Result.PostCount,
                JournalCount = countTask.Result.JournalCount,
                CategoryCount = countTask.Result.CategoryCount,
                TagCount = countTask.Result.TagCount
            };
        }

        private static HomeResponse.RecentPages ToRecentPage(PageIndexEntry indexEntry)
        {
            return new HomeResponse.RecentPages
            {
                Id = indexEntry.Id,
                Title = indexEntry.Title,
                Slug = new Slug(indexEntry.Title).SlugValue,
                UpdatedAt = indexEntry.UpdatedAt,
                Tags = string.Join(", ", indexEntry.Tags)
            };
        }
    }

    public class HomeResponse
    {
        public IEnumerable<RecentPages> Recent { get; init; } = [];
        public IEnumerable<RecentPages> Pinned { get; init; } = [];
        
        public int NoteCount { get; init; }
        public int PostCount { get; init; }
        public int JournalCount { get; init; }
        public int CategoryCount { get; init; }
        public int TagCount { get; init; }

        public class RecentPages
        {
            public required string Id { get; init; }
            public required string Title { get; init; }
            public required string Slug { get; init; }
            public DateTime? UpdatedAt { get; init; }
            public string? Tags { get; init; }       
        }
    }
}