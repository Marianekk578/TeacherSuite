# ADR 001: ChibiSafe for Lesson Material File Storage

## Status

Accepted

## Context

TeacherSuite requires file storage for lesson materials — primarily Word documents (.docx), text files (.txt), and Markdown files (.md). These materials need to be uploaded by administrators and supervisors and downloaded by teachers. Files must be organized per lesson, with the ability to store multiple files per lesson.

Options considered:
1. **Local filesystem** — Simple to implement but limits horizontal scaling, requires backup strategy, and ties storage to the application server.
2. **Cloud object storage (S3/Azure Blob)** — Scalable and durable but adds cloud vendor dependency and cost for a self-hosted application.
3. **ChibiSafe** — Self-hosted, open-source file server with a REST API, album-based file organization, and Docker support.

## Decision

We chose **ChibiSafe** (v5) as the file storage backend for lesson materials.

Key reasons:
- **Self-hosted**: Aligns with TeacherSuite's self-hosted deployment model. No external cloud services required.
- **Album-based organization**: Files are grouped into albums, which maps naturally to lessons — each lesson gets one album. The album UUID is stored on the Lesson entity.
- **REST API**: ChibiSafe exposes a documented REST API for upload (`POST /api/upload`), download (`GET /api/file/{uuid}`), album creation (`POST /api/album/create`), and file-to-album linking (`POST /api/file/{uuid}/album/{albumUuid}`).
- **Containerized**: ChibiSafe ships as Docker images, fitting into the existing `docker-compose.yml` infrastructure alongside the TeacherSuite backend and database.

## Implementation

- **`IFileStorageService`** abstraction in the Application layer defines the contract: `UploadAsync`, `DownloadAsync`, `DeleteAsync`, `CreateAlbumAsync`, `AddFileToAlbumAsync`, `GetAlbumFilesAsync`.
- **`ChibiSafeFileStorageService`** in the Infrastructure layer implements the interface using `HttpClient` against the ChibiSafe REST API. Registered via `AddHttpClient<IFileStorageService, ChibiSafeFileStorageService>()`.
- Configuration via environment variables: `CHIBISAFE__BASEURL` and `CHIBISAFE__APIKEY`.
- On first file upload for a lesson, an album is created in ChibiSafe and the album UUID is persisted on the `Lesson.AlbumId` column. Subsequent uploads are linked to the same album.
- Markdown files (.md) fetched from ChibiSafe are parsed and rendered as HTML in the frontend. Word and text files are offered for download.

## Consequences

- **Positive**: No cloud vendor lock-in. Full control over storage. Album-based grouping keeps files organized. Easy to back up (ChibiSafe data volume).
- **Negative**: Adds another container to the deployment. ChibiSafe is a third-party dependency that must be maintained and updated. API changes in ChibiSafe could require adapter updates.
- **Mitigated by**: The `IFileStorageService` abstraction allows swapping the storage backend without changing application logic.
