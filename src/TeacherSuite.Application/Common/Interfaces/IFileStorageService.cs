namespace TeacherSuite.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
    Task<string> CreateAlbumAsync(string albumName, CancellationToken cancellationToken = default);
    Task AddFileToAlbumAsync(string albumId, string fileUuid, CancellationToken cancellationToken = default);
}
