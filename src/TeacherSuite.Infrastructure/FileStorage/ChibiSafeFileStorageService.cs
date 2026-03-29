using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Infrastructure.FileStorage;

public class ChibiSafeFileStorageService : IFileStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public ChibiSafeFileStorageService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = Environment.GetEnvironmentVariable("CHIBISAFE__BASEURL")
                   ?? throw new InvalidOperationException("ChibiSafe:BaseUrl is not configured.");
        _apiKey = Environment.GetEnvironmentVariable("CHIBISAFE__APIKEY")
                     ?? throw new InvalidOperationException("ChibiSafe:ApiKey is not configured.");
    }

    public async Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        using var formContent = new MultipartFormDataContent();
        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        formContent.Add(streamContent, "file", fileName);

        var url = $"{_baseUrl.TrimEnd('/')}/api/upload";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-api-key", _apiKey);
        request.Content = formContent;

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var uuid = doc.RootElement.GetProperty("uuid").GetString()
                   ?? throw new InvalidOperationException("ChibiSafe upload did not return a UUID.");
        return uuid;
    }

    public async Task<Stream> DownloadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/api/file/{storageKey}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-api-key", _apiKey);

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/api/file/{storageKey}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Add("x-api-key", _apiKey);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> CreateAlbumAsync(string albumName, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/api/album/create";
        var body = JsonSerializer.Serialize(new { name = albumName });

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-api-key", _apiKey);
        request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var uuid = doc.RootElement.GetProperty("uuid").GetString()
                   ?? throw new InvalidOperationException("ChibiSafe album creation did not return a UUID.");
        return uuid;
    }

    public async Task AddFileToAlbumAsync(string albumId, string fileUuid, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/api/album/{albumId}/link";
        var body = JsonSerializer.Serialize(new { uuid = fileUuid });

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-api-key", _apiKey);
        request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
