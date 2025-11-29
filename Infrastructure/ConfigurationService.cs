using Domain;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class ConfigurationService(IConfiguration configuration) : IConfigurationService
{
    public string GetDataDirectory()
    {
        var directory = configuration["DATA_DIRECTORY"];
        ArgumentException.ThrowIfNullOrEmpty(directory);
        return directory;
    }

    public string GetSearchIndexDirectory()
    {
        return Path.Combine(GetDataDirectory(), "search_index");
    }
}