using Craftify.ObjectStorageServiceToolkit.Constants;

namespace Craftify.ObjectStorageServiceToolkit.Options;

public class UploadObjectOptions
{
    public int ChunkMegabyteSize { get; set; } = ObjectConstants.DefaultChunkMegabyteSize;
}