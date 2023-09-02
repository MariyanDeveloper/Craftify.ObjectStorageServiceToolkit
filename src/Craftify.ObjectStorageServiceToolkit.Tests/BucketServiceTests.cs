using Craftify.ObjectStorageServiceToolkit.Tests.Fixtures;

namespace Craftify.ObjectStorageServiceToolkit.Tests;

public class BucketServiceTests : IClassFixture<BucketServiceFixture>
{
    private string _existingBucketKey;
    private string _notExistingKey;
    private IBucketService _bucketService;
    private string _uniqueBucketForCreation;

    public BucketServiceTests(BucketServiceFixture bucketServiceFixture)
    {
        _existingBucketKey = bucketServiceFixture.ExistingBucketKey;
        _notExistingKey = bucketServiceFixture.NotExistingBucketKey;
        _bucketService = bucketServiceFixture.BucketService;
        _uniqueBucketForCreation = bucketServiceFixture.UniqueBucketNameForCreation;
    }
    [Fact]
    public async Task Get_WhenSearchForExisting_ReturnsIt()
    {
        var bucket = await _bucketService.GetByKey(_existingBucketKey);
        Assert.False(bucket is null);
    }
    
    [Fact]
    public async Task Get_WhenSearchForNotExisting_ThrowsApiException()
    {
        await Assert.ThrowsAsync<ApiException>(async () => await _bucketService.GetByKey(_notExistingKey));
    }
    
    [Fact]
    public async Task TryGet_WhenSearchForNotExisting_ReturnsNotFoundResult()
    {
        var bucketResult = await _bucketService.TryGetByKey(_notExistingKey);
        Assert.True(bucketResult.IsFound is false && bucketResult.Bucket is null);
    }
    
    [Fact]
    public async Task TryGet_WhenSearchForExisting_ReturnsFoundResult()
    {
        var bucketResult = await _bucketService.TryGetByKey(_existingBucketKey);
        Assert.True(bucketResult.IsFound);
    }
    
    [Fact]
    public async Task Create_WhenBucketNameIsUnique_ReturnsNewlyCreated()
    {
        var bucket = await _bucketService.Create(_uniqueBucketForCreation);
        Assert.Equal(_uniqueBucketForCreation, bucket.BucketKey);
    }
    
    [Fact]
    public async Task Create_WhenBucketNameAlreadyExists_ThrowsException()
    {
        await Assert.ThrowsAsync<ApiException>(async () => await _bucketService.Create(_existingBucketKey));
    }
    [Fact]
    public async Task GetFiltered_ShouldNotReturnMoreThanSpecifiedLimit()
    {
        var residesIn = ResidesIn.UnitedStates;
        var limit = 3;

        var result = await _bucketService.GetFiltered(options =>
        {
            options.ResidesIn = residesIn;
            options.Limit = limit;
        });

        Assert.True(result.Items.Count <= limit, "Returned more items than the specified limit");
    }
    
    [Fact]
    public async Task GetAll_ShouldRetrieveAllBucketsForSpecifiedRegion()
    {
        var residesIn = ResidesIn.UnitedStates;

        var result = await _bucketService.GetAll(residesIn);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);

        Assert.Contains(result.Items, item => item.BucketKey == _existingBucketKey);
    }
    
}