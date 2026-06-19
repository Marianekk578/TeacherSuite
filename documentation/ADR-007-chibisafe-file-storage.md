# ADR-007: ChibiSafe File Storage

## Date:
2026-03-31

## Status:
Accepted

## Context:
TeacherSuite needs a way to store and manage lesson materials such as Word documents, text files, and markdown files. The storage solution must be self-hosted to align with the existing deployment model, support organized grouping of files per lesson, and expose an API for basic file operations like upload, download, delete, and album management.

## Decision:
We chose ChibiSafe v5 as the file storage backend. It is a self-hosted solution that fits naturally into our Docker-based stack as an additional container. Its album-based organization maps well to lessons, allowing files to be grouped logically. ChibiSafe provides an API for the file operations we need, and an abstraction layer in the application decouples business logic from the specific storage implementation.

## Consequences:
On the positive side, self-hosting gives us full control over stored data with no vendor lock-in, and album-based grouping simplifies per-lesson file management. On the negative side, it introduces an extra container to maintain and a third-party dependency to keep updated. The abstraction layer allows swapping the storage backend without changing application logic if a different solution is needed in the future.
