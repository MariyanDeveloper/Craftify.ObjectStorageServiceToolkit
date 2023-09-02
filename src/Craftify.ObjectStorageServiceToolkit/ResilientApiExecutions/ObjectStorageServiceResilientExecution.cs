using Autodesk.Forge;
using Craftify.AutodeskAuthenticationToolkit.Interfaces;
using Craftify.ObjectStorageServiceToolkit.Extensions;
using Craftify.ObjectStorageServiceToolkit.Interfaces;
using Craftify.ObjectStorageServiceToolkit.ResilientApiExecutions.Interfaces;
using Polly;

namespace Craftify.ObjectStorageServiceToolkit.ResilientApiExecutions;

public class ObjectStorageServiceResilientExecution : IObjectStorageServiceResilientExecution
{
    private readonly IAutodeskAuthenticationService _autodeskAuthenticationService;
    private readonly IObjectsApi _objectsApi;
    private readonly IBucketsApi _bucketsApi;
    private readonly ResilienceStrategy _resilienceStrategy;
    private readonly Scope[] _scopes =
    {
        Scope.DataRead, Scope.DataWrite,
        Scope.BucketCreate, Scope.BucketDelete, Scope.BucketRead
    };
    
    private Task<string> _twoLeggedAccessToken => _lazyTwoLeggedAccessToken.Value;
    private Lazy<Task<string>> _lazyTwoLeggedAccessToken;
    
    public ObjectStorageServiceResilientExecution(
        IResilienceStrategyFactory resilienceStrategyFactory,
        IAutodeskAuthenticationService autodeskAuthenticationService,
        IObjectsApi objectsApi,
        IBucketsApi bucketsApi)
    {
        if (resilienceStrategyFactory is null) throw new ArgumentNullException(nameof(resilienceStrategyFactory));
        _autodeskAuthenticationService = autodeskAuthenticationService ?? throw new ArgumentNullException(nameof(autodeskAuthenticationService));
        _objectsApi = objectsApi ?? throw new ArgumentNullException(nameof(objectsApi));
        _bucketsApi = bucketsApi ?? throw new ArgumentNullException(nameof(bucketsApi));
        RefreshAccessToken();
        _resilienceStrategy = resilienceStrategyFactory.CreateAuthenticationStrategy(async (_) =>
        {
            await RefreshAccessToken();
        });
    }
    public async Task<T> ExecuteWithObjectsApiTokenReceived<T>(Func<IObjectsApi, Task<T>> callback)
    {
        return await ExecuteWithApiTokenReceived(
            token =>
            {
                _objectsApi.ApplyAccessToken(token);
                return callback(_objectsApi);
            });
    }
    public async Task<T> ExecuteWithBucketsApiTokenReceived<T>(Func<IBucketsApi, Task<T>> callback)
    {
        return await ExecuteWithApiTokenReceived(
            token =>
            {
                _bucketsApi.ApplyAccessToken(token);
                return callback(_bucketsApi);
            });
    }

    public async Task ExecuteWithObjectsApiTokenReceived(Func<IObjectsApi, Task> callback)
    {
        await ExecuteWithApiTokenReceived(
            token =>
            {
                _objectsApi.ApplyAccessToken(token);
                callback(_objectsApi);
            });
    }

    public async Task ExecuteWithBucketsApiTokenReceived(Func<IBucketsApi, Task> callback)
    {
        await ExecuteWithApiTokenReceived(
            token =>
            {
                _bucketsApi.ApplyAccessToken(token);
                callback(_bucketsApi);
            });
    }

    private async Task ExecuteWithApiTokenReceived(Action<string> onTokenReceived)
    {
        await _resilienceStrategy.ExecuteAsync(async (_) =>
        {
            var token = await _twoLeggedAccessToken;
            onTokenReceived(token);
        });
    }

    private async Task<T> ExecuteWithApiTokenReceived<T>(Func<string, Task<T>> onTokenReceived)
    {
        return await _resilienceStrategy.ExecuteAsync(async (_) =>
        {
            var token = await _twoLeggedAccessToken;
            return await onTokenReceived(token);
        });
    }
    private async Task RefreshAccessToken()
    {
        _lazyTwoLeggedAccessToken = new Lazy<Task<string>>(async () =>
        {
            var token = await _autodeskAuthenticationService.GetTwoLeggedToken(options => options.Scopes = _scopes);
            return token.AccessToken;
        });
    }
}