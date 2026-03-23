using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Infrastructure.FileStorage;

public class ChibiSafeFileStorageService : IFileStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ChibiSafeFileStorageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ChibiSafe:BaseUrl"]?.TrimEnd('/')
                   ?? throw new InvalidOperationException("ChibiSafe:BaseUrl is not configured.");
        var apiKey = configuration["ChibiSafe:ApiKey"]
                     ?? throw new InvalidOperationException("ChibiSafe:ApiKey is not configured.");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        using var formContent = new MultipartFormDataContent();
        using var streamContent = new StreamContent(content);
        formContent.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/upload", formContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var uuid = doc.RootElement.GetProperty("uuid").GetString()
                   ?? throw new InvalidOperationException("ChibiSafe upload did not return a UUID.");
        return uuid;
    }

    public async Task<Stream> DownloadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/file/{storageKey}", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/file/{storageKey}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
