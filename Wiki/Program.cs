using Domain;
using Domain.Repositories;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.StartupTasks;
using Wiki.Services;
using ISearchRepository = Infrastructure.Repositories.ISearchRepository;
using MarkdownParser = Infrastructure.MarkdownParser;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = builder.Configuration.GetValue<string>("DATA_DIRECTORY");
ArgumentException.ThrowIfNullOrEmpty(dataDirectory);

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder
        .UseLocalhostClustering()
        .AddLocalFileGrainStorage("local", options =>
        {
            options.RootDirectory = Path.Combine(dataDirectory, "db");
        })
        .AddStartupTask<SeedInitialPageIndexTask>();
});

// Add services to the container.
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IPageRepository, PageRepository>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IIdService, SquidIdService>();
builder.Services.AddScoped<IMarkdownParser, MarkdownParser>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ISearchRepository, LuceneRepository>();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();