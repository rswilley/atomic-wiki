using Microsoft.Extensions.Options;
using Orleans.Runtime.Hosting;

namespace Wiki.Grains.Extensions;

public static class LocalFileStorageSiloBuilderExtensions
{
    public static ISiloBuilder AddLocalFileGrainStorage(this ISiloBuilder builder, string name, Action<LocalFileStorageOptions> configureOptions)
    {
        return builder.ConfigureServices(services =>
        {
            // 1. Configure the options with the specific name provided
            services.AddOptions<LocalFileStorageOptions>(name)
                .Configure(configureOptions);

            // 2. Register the storage provider using the factory delegate
            // This allows us to resolve the specific named options if needed later
            services.AddGrainStorage(name, (sp, storageName) =>
            {
                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<LocalFileStorageOptions>>();
                var options = optionsMonitor.Get(storageName);

                // Create the provider, injecting dependencies
                // Note: You may need to add a constructor to LocalFileStorageProvider to accept 'storageName' or 'options'
                return ActivatorUtilities.CreateInstance<LocalFileStorageProvider>(sp, options, storageName);
            });
        });
    }
}

public class LocalFileStorageOptions
{
    public required string RootDirectory { get; set; }
}