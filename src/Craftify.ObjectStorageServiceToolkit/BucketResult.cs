using Autodesk.Forge.Model;

namespace Craftify.ObjectStorageServiceToolkit;

public class BucketResult
{
    public bool IsFound { get; } = false;
    public Bucket? Bucket { get; }

    public BucketResult(bool isFound, Bucket? bucket = null)
    {
        IsFound = isFound;
        Bucket = bucket;
    }
}