namespace Domain.Repositories;

public interface IPageRepository
{
    Task<List<string>> GetAll();
    Task Save(string markdown, string title, string permanentId);
}

public class PageDocument
{
    public ContentFrontMatter Meta { get; set; } = new();
    public string Markdown { get; set; } = "";
    public string Html { get; set; } = "";  // after rendering
    public string FilePath { get; set; } = "";
}