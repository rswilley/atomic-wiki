using System.Text;
using Wiki.Extensions;
using Wiki.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
    private readonly IDeserializer _yaml = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();
    private readonly ISerializer _yamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull) // cleaner YAML
        .Build();

    public async Task<List<PageDocument>> GetAll()
    {
        var docs = new List<PageDocument>();
        var pagesDirectory = CreatePagesDirectoryIfNotExists();
        
        foreach (var file in Directory.GetFiles(pagesDirectory, "*.md", SearchOption.TopDirectoryOnly))
        {
            var doc = await LoadFromFileAsync(file);
            if (doc != null)
            {
                docs.Add(doc);
            }
        }

        return docs;
    }

    private string CreatePagesDirectoryIfNotExists()
    {
        var pagesDirectory = Path.Combine(configurationService.GetDataDirectory(), "pages");
        if (!Directory.Exists(pagesDirectory))
        {
            Directory.CreateDirectory(pagesDirectory);
        }

        return pagesDirectory;
    }

    public async Task Save(PageDocument doc)
    {
        var pagesDirectory = CreatePagesDirectoryIfNotExists();

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