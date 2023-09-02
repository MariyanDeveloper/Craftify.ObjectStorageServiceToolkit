using Autodesk.Forge;
using Autodesk.Forge.Model;
using Craftify.ObjectStorageServiceToolkit.Constants;
using Craftify.ObjectStorageServiceToolkit.Extensions;
using Craftify.ObjectStorageServiceToolkit.Interfaces;
using Craftify.ObjectStorageServiceToolkit.Options;
using Craftify.ObjectStorageServiceToolkit.ResilientApiExecutions.Interfaces;
using Polly;

namespace Craftify.ObjectStorageServiceToolkit.Services;

public class ObjectService : IObjectService
{
    private readonly IObjectStorageServiceResilientExecution _resilientExecution;
    private readonly int _minChunkSize = 2;
    private readonly int _endRangeAdjustment = 1;
    private readonly string _extraDetailsSeparator = ",";

    private readonly ResilienceStrategy _resilienceStrategy;

    public ObjectService(
        IObjectStorageServiceResilientExecution resilientExecution)
    {
        _resilientExecution = resilientExecution ?? throw new ArgumentNullException(nameof(resilientExecution));
    }
    
    public async Task<ObjectDetails> UploadAsync(
        string bucketKey,
        string objectName,
        string fullLocalPath,
        Action<UploadObjectOptions>? configUploadOptions = null)
    {
        if (bucketKey is null) throw new ArgumentNullException(nameof(bucketKey));
        if (objectName is null) throw new ArgumentNullException(nameof(objectName));
        if (fullLocalPath is null) throw new ArgumentNullException(nameof(fullLocalPath));
        var options = new UploadObjectOptions();
        configUploadOptions?.Invoke(options);
        if (File.Exists(fullLocalPath) is false)
        {
            throw new FileNotFoundException($"{fullLocalPath} not found");
        }
        return await _resilientExecution.ExecuteWithObjectsApiTokenReceived(async (objectsApi) =>
        {
            var chunkMegabytesSize = options.ChunkMegabyteSize;
            chunkMegabytesSize = Math.Max(_minChunkSize, chunkMegabytesSize);
            var chunkSize = chunkMegabytesSize.ConvertToBytes();
            await using var fileReadStream = File.OpenRead(fullLocalPath);
            var sizeToUpload = fileReadStream.Length;
            chunkSize = CalculateAutoChunkSize(sizeToUpload, chunkSize);
            if (UploadInChunksRequired(sizeToUpload, chunkSize))
            {
                var result = await UploadInChunksAsync(
                    objectsApi,
                    bucketKey,
                    objectName,
                    fileReadStream,
                    chunkSize,
                    sizeToUpload);
                return result;
            }
            var uploadObjectResponse = await objectsApi.UploadObjectAsync(
                bucketKey,
                objectName,
                (int)sizeToUpload,
                fileReadStream) as DynamicJsonResponse;
            if (uploadObjectResponse is null)
            {
                ThrowObjectDetailsNullException();
            }
            return uploadObjectResponse.ToObject<ObjectDetails>();
        });
    }



    public async Task<ObjectDetails> Get(string bucketKey, string objectName, Action<GetObjectOptions>? configOptions = null)
    {
        if (bucketKey is null) throw new ArgumentNullException(nameof(bucketKey));
        if (objectName is null) throw new ArgumentNullException(nameof(objectName));
        var getOptions = new GetObjectOptions();
        configOptions?.Invoke(getOptions);
        return await _resilientExecution.ExecuteWithObjectsApiTokenReceived(async (objectsApi) =>
        {
            var objectDetailsAsDynamic = await objectsApi.GetObjectDetailsAsync(
                bucketKey,
                objectName,
                getOptions.ModifiedSinceDate,
                GetExtraObjectDetailsAsString(getOptions.ExtraObjectDetails)) as DynamicJsonResponse;
            return objectDetailsAsDynamic.ToObject<ObjectDetails>();
        });
    }

    public async Task<PostObjectSigned> CreateSignedAsync(string bucketKey, string objectName, Action<CreateSignedObjectOptions>? configOptions = null)
    {
        if (bucketKey is null) throw new ArgumentNullException(nameof(bucketKey));
        if (objectName is null) throw new ArgumentNullException(nameof(objectName));
        var options = new CreateSignedObjectOptions();
        configOptions?.Invoke(options);
        
        return await _resilientExecution.ExecuteWithObjectsApiTokenReceived(async (objectsApi) =>
        {
            var responseAsDynamic = await objectsApi.CreateSignedResourceAsync(
                bucketKey,
                objectName,
                options.PostBucketsSigned,
                options.Access.ToString().ToLower(),
                options.UseCdn) as DynamicJsonResponse;
            return responseAsDynamic.ToObject<PostObjectSigned>();
        });
    }

    public async Task<PostObjectSigned> UploadAsSignedAsync(string bucketKey, string objectName, string fullLocalPath, Action<UploadAsSignedOptions>? configOptions = null)
    {
        var uploadAsSignedOptions = new UploadAsSignedOptions();
        configOptions?.Invoke(uploadAsSignedOptions);
        await UploadAsync(bucketKey, objectName, fullLocalPath, uploadOptions =>
        {
            uploadOptions.ChunkMegabyteSize = uploadAsSignedOptions.ChunkMegabyteSize;
        });
        return await CreateSignedAsync(bucketKey, objectName, signedOptions =>
        {
            signedOptions.ApplyOptionsFrom(uploadAsSignedOptions);
        });
    }
    private string? GetExtraObjectDetailsAsString(ExtraObjectDetails? extraDetails = null)
    {
        if (extraDetails is null)
        {
            return default;
        }
        var mappings = new Dictionary<ExtraObjectDetails, string>()
        {
            [ExtraObjectDetails.CreatedDate] = ExtraObjectDetailsConstants.CreatedDate,
            [ExtraObjectDetails.LastAccessedDate] = ExtraObjectDetailsConstants.LastAccessedDate,
            [ExtraObjectDetails.LastModifiedDate] = ExtraObjectDetailsConstants.LastModifiedDate,
        };
        var details = mappings
            .Where(m => extraDetails.Value.HasFlag(m.Key))
            .Select(m => m.Value)
            .ToList();
        var withParameter = string.Join(_extraDetailsSeparator, details);
        return withParameter;
    }
    private bool UploadInChunksRequired(long sizeToUpload, long chunkSize) => sizeToUpload > chunkSize;

    private long CalculateAutoChunkSize(long sizeToUpload, long chunkSize)
    {
        if (sizeToUpload <= chunkSize)
        {
            return chunkSize;
        }
        var chunkNumber = Math.Ceiling((double)sizeToUpload / (double)chunkSize);
        return (long)Math.Ceiling(sizeToUpload / chunkNumber);
    }
    private async Task<ObjectDetails> UploadInChunksAsync(IObjectsApi objectsApi, string bucketKey, string objectName, FileStream fileReadStream, long chunkSize, long sizeToUpload)
    {
        var sessionId = Guid.NewGuid().ToString();
        long begin = 0;
        var buffer = new byte[chunkSize];
        ObjectDetails objectDetails = null;

        while (begin < sizeToUpload - _endRangeAdjustment)
        {
            var memoryStreamSize = sizeToUpload - begin < chunkSize ? (int)(sizeToUpload - begin) : (int)chunkSize;
            var bytesRead = await fileReadStream.ReadAsync(buffer, 0, memoryStreamSize);
            using var chunkStream = new MemoryStream(buffer, 0, memoryStreamSize);
            var contentRange = FormatContentRange(begin, bytesRead, sizeToUpload);
            var objectDetailsAsDynamic = await objectsApi.UploadChunkAsync(
                bucketKey,
                objectName,
                memoryStreamSize, 
                contentRange,
                sessionId,
                chunkStream) as DynamicJsonResponse;
            objectDetails = objectDetailsAsDynamic.ToObject<ObjectDetails>();
            begin += bytesRead;
        }
        if (objectDetails is null)
        {
            ThrowObjectDetailsNullException();
        }
        return objectDetails;
    }
    private string FormatContentRange(long begin, int bytesRead, long sizeToUpload)
    {
        return $"bytes {begin}-{begin + bytesRead - _endRangeAdjustment}/{sizeToUpload}";
    }
    private ObjectDetails ThrowObjectDetailsNullException()
    {
        throw new InvalidOperationException($"{nameof(ObjectDetails)} cannot be null");
    }
}