using Autodesk.Forge.Model;
using Craftify.ObjectStorageServiceToolkit.Enums;
using Craftify.ObjectStorageServiceToolkit.Options;

namespace Craftify.ObjectStorageServiceToolkit.Interfaces;

public interface IBucketService
{
    Task<Bucket> GetByKey(string bucketKey);
    Task<BucketResult> TryGetByKey(string bucketKey);
    Task<Bucket> Create(string bucketKey, Action<CreateBucketOptions>? configOptions = null);
    Task<Bucket> GetOrCreate(string bucketKey, Action<CreateBucketOptions>? configOptions = null);
    Task<Buckets> GetFiltered(Action<FilterBucketOptions>? configFilterOptions = null);
    Task<Buckets> GetAll(ResidesIn residesIn);
    Task Delete(string bucketKey);
}