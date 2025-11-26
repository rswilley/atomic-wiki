using Microsoft.EntityFrameworkCore;
using Wiki.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// EF Core + SQLite
var dataDirectory = builder.Configuration.GetValue<string>("DATA_DIRECTORY");
ArgumentException.ThrowIfNullOrEmpty(dataDirectory);
var connectionString = $"Data Source={Path.Combine(dataDirectory, "atomicwiki.db")}";

builder.Services.AddDbContext<AtomicWikiDbContext>(options =>
    options.UseSqlite(connectionString));

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