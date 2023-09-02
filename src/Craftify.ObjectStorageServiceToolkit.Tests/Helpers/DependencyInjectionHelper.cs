using Microsoft.Extensions.Configuration;

namespace Craftify.ObjectStorageServiceToolkit.Tests.Helpers;

public static class DependencyInjectionHelper
{
    private static readonly IServiceProvider Provider = GetProvider();
    public static T GetRequiredService<T>()
    {
        return Provider.GetRequiredService<T>();
    }
    private static IServiceProvider GetProvider()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        var clientId = configuration["APS_CLIENT_ID"];
        var clientSecret = configuration["APS_CLIENT_SECRET"];
        services.AddObjectStorageServiceToolkit();
        services.AddAutodeskAuthenticationToolkit(new AuthCredentials(clientId, clientSecret));
        return services.BuildServiceProvider();
    }

}