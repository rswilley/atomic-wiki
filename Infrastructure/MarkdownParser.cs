using System.Text;
using Domain;
using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Infrastructure;

public class MarkdownParser : IMarkdownParser
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

    public string Serialize(object obj, string markdown)
    {
        var yaml = _yamlSerializer.Serialize(obj).TrimEnd();
        
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine(yaml);
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine(markdown);
        
        return sb.ToString();
    }

    public (T meta, string html, string markdownBody) Deserialize<T>(string markdown) where T : new()
    {
        // front matter format:
        // ---\n
        // yaml
        // ---\n
        // markdown body
        if (!markdown.StartsWith("---"))
        {
            return (new T(), ToHtml(markdown), markdown); // or treat as page with default meta
        }

        var parts = markdown.Split(["\n---", "\r\n---"], 3, StringSplitOptions.None);
        if (parts.Length < 2)
        {
            return (new T(), "", "");
        }

        // Remove leading '---'
        var yamlText = parts[0].TrimStart('-', ' ', '\r', '\n');
        var markdownBody = parts[1].TrimStart('\r', '\n');

        var meta = _yamlDeserializer.Deserialize<T>(yamlText);
        var html = ToHtml(markdownBody);
        
        return (meta, html, markdownBody);
    }
    
    private string ToHtml(string markdown)
    {
        return Markdown.ToHtml(markdown, _markdownPipeline);
    }
}