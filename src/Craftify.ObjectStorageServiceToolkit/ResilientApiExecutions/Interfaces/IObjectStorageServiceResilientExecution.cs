using Autodesk.Forge;

namespace Craftify.ObjectStorageServiceToolkit.ResilientApiExecutions.Interfaces;

public interface IObjectStorageServiceResilientExecution
{
    Task<T> ExecuteWithObjectsApiTokenReceived<T>(Func<IObjectsApi, Task<T>> callback);
    Task<T> ExecuteWithBucketsApiTokenReceived<T>(Func<IBucketsApi, Task<T>> callback);
    Task ExecuteWithObjectsApiTokenReceived(Func<IObjectsApi, Task> callback);
    Task ExecuteWithBucketsApiTokenReceived(Func<IBucketsApi, Task> callback);
}