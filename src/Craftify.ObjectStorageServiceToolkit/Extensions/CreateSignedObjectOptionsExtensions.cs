using Craftify.ObjectStorageServiceToolkit.Options;

namespace Craftify.ObjectStorageServiceToolkit.Extensions;

public static class CreateSignedObjectOptionsExtensions
{
    public static void ApplyOptionsFrom(this CreateSignedObjectOptions toOptions, UploadAsSignedOptions fromOptions)
    {
        toOptions.Access = fromOptions.Access;
        toOptions.PostBucketsSigned = fromOptions.PostBucketsSigned;
        toOptions.UseCdn = fromOptions.UseCdn;
    }
}