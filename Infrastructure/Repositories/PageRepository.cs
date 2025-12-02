using System.Text;
using Domain;
using Domain.Repositories;

namespace Infrastructure.Repositories;

public class PageRepository(IConfigurationService configurationService) : IPageRepository
{
    public async Task<string> Get(string fileName)
    {
        var outputDirectory = CreateOutputDirectoryIfNotExists();

        var filePath = Path.Combine(outputDirectory, fileName);
        if (File.Exists(filePath))
        {
            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }

        return string.Empty;
    }

    public async Task<List<string>> GetAll()
    {
        var contentList = new List<string>();
        var outputDirectory = CreateOutputDirectoryIfNotExists();

        foreach (var file in Directory.GetFiles(outputDirectory, "*.md", SearchOption.TopDirectoryOnly))
        {
            var content = await File.ReadAllTextAsync(file, Encoding.UTF8);
            contentList.Add(content);
        }

        return contentList;
    }

    private string CreateOutputDirectoryIfNotExists()
    {
        var directory = Path.Combine(configurationService.GetDataDirectory(), "output");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return directory;
    }

    public async Task Save(string markdown, string fileName)
    {
        var pagesDirectory = CreateOutputDirectoryIfNotExists();
        var filePath = Path.Combine(pagesDirectory, fileName);
        var tempPath = filePath + ".tmp";

        await File.WriteAllTextAsync(tempPath, markdown, Encoding.UTF8);
        File.Move(tempPath, filePath, overwrite: true);
    }

    public Task Delete(string fileName)
    {
        var pagesDirectory = CreateOutputDirectoryIfNotExists();
        var filePath = Path.Combine(pagesDirectory, fileName);
        File.Delete(filePath);
        return Task.CompletedTask;
    }
}
