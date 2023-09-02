using Craftify.ObjectStorageServiceToolkit.Enums;

namespace Craftify.ObjectStorageServiceToolkit.Options;

public class FilterBucketOptions
{
    public ResidesIn ResidesIn { get; set; } = ResidesIn.UnitedStates;
    public int Limit { get; set; } = 10;
    public string? StartAt { get; set; } = default;
}