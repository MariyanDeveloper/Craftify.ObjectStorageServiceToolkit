namespace Craftify.ObjectStorageServiceToolkit.Options;

public class GetObjectOptions
{
    public DateTime? ModifiedSinceDate { get; set; } = null;
    public ExtraObjectDetails? ExtraObjectDetails { get; set; } = null;
}