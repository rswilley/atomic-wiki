namespace Wiki.Services;

public interface IConfigurationService
{
    string GetDataDirectory();
}

public class ConfigurationService(IConfiguration configuration) : IConfigurationService
{
    public string GetDataDirectory()
    {
        var directory = configuration.GetValue<string>("DATA_DIRECTORY");
        ArgumentException.ThrowIfNullOrEmpty(directory);
        return directory;
    }
}