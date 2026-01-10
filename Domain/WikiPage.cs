using System.Text.RegularExpressions;
using Domain.Enums;

namespace Domain;

public class WikiPage(WikiContent content)
{
    public string Id { get; } = content.FrontMatter.PermanentId;
    public string Title { get; } = content.GetTitle();
    public WikiContent Content => content;
}

public record WikiContent
{
    public PageMeta FrontMatter { get; }
    public string Value { get; }
    public string MarkdownBody { get; }

    public WikiContent(string? markdown, IMarkdownParser markdownParser)
    {
        Value = markdown ?? string.Empty;
        if (string.IsNullOrWhiteSpace(Value))
        {
            FrontMatter = new PageMeta();
            _html = string.Empty;
            MarkdownBody = string.Empty;
        }
        else
        {
            var parsed = markdownParser.Deserialize<PageMeta>(Value);
            FrontMatter = parsed.meta;
            _html = parsed.html;
            MarkdownBody = parsed.markdownBody;
        }
    }

    public string GetTitle()
    {
        string pattern = @"<h1\b[^>]*>(.*?)<\/h1>";

        Match match = Regex.Match(_html, pattern);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return "Untitled";
    }

    public IReadOnlyCollection<string> GetHeaders()
    {
        string pattern = @"<h[2-6]\b[^>]*>(.*?)<\/h[2-6]>";

        MatchCollection matches = Regex.Matches(_html, pattern);
        return matches.Select(m => m.Groups[1].Value).ToList();
    }

    public string GetSearchBody()
    {
        var html = _html;

        // Strip h1 tag
        html = Regex.Replace(html, @"<h1\b[^>]*>.*?<\/h1>", string.Empty);

        // Strip h2-h6 tags
        html = Regex.Replace(html, @"<h[2-6]\b[^>]*>.*?<\/h[2-6]>", string.Empty);

        // Strip pre tags
        html = Regex.Replace(html, @"<pre\b[^>]*>.*?<\/pre>", string.Empty, RegexOptions.Singleline);

        // Remove all remaining HTML tags
        html = Regex.Replace(html, @"<.*?>", string.Empty);

        return html.Trim();
    }

    public IReadOnlyCollection<string> GetOutgoingLinks()
    {
        if (string.IsNullOrWhiteSpace(Value))
            return [];

        var regex = new Regex(@"\[\[([^|\]]+)(?:\|.*?)?\]\]");

        return regex.Matches(Value)
            .Select(m => m.Groups[1].Value.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToList();
    }

    public string ToPlainText()
    {
        if (string.IsNullOrWhiteSpace(MarkdownBody))
            return string.Empty;

        var text = Regex.Replace(MarkdownBody, @"#{1,6}\s", "");
        return text.Trim();
    }

    public static List<string> GetTags(string? tags)
    {
        return string.IsNullOrEmpty(tags)
            ? []
            : tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLowerInvariant())
                .Distinct()
                .ToList();
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

    public string ToRenderedContent()
    {
        var html = RemoveTitleFromHtml();
        return html;
    }

    public string ToSearchContent()
    {
        var html = RemoveTitleFromHtml();
        var htmlStripped = Regex.Replace(html, "<.*?>", string.Empty);
        return htmlStripped;
    }

    private readonly string _html;

    private string RemoveTitleFromHtml()
    {
        string pattern = @"<h1\b[^>]*>(.*?)<\/h1>";
        string title = "";

        Match match = Regex.Match(_html, pattern);
        if (match.Success)
        {
            title = match.Groups[1].Value;
        }

        if (string.IsNullOrEmpty(title))
            return _html;

        return _html.Replace(title, "");
    }
}

public class PageMeta
{
    public string PermanentId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = nameof(PageType.Note).ToLower();

    public string? Category { get; set; }

    public List<string>? Tags { get; set; }
    public bool? Pinned { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Dictionary<string, object>? Extra { get; set; }
}
