using System.Web;

namespace Craftify.ObjectStorageServiceToolkit.Extensions;

public static class StringExtensions
{
    public static string ParseStartAtFromNextUri(this string next)
    {
        var nextUri = new Uri(next);
        var queryDictionary = HttpUtility.ParseQueryString(nextUri.Query);
        return queryDictionary["startAt"];
    }
}