using System.Text;
using Markdig;
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

public class FilePageStore(IConfigurationService configurationService) : IPageStore
{
    private readonly IDeserializer _yaml = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();
    private readonly ISerializer _yamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull) // cleaner YAML
        .Build();
    private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
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

        // Serialize front matter to YAML
        var yaml = _yamlSerializer.Serialize(doc.Meta).TrimEnd();

        // Build final file text
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine(yaml);
        sb.AppendLine("---");
        sb.AppendLine(); // blank line before markdown body
        sb.Append(doc.Markdown ?? "");

        var content = sb.ToString();

        // Safe-ish write: write temp file then move
        var tempPath = doc.FilePath + ".tmp";

        await File.WriteAllTextAsync(tempPath, content, Encoding.UTF8);
        File.Move(tempPath, doc.FilePath, overwrite: true);
    }

    private async Task<PageDocument?> LoadFromFileAsync(string path)
    {
        var text = await File.ReadAllTextAsync(path, Encoding.UTF8);

        // front matter format:
        // ---\n
        // yaml
        // ---\n
        // markdown body
        if (!text.StartsWith("---"))
        {
            return null; // or treat as page with default meta
        }

        var parts = text.Split(["\n---", "\r\n---"], 3, StringSplitOptions.None);
        if (parts.Length < 3)
        {
            return null;
        }

        // Remove leading '---'
        var yamlText = parts[0].TrimStart('-', ' ', '\r', '\n') + parts[1];
        var markdown = parts[2];

        var meta = _yaml.Deserialize<PageFrontMatter>(yamlText);
        var html = Markdown.ToHtml(markdown, _markdownPipeline);

        return new PageDocument
        {
            Meta = meta,
            Markdown = markdown,
            Html = html,
            FilePath = path
        };
    }
}