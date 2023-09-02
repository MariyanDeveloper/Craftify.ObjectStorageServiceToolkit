using Autodesk.Forge.Model;
using Craftify.ObjectStorageServiceToolkit.Enums;

namespace Craftify.ObjectStorageServiceToolkit.Options;

public class CreateBucketOptions
{
    public List<PostBucketsPayloadAllow>? PayloadAllows { get; set; } = default;
    public PostBucketsPayload.PolicyKeyEnum PolicyKey { get; set; } = PostBucketsPayload.PolicyKeyEnum.Transient;
    public ResidesIn ResidesIn { get; set; } = ResidesIn.UnitedStates;
}