using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Wiki.Services;

public interface IMarkdownService
{
    string ToHtml(string markdown);
    string? GetFirstHeader(string markdown);
    string SerializeYaml(object obj);
    (T meta, string html) Deserialize<T>(string markdown) where T : new();
}

public class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();
    
    private readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();
    private readonly ISerializer _yamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull) // cleaner YAML
        .Build();
    
    public string ToHtml(string markdown)
    {
        return Markdown.ToHtml(markdown, _markdownPipeline);
    }
    
    public string? GetFirstHeader(string markdown)
    {
        string pattern = @"^#\s+(.+)$";

        Match match = Regex.Match(markdown, pattern);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }

    public string SerializeYaml(object obj)
    {
        var yaml = _yamlSerializer.Serialize(obj).TrimEnd();
        
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine(yaml);
        sb.AppendLine("---");
        sb.AppendLine();
        
        return sb.ToString();
    }

    public (T meta, string html) Deserialize<T>(string markdown) where T : new()
    {
        // front matter format:
        // ---\n
        // yaml
        // ---\n
        // markdown body
        if (!markdown.StartsWith("---"))
        {
            return (new T(), ""); // or treat as page with default meta
        }

        var parts = markdown.Split(["\n---", "\r\n---"], 3, StringSplitOptions.None);
        if (parts.Length < 3)
        {
            return (new T(), "");
        }

        // Remove leading '---'
        var yamlText = parts[0].TrimStart('-', ' ', '\r', '\n') + parts[1];
        var markdownText = parts[2];

        var meta = _yamlDeserializer.Deserialize<T>(yamlText);
        var html = ToHtml(markdownText);
        
        return (meta, html);
    }
}