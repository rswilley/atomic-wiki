using System.Text.RegularExpressions;

namespace Wiki.Services;

public interface IRenderService
{
    string GetExcerpt(string html);
}

public class RenderService : IRenderService
{
    public string GetExcerpt(string html)
    {
        html = RemoveTitle(html);
        var htmlStripped = Regex.Replace(html, "<.*?>", string.Empty);
        if (htmlStripped.Length >= 45)
        {
            return string.Concat(htmlStripped.AsSpan(0, 45), "...");
        }
        else
        {
            return htmlStripped;
        }
    }
    
    private static string RemoveTitle(string body)
    {
        string pattern = @"<h1\b[^>]*>(.*?)<\/h1>";
        string title = "";

        Match match = Regex.Match(body, pattern);
        if (match.Success)
        {
            title = match.Groups[1].Value;
        }

        if (string.IsNullOrEmpty(title))
            return body;

        return body.Replace(title, "");
    }
}