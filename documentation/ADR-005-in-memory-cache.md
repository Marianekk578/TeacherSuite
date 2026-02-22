# ADR-005: In-memory cache

## Date:
2026-02-2

## Status:
Accepted

## Context:
Application requires a caching layer to increase functionality performance

## Decision:
In memory cache implementation will be done. Caching will be used for non-critical, easily recomputable data.

## Consequences:
Single-instance deployment - in-memory cache is local to one process, at the time of writing ADR app is launched only in one process.

No cache persistence - restart wipes everything, for development purpose is acceptable.

Memory pressure - large caches compete with the app's own heap, which can affect GC behavior and stability.