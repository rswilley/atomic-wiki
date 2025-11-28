using Wiki.Grains;
using Wiki.Grains.Page;
using Wiki.Models;

namespace Wiki.Services;

public interface IPageService
{
    Task<string> Save(PageWriteModel page);
}

public class PageService(
    IIdService idService,
    IGrainFactory grainFactory) : IPageService
{
    public async Task<string> Save(PageWriteModel page)
    {
        var permanentId = idService.Generate(DateTime.UtcNow.Ticks);
        var pageGrain = grainFactory.GetGrain<IPageGrain>(permanentId);
        await pageGrain.CreatePage(page);
        return permanentId;
    }
}