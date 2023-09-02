using Craftify.ObjectStorageServiceToolkit.Extensions;
using Craftify.ObjectStorageServiceToolkit.Interfaces;
using Polly;
using Polly.Retry;

namespace Craftify.ObjectStorageServiceToolkit.Factories;

public class ResilienceStrategyFactory : IResilienceStrategyFactory
{
    public ResilienceStrategy CreateAuthenticationStrategy(Action<OutcomeArguments<object, OnRetryArguments>> onUnauthorizedCallback)
    {
        return new CompositeStrategyBuilder()
            .AddAuthenticationStrategy(onUnauthorizedCallback)
            .Build();
    }
}