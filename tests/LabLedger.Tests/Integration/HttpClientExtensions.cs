using System.Net.Http.Json;

namespace LabLedger.Tests.Integration;

internal static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(
        this HttpClient client,
        string? requestUri,
        T value,
        CancellationToken cancellationToken = default) =>
        client.PatchAsync(requestUri, JsonContent.Create(value), cancellationToken);
}
