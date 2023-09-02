using Polly;
using Polly.Retry;

namespace Craftify.ObjectStorageServiceToolkit.Interfaces;

public interface IResilienceStrategyFactory
{
    ResilienceStrategy CreateAuthenticationStrategy(Action<OutcomeArguments<object, OnRetryArguments>> onUnauthorizedCallback);
}