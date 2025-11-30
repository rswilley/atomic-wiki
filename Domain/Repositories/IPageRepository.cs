namespace Domain.Repositories;

public interface IPageRepository
{
    Task<string> Get(string slug);
    Task<List<string>> GetAll();
    Task Save(string markdown, string title, string permanentId);
}