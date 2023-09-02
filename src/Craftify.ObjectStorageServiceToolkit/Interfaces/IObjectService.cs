using Autodesk.Forge.Model;
using Craftify.ObjectStorageServiceToolkit.Options;

namespace Craftify.ObjectStorageServiceToolkit.Interfaces;

public interface IObjectService
{
    Task<ObjectDetails> UploadAsync(
        string bucketKey,
        string objectName,
        string fullLocalPath,
        Action<UploadObjectOptions>? configUploadOptions = null);

    Task<ObjectDetails> Get(
        string bucketKey,
        string objectName,
        Action<GetObjectOptions>? configOptions = null);

    Task<PostObjectSigned> CreateSignedAsync(
        string bucketKey,
        string objectName,
        Action<CreateSignedObjectOptions>? configOptions = null);

    Task<PostObjectSigned> UploadAsSignedAsync(
        string bucketKey,
        string objectName,
        string fullLocalPath,
        Action<UploadAsSignedOptions>? configOptions = null);
}