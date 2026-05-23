extern alias WorkerHost;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkerHost::EnglishTutor.Worker;

namespace EnglishTutor.IntegrationTests.Infrastructure;

public static class IntegrationWorkerHost
{
    public static ServiceProvider CreateServiceProvider(string connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(DatabaseEnglishTutorApiFactory.CreateConfiguration(connectionString))
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug());
        services.AddWorkerServices(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }
}
