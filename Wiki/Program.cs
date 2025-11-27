using Wiki.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IPageStore, FilePageStore>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IIdService, IdService>();
builder.Services.AddScoped<IMarkdownService, MarkdownService>();
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