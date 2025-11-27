# ADR-004: Angular as Frontend Framework

## Date:
2025-11-27

## Status:
Accepted

## Context:
The TeacherSuite application requires a user interface to allow users to interact with the existing API endpoints for Teachers, Courses, and Age Groups management. A modern, maintainable, and scalable frontend framework is needed that integrates well with the existing ASP.NET backend.

## Decision:
Angular will be used as the frontend framework for the following reasons:
- Angular is a popular and mature framework that pairs well with .NET backends, commonly used in enterprise applications
- It provides a comprehensive solution with built-in routing, HTTP client, forms handling, and state management
- TypeScript-first approach ensures type safety and better developer experience
- Strong community support and extensive documentation
- Component-based architecture promotes code reusability and maintainability
- Built-in CLI tools for scaffolding, testing, and building applications

## Consequences:
- Developers need to be familiar with TypeScript and Angular concepts
- Initial learning curve for those new to Angular
- The application will require Node.js and npm for development and build processes
- Additional build step needed to compile Angular application for production deployment
- The frontend can be served from the ASP.NET backend as static files or deployed separately
