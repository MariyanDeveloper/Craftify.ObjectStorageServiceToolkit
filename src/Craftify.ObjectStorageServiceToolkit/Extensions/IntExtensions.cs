namespace Craftify.ObjectStorageServiceToolkit.Extensions;

public static class IntExtensions
{
    public static long ConvertToBytes(this int megabytes) =>
       megabytes * 1024 * 1024;
    
}