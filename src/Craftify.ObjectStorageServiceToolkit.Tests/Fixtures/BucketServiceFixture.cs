using Craftify.ObjectStorageServiceToolkit.Tests.Helpers;

namespace Craftify.ObjectStorageServiceToolkit.Tests.Fixtures;

public class BucketServiceFixture : IAsyncLifetime
{
    public string ExistingBucketKey { get; private set; }
    public string NotExistingBucketKey { get; private set; }
    public string UniqueBucketNameForCreation { get; private set; }
    public IBucketService BucketService { get; private set; }
    
    public async Task InitializeAsync()
    {
        BucketService = DependencyInjectionHelper.GetRequiredService<IBucketService>();
        var existingBucketKey = Guid.NewGuid().ToString(); 
        var bucket = await BucketService.GetOrCreate(existingBucketKey);
        ExistingBucketKey = bucket.BucketKey;
        NotExistingBucketKey = Guid.NewGuid().ToString();
        UniqueBucketNameForCreation = Guid.NewGuid().ToString();
    }

    public async Task DisposeAsync()
    {
        await BucketService.Delete(ExistingBucketKey);
        await BucketService.Delete(UniqueBucketNameForCreation);
        await ThrowIfBucketsWereNotProperlyDeleted();
    }

    private async Task ThrowIfBucketsWereNotProperlyDeleted()
    {
        await ThrowIfBucketWasNotDeleted(UniqueBucketNameForCreation);
        await ThrowIfBucketWasNotDeleted(ExistingBucketKey);
    }

    private async Task ThrowIfBucketWasNotDeleted(string bucketKey)
    {
        var bucketResult = await BucketService.TryGetByKey(bucketKey);
        if (bucketResult.IsFound)
        {
            throw new InvalidOperationException(
                $"Failed to delete the bucket with key {bucketKey}. It still exists after deletion.");
        }
    }
}