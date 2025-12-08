using Domain.ValueObject;
using Infrastructure.Actors.PageIndex;
using Microsoft.AspNetCore.Mvc;

namespace Wiki.Pages.Shared.Components.Categories;

public class CategoriesViewComponent(IGrainFactory grainFactory) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        var categories = (await pageIndexGrain.GetCategories()).Select(c => new CategoriesList.CategoriesModel
        {
            Name = c.Name,
            Slug = new Slug(c.Name).SlugValue,
            PageCount = c.PageCount
        }).ToList();

        return View("Categories", categories);
    }
}

public class CategoriesList
{
    public required List<CategoriesModel> Categories { get; init; }

    public class CategoriesModel
    {
        public required string Name { get; init; }
        public required string Slug { get; init; }
        public required int PageCount { get; init; }
    }
}
