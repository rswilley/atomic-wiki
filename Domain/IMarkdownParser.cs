namespace Domain;

public interface IMarkdownParser
{
    string Serialize(object obj, string markdown);
    (T meta, string html, string markdownBody) Deserialize<T>(string markdown) where T : new();
}