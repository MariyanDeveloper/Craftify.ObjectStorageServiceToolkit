namespace Craftify.ObjectStorageServiceToolkit;

[Flags]
public enum ExtraObjectDetails
{
    CreatedDate = 1,
    LastAccessedDate = 2,
    LastModifiedDate = 4
}