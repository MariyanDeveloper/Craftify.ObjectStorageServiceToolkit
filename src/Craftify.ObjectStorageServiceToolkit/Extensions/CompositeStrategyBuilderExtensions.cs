using Autodesk.Forge.Client;
using Polly;
using Polly.Retry;

namespace Craftify.ObjectStorageServiceToolkit.Extensions;

public static class CompositeStrategyBuilderExtensions
{
    private static readonly int _tooManyRequestsStatus = 429;
    private static readonly int _unauthorizedStatus = 401;
    public static TBuilder AddAuthenticationStrategy<TBuilder>(this TBuilder builder, Action<OutcomeArguments<object, OnRetryArguments>> onUnauthorizedCallback)
        where TBuilder : CompositeStrategyBuilderBase
    {
        var resilienceStrategy = new CompositeStrategyBuilder()
            .AddRetry(new RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<ApiException>(IsUnauthorized),
                RetryCount = 5,
                BaseDelay = TimeSpan.FromSeconds(1),
                OnRetry = outcome =>
                {
                    onUnauthorizedCallback(outcome);
                    return default;
                },
                BackoffType = RetryBackoffType.Exponential,
                UseJitter = true
            })
            .AddRetry(new RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<ApiException>(IsTooManyRequests),
                RetryCount = 3,
                BaseDelay = TimeSpan.FromSeconds(10),
                BackoffType = RetryBackoffType.Linear
            })
            .Build();
        return builder.AddStrategy(resilienceStrategy);
    }
    private static bool IsTooManyRequests(ApiException exception) =>
       exception.ErrorCode == _tooManyRequestsStatus;
    private static bool IsUnauthorized(ApiException exception) =>
       exception.ErrorCode == _unauthorizedStatus;
    
}