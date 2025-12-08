using Domain;
using Domain.Repositories;
using Infrastructure.Actors.Page;
using Infrastructure.Actors.PageIndex;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Actors.StartupTasks;

public sealed class SeedInitialPageIndexTask(
    IGrainFactory grainFactory, 
    IServiceProvider serviceProvider) : IStartupTask
{
    async Task IStartupTask.Execute(CancellationToken cancellationToken)
    {
        var pageIndexGrain = grainFactory.GetGrain<IPageIndexGrain>("index");
        var pageIndexState = await pageIndexGrain.GetState();
        if (pageIndexState.Pages.Count > 0) 
            return;
        
        using var scope = serviceProvider.CreateScope();
        var pageRepository = scope.ServiceProvider.GetRequiredService<IPageRepository>();
        var markdownParser = scope.ServiceProvider.GetRequiredService<IMarkdownParser>();
        
        var seedDataItems = await pageRepository.GetSeedData();
        foreach (var content in seedDataItems)
        {
            var page = new WikiPage(new WikiContent(content, markdownParser));
            var pageGrain = grainFactory.GetGrain<IPageGrain>(page.Id);
            await pageGrain.CreatePage(page.Content.FrontMatter, page.Content.MarkdownBody);
        }
    }
}