using Domain.ValueObject;
using Infrastructure.Actors.PageIndex;
using Microsoft.AspNetCore.Mvc;

namespace Wiki.Pages.Shared.Components.Tags;

public class TagsViewComponent(IGrainFactory grainFactory) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        var tags = (await pageIndexGrain.GetTags()).Select(c => new TagsList.TagsModel
        {
            Name = c.Name,
            Slug = new Slug(c.Name).SlugValue,
            PageCount = c.PageCount
        }).ToList();

        return View("Tags", tags);
    }
}

public class TagsList
{
    public required List<TagsList> Tags { get; init; }

    public class TagsModel
    {
        public required string Name { get; init; }
        public required string Slug { get; init; }
        public required int PageCount { get; init; }
    }
}
