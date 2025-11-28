using System.Text.Json;
using Orleans.Storage;
using Wiki.Grains.Extensions;

namespace Wiki.Grains;

public class LocalFileStorageProvider(
    LocalFileStorageOptions options,
    string storageName,
    ILogger<LocalFileStorageProvider> logger)
    : IGrainStorage
{
    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var stateDirectory = CreateStateDirectoryIfNotExists(stateName);
        var filePath = Path.Combine(stateDirectory, $"{grainId.Key}.json");
        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(Path.Combine(stateDirectory, $"{grainId.Key}.json"));
            grainState.State = JsonSerializer.Deserialize<T>(json) ?? Activator.CreateInstance<T>()!;
        
            // Mark that the record exists and set a basic ETag (e.g., timestamp)
            grainState.RecordExists = true;
            grainState.ETag = File.GetLastWriteTimeUtc(filePath).ToString("O");
        }
        else
        {
            // If no file exists, initialize with default state
            grainState.State = Activator.CreateInstance<T>()!;
        }
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var stateDirectory = CreateStateDirectoryIfNotExists(stateName);
        var filePath = Path.Combine(stateDirectory, $"{grainId.Key}.json");
        await File.WriteAllTextAsync(filePath,
            JsonSerializer.Serialize(grainState.State, new JsonSerializerOptions { WriteIndented = true }));
    }

    public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        CreateStateDirectoryIfNotExists(stateName);
        return Task.CompletedTask;
    }
    
    private string CreateStateDirectoryIfNotExists(string stateName)
    {
        var stateDirectory = Path.Combine(options.RootDirectory, stateName);
        if (!Directory.Exists(stateDirectory))
        {
            Directory.CreateDirectory(stateDirectory);
        }
        return stateDirectory;
    }
}