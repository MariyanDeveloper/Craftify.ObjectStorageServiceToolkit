using Autodesk.Forge.Model;
using Craftify.ObjectStorageServiceToolkit.Constants;
using Craftify.ObjectStorageServiceToolkit.Enums;

namespace Craftify.ObjectStorageServiceToolkit.Options;

public class UploadAsSignedOptions
{
    public int ChunkMegabyteSize { get; set; } = ObjectConstants.DefaultChunkMegabyteSize;
    public PostBucketsSigned PostBucketsSigned { get; set; } = new PostBucketsSigned(ObjectConstants.DefaultMinutesExpiration);
    public Access Access { get; set; } = Access.Read;
    public bool UseCdn { get; set; } = true;
}