using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal sealed class GitHubTransport : IDisposable
{
  private readonly HttpClient client;
  internal GitHubTransport() : this(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { }
  internal GitHubTransport(HttpMessageHandler handler) => client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };

  internal Task<GitHubResponse> ExchangeAsync(Dictionary<string, string> fields, CancellationToken cancellationToken) =>
      SendAsync(new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
      { Content = new FormUrlEncodedContent(fields) }, cancellationToken);

  internal Task<GitHubResponse> GetAsync(string path, string token, CancellationToken cancellationToken)
  {
    if (!path.StartsWith('/') || path.StartsWith("//", StringComparison.Ordinal) || path.Contains('\\') || path.Contains('#'))
      throw new ArgumentException("Expected a fixed GitHub API path.", nameof(path));
    var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com" + path);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");
    return SendAsync(request, cancellationToken);
  }

  private async Task<GitHubResponse> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    using (request)
    {
      request.Headers.UserAgent.ParseAdd("SoFa-Development/1.0");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      try
      {
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        await response.Content.LoadIntoBufferAsync(2 * 1024 * 1024, cancellationToken).ConfigureAwait(false);
        var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        return new GitHubResponse(response.StatusCode, payload);
      }
      catch (HttpRequestException) { return new GitHubResponse(0, null); }
      catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return new GitHubResponse(0, null); }
      catch (JsonException) { return new GitHubResponse(0, null); }
    }
  }

  public void Dispose() => client.Dispose();
}

internal sealed class GitHubResponse(HttpStatusCode status, JsonDocument? payload) : IDisposable
{
  internal HttpStatusCode Status { get; } = status;
  internal JsonElement Value => payload?.RootElement ?? default;
  internal bool Successful => Status == HttpStatusCode.OK;
  public void Dispose() => payload?.Dispose();
}

internal static class SourceJson
{
  internal static string? Text(JsonElement value, string name) => value.ValueKind == JsonValueKind.Object
      && value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
  internal static long? Number(JsonElement value, string name) => value.ValueKind == JsonValueKind.Object
      && value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number
      && property.TryGetInt64(out var number) ? number : null;
}
