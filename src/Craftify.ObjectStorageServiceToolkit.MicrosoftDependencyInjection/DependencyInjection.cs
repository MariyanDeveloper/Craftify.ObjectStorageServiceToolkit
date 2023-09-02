using Autodesk.Forge;
using Craftify.ObjectStorageServiceToolkit.Factories;
using Craftify.ObjectStorageServiceToolkit.Interfaces;
using Craftify.ObjectStorageServiceToolkit.ResilientApiExecutions;
using Craftify.ObjectStorageServiceToolkit.ResilientApiExecutions.Interfaces;
using Craftify.ObjectStorageServiceToolkit.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Craftify.ObjectStorageServiceToolkit.MicrosoftDependencyInjection;

public static class DependencyInjection
{
    public static void AddObjectStorageServiceToolkit(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IBucketsApi, BucketsApi>();
        serviceCollection.AddTransient<IObjectsApi, ObjectsApi>();
        serviceCollection.AddTransient<IResilienceStrategyFactory, ResilienceStrategyFactory>();
        serviceCollection.AddSingleton<IObjectStorageServiceResilientExecution, ObjectStorageServiceResilientExecution>();
        serviceCollection.AddTransient<IObjectService, ObjectService>();
        serviceCollection.AddTransient<IBucketService, BucketService>();
    }
}