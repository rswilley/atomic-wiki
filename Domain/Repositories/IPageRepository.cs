namespace Domain.Repositories;

public interface IPageRepository
{
    Task<string> Get(string filename);
    Task<List<string>> GetAll();
    Task Save(string markdown, string fileName);
    Task Delete(string fileName);
}
