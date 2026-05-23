using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Storage;

public static class StorageServiceRegistration
{
    internal const string StorageHttpClientName = "EnglishTutor.Storage";

    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Storage");
        services.Configure<StorageOptions>(options =>
        {
            if (Enum.TryParse<StorageProvider>(section["Provider"], true, out var provider))
            {
                options.Provider = provider;
            }

            options.BucketName = section["BucketName"];
            options.Region = section["Region"];
            options.AccessKey = section["AccessKey"];
            options.SecretKey = section["SecretKey"];
            options.ServiceUrl = section["ServiceUrl"];
            options.BaseUrl = section["BaseUrl"] ?? options.BaseUrl;
            options.LocalPath = section["LocalPath"] ?? options.LocalPath;
        });
        services.AddHttpClient(StorageHttpClientName);
        services.AddSingleton<IFileStorageService>(CreateStorage);
        services.AddSingleton<IAudioStorageService>(provider => (IAudioStorageService)provider.GetRequiredService<IFileStorageService>());
        return services;
    }

    private static IFileStorageService CreateStorage(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<StorageOptions>>().Value;
        return options.Provider switch
        {
            StorageProvider.Local => ActivatorUtilities.CreateInstance<LocalFileStorageService>(provider),
            StorageProvider.S3 or StorageProvider.CloudflareR2 => ActivatorUtilities.CreateInstance<S3FileStorageService>(provider),
            _ => throw new NotSupportedException($"Storage provider {options.Provider} is not supported yet.")
        };
    }
}
