using System.Text.RegularExpressions;
using Domain.Enums;
using Domain.Extensions;

namespace Domain;

public class WikiPage(WikiContent content, IIdService idService)
{
    public string Id { get; } = content.FrontMatter.PermanentId ?? idService.Generate(DateTime.UtcNow.Ticks);
    public string Slug { get; } = content.FrontMatter.Title?.ToSlug() ?? string.Empty;
    public WikiContent Content { get; } = content;
}

public record WikiContent
{
    public ContentFrontMatter FrontMatter { get; }
    public string Value { get; }
    public string Html { get; }
    public string MarkdownBody { get; }

    public WikiContent(string? markdown, IMarkdownParser markdownParser)
    {
        Value = markdown ?? string.Empty;
        if (string.IsNullOrWhiteSpace(Value))
        {
            FrontMatter = new ContentFrontMatter();
            Html = string.Empty;
            MarkdownBody = string.Empty;
        }
        else
        {
            var parsed = markdownParser.Deserialize<ContentFrontMatter>(Value);
            FrontMatter = parsed.meta;
            Html = parsed.html;
            MarkdownBody = parsed.markdownBody;
        }
    }
    
    public string GetTitle()
    {
        string pattern = @"<h1\b[^>]*>(.*?)<\/h1>";

        Match match = Regex.Match(Html, pattern);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return "Untitled";
    }

    // Logic moves here. The data 'owns' the behavior.
    public IReadOnlyCollection<string> GetOutgoingLinks()
    {
        if (string.IsNullOrWhiteSpace(Value))
            return Array.Empty<string>();

        // Your regex logic lives here now
        var regex = new Regex(@"\[\[([^|\]]+)(?:\|.*?)?\]\]");
        
        return regex.Matches(Value)
            .Select(m => m.Groups[1].Value.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToList();
    }

    public string ToPlainText()
    {
        if (string.IsNullOrWhiteSpace(Value)) return string.Empty;
        
        // Your cleaning logic lives here
        var text = Regex.Replace(Value, @"#{1,6}\s", ""); 
        // ... rest of regex cleaning ...
        return text.Trim();
    }
    
    public List<string> GetTags(string? tags)
    {
        return string.IsNullOrEmpty(tags) 
            ? [] 
            : tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public string GetExcerpt()
    {
        var html = RemoveTitleFromHtml();
        var htmlStripped = Regex.Replace(html, "<.*?>", string.Empty);
        
        if (htmlStripped.Length >= 45)
        {
            return string.Concat(htmlStripped.AsSpan(0, 45), "...");
        }

        return htmlStripped;
    }
    
    private string RemoveTitleFromHtml()
    {
        string pattern = @"<h1\b[^>]*>(.*?)<\/h1>";
        string title = "";

        Match match = Regex.Match(Html, pattern);
        if (match.Success)
        {
            title = match.Groups[1].Value;
        }

        if (string.IsNullOrEmpty(title))
            return Html;

        return Html.Replace(title, "");
    }
}

public class ContentFrontMatter
{
    public string? PermanentId { get; set; }
    public string Title { get; set; } = "Untitled";
    public string Type { get; set; } = nameof(PageType.Note).ToLower();

    public string? Category { get; set; }

    public List<string>? Tags { get; set; }
    public bool? Pinned { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Dictionary<string, object>? Extra { get; set; }
}