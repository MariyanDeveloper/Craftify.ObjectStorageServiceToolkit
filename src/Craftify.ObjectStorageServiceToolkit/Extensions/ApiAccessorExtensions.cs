using Autodesk.Forge.Client;

namespace Craftify.ObjectStorageServiceToolkit.Extensions;

public static class ApiAccessorExtensions
{
    public static void ApplyAccessToken(this IApiAccessor apiAccessor, string accessToken) =>
        apiAccessor.Configuration.AccessToken = accessToken;
}