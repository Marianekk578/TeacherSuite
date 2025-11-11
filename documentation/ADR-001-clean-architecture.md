# ADR-001: Clean Architecture

## Date:
2025-11-11

## Status:
Accepted

## Context:
The choice of an ASP.NET project architecture is crucial for ensuring maintainability and future scalability. It also ensures other developers reading the code can easily recognize the patters and the project structure.

## Decision:
Clean Architecture is a well-known design. Author of the project wants to get hands-on experience with it, even though he heard that Vertical Slice is much better in a bigger scale. Clean Architecture is known for decupling business logic from infrastructure, like databases, caches etc. Making it easier to test and swtich to other technologies.

## Consequences:
Developer needs to Clean Architecture principles, which may introduce some initial complexity. There is a risk of over-engineering the project and making circular references.