using System.Text;
using Wiki.Extensions;
using Wiki.Models;

namespace Wiki.Services;

public interface IPageStore
{
    Task<List<PageDocument>> GetAll();
    Task Save(PageDocument doc);
}

public class FilePageStore(
    IConfigurationService configurationService,
    IMarkdownService markdownService) : IPageStore
{
    public async Task<List<PageDocument>> GetAll()
    {
        var docs = new List<PageDocument>();
        var outputDirectory = CreateOutputDirectoryIfNotExists();
        
        foreach (var file in Directory.GetFiles(outputDirectory, "*.md", SearchOption.TopDirectoryOnly))
        {
            var doc = await LoadFromFileAsync(file);
            if (doc != null)
            {
                docs.Add(doc);
            }
        }

        return docs;
    }

    private string CreateOutputDirectoryIfNotExists()
    {
        var directory = Path.Combine(configurationService.GetDataDirectory(), "output");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return directory;
    }

    public async Task Save(PageDocument doc)
    {
        var pagesDirectory = CreateOutputDirectoryIfNotExists();

        // Timestamps
        var now = DateTime.UtcNow;
        doc.Meta.CreatedAt ??= now;
        doc.Meta.UpdatedAt = now;

        // Determine file path
        if (string.IsNullOrWhiteSpace(doc.FilePath))
        {
            var fileName = $"{doc.Meta.Title.ToSlug()}-{doc.Meta.PermanentId}.md";
            doc.FilePath = Path.Combine(pagesDirectory, fileName);
        }

        var yaml = markdownService.SerializeYaml(doc.Meta);
        var sb = new StringBuilder();
        sb.Append(yaml);
        sb.Append(doc.Markdown ?? "");

        var content = sb.ToString();
        var tempPath = doc.FilePath + ".tmp";

        await File.WriteAllTextAsync(tempPath, content, Encoding.UTF8);
        File.Move(tempPath, doc.FilePath, overwrite: true);
    }

    private async Task<PageDocument?> LoadFromFileAsync(string path)
    {
        var markdown = await File.ReadAllTextAsync(path, Encoding.UTF8);
        var (meta, html) = markdownService.Deserialize<PageFrontMatter>(markdown);

        return new PageDocument
        {
            Meta = meta,
            Markdown = markdown,
            Html = html,
            FilePath = path
        };
    }
}