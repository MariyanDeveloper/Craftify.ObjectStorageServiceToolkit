using Autodesk.Forge.Client;
using Autodesk.Forge.Model;
using Craftify.ObjectStorageServiceToolkit.Constants;
using Craftify.ObjectStorageServiceToolkit.Enums;
using Craftify.ObjectStorageServiceToolkit.Extensions;
using Craftify.ObjectStorageServiceToolkit.Interfaces;
using Craftify.ObjectStorageServiceToolkit.Options;
using Craftify.ObjectStorageServiceToolkit.ResilientApiExecutions.Interfaces;

namespace Craftify.ObjectStorageServiceToolkit.Services;

public class BucketService : IBucketService
{
    private readonly IObjectStorageServiceResilientExecution _resilientExecution;

    public BucketService(IObjectStorageServiceResilientExecution objectStorageServiceResilientExecution)
    {
        _resilientExecution = objectStorageServiceResilientExecution ?? throw new ArgumentNullException(nameof(objectStorageServiceResilientExecution));
    }

    public async Task<Buckets> GetAll(ResidesIn residesIn)
    {
        var maximumLimit = 100;
        string? startAt = null;
        Buckets? buckets = default;
        do
        {
            var retrievedBuckets = await GetFiltered(options =>
            {
                options.ResidesIn = residesIn;
                options.Limit = maximumLimit;
                options.StartAt = startAt;
            });
            if (buckets is null)
            {
                buckets = retrievedBuckets;
                startAt = retrievedBuckets.Next?.ParseStartAtFromNextUri();
                continue;
            }
            buckets.Items.AddRange(retrievedBuckets.Items);
            startAt = retrievedBuckets.Next?.ParseStartAtFromNextUri();
        } while (string.IsNullOrEmpty(startAt) is false);
        return buckets;
    }
    public async Task<Buckets> GetFiltered(Action<FilterBucketOptions>? configFilterOptions = null)
    {
        var options = new FilterBucketOptions();
        configFilterOptions?.Invoke(options);
        return await _resilientExecution.ExecuteWithBucketsApiTokenReceived(async (bucketsApi) =>
        {
            var bucketAsJsonResponse = await bucketsApi.GetBucketsAsync(
                PickRegion(options.ResidesIn),
                options.Limit,
                options.StartAt) as DynamicJsonResponse;
            return bucketAsJsonResponse.ToObject<Buckets>();
        });
    }
    public async Task<Bucket> Create(string bucketKey, Action<CreateBucketOptions>? configOptions = null)
    {
        var options = CreateBucketOptions(configOptions);
        var postBucketPayload = new PostBucketsPayload(
            bucketKey,
            options.PayloadAllows,
            options.PolicyKey);
        return await _resilientExecution.ExecuteWithBucketsApiTokenReceived(async (bucketsApi) =>
        {
            var bucketAsDynamic = await bucketsApi.CreateBucketAsync(
                postBucketPayload,
                PickRegion(options.ResidesIn));
            return ConvertDynamicToBucket(bucketAsDynamic);
        });

    }
    
    public async Task<Bucket> GetByKey(string bucketKey)
    {
        return await _resilientExecution.ExecuteWithBucketsApiTokenReceived(async (bucketsApi) =>
            ConvertDynamicToBucket(await bucketsApi.GetBucketDetailsAsync(bucketKey)));
    }

    public async Task<BucketResult> TryGetByKey(string bucketKey)
    {
        try
        {
            var bucket = await GetByKey(bucketKey);
            return new BucketResult(isFound: true, bucket: bucket);
        }
        catch
        {
            return new BucketResult(isFound: false);
        }
    }

    public async Task<Bucket> GetOrCreate(string bucketKey, Action<CreateBucketOptions>? configOptions = null)
    {
        return await _resilientExecution.ExecuteWithBucketsApiTokenReceived(async (bucketsApi) =>
        {
            try
            {
                var existingBucket = await GetByKey(bucketKey);
                return existingBucket;
            }
            catch (ApiException ex) when (ex.ErrorCode == 404)
            {
                return await Create(
                    bucketKey,
                    configOptions);
            }
        });

    }

    public async Task Delete(string bucketKey)
    {
        await _resilientExecution.ExecuteWithBucketsApiTokenReceived(async (bucketsApi) =>
        {
            await bucketsApi.DeleteBucketAsync(bucketKey);
        });
    }

    private Bucket ConvertDynamicToBucket(dynamic bucketResult) =>
        (bucketResult as DynamicJsonResponse).ToObject<Bucket>();
    private string PickRegion(ResidesIn residesIn) =>
        residesIn switch
        {
            ResidesIn.UnitedStates => Regions.UnitedStates,
            ResidesIn.EMEA => Regions.EMEA,
            _ => throw new NotImplementedException()
        };
    private CreateBucketOptions CreateBucketOptions(Action<CreateBucketOptions>? configOptions)
    {
        var options = new CreateBucketOptions();
        configOptions?.Invoke(options);
        return options;
    }

}