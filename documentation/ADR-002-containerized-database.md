# ADR-002: Containerized Database

## Date:
2025-11-11

## Status:
Accepted

## Context:
Databases need to run in an isolated environment to ensure consistency across development and operations. Free containerization tool is required.

## Decision:
Containerized database will be used for all development and production environments. Docker Desktop on Personal plan, running WSL 2, was used to create and manage the database containers. Docker Desktop is widely used in the industry.

## Consequences:
Containerized databases will require additional setup, which might not be easy at the start. The developerw will get more hands-on experience with containers.