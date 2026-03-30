namespace TeacherSuite.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
    Task<string> CreateAlbumAsync(string albumName, CancellationToken cancellationToken = default);
    Task AddFileToAlbumAsync(string fileUuid, string albumUuid, CancellationToken cancellationToken = default);
    Task<List<AlbumFile>> GetAlbumFilesAsync(string albumUuid, CancellationToken cancellationToken = default);
}

public record AlbumFile(string Uuid, string Name, long Size);
