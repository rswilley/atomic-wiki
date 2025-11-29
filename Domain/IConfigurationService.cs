namespace Domain;

public interface IConfigurationService
{
    string GetDataDirectory();
    string GetSearchIndexDirectory();
}