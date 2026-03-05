# ADR-005: Hybrid Cache with Redis

## Date:
2026-03-05

## Status:
Accepted

## Context:
The application needs a caching layer to reduce database load and improve response times for frequently accessed, read-heavy data such as reference lists (age groups, courses, programming languages). A single-level cache (memory-only or distributed-only) has trade-offs — memory caches are fast but not shared across instances, while distributed caches add network latency. A hybrid approach combines the strengths of both.

## Decision:
We adopt .NET's built-in `HybridCache` (available since .NET 9) as a two-level caching solution:

- **L1 (In-Process):** `IMemoryCache` — fast, per-instance, with a shorter TTL (2 minutes by default).
- **L2 (Distributed):** A dedicated, containerized **Redis** instance — shared across application instances, with a longer TTL (10 minutes by default).

Redis was chosen as the L2 backing store because it is widely adopted, battle-tested, and has first-class support in the .NET ecosystem via `Microsoft.Extensions.Caching.StackExchangeRedis`. A dedicated Redis instance is used exclusively for caching, separate from any other Redis usage.

Cache logic is implemented transparently via MediatR pipeline behaviors:
- **`CachingBehaviour`** — automatically caches responses for any query implementing the `ICacheableQuery` marker interface.
- **`CacheInvalidationBehaviour`** — automatically evicts cache entries (by tag) for any command implementing the `ICacheInvalidationCommand` marker interface.

Cache abstractions (`ICacheService`, `ICacheableQuery`, `ICacheInvalidationCommand`) are defined in the Application layer. The implementation (`CacheService`) lives in the Infrastructure layer. Structured, namespaced cache keys (e.g., `teachersuite:agegroups:all`) are used. `CancellationToken` is always passed. Large or sensitive objects are not cached. Tags are used strategically — only on entries that benefit from grouped invalidation.

## Consequences:
- Queries that implement `ICacheableQuery` are cached automatically — zero repetition, fully centralized.
- Commands that implement `ICacheInvalidationCommand` evict related cache entries by tag.
- Adding caching to a new query requires only implementing the marker interface — no changes to handler logic.
- Redis must be running (containerized via Docker Compose) for L2 caching to function; the application degrades gracefully to L1-only if Redis is unavailable.
- Developers must ensure cache keys are unique and tags are applied consistently to maintain cache coherence.
