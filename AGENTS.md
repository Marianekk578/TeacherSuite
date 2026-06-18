# AGENTS.md

## Purpose

This file defines the working rules for coding agents contributing to this repository.

The application consists of:

* an Angular 21 frontend;
* a .NET 10 / ASP.NET Core backend;
* shared API contracts, generated clients, infrastructure, and tests where present.

These instructions apply to the entire repository unless a more specific `AGENTS.md` exists in a subdirectory. A nested file overrides this file only for its directory tree.

## Core working principles

1. Inspect before editing.
2. Preserve the existing architecture and conventions unless the task explicitly requires a change.
3. Make the smallest coherent change that fully solves the task.
4. Do not modify unrelated code, reformat unrelated files, or overwrite user changes.
5. Keep the application buildable and testable after every logical change.
6. Prefer clear, maintainable code over clever abstractions.
7. Do not introduce a dependency, framework, architectural layer, or code generator without a concrete need.
8. Treat warnings, nullable diagnostics, lint failures, and type errors as problems to resolve, not suppress.
9. Never commit secrets, credentials, tokens, private certificates, production connection strings, or personal data.
10. State assumptions and verification gaps in the final report.

## Repository discovery

Before making changes, inspect the repository root and identify:

* `angular.json`, frontend `package.json`, TypeScript configuration files, and the package-manager lockfile;
* `.sln`, `.slnx`, `.csproj`, `global.json`, `Directory.Build.props`, and `Directory.Packages.props`;
* existing `README.md`, `.editorconfig`, lint rules, formatting rules, and CI workflows;
* Docker, container orchestration, deployment, and local-development files;
* OpenAPI documents and generated API clients;
* database projects, Entity Framework Core contexts, and migration locations;
* unit, integration, component, and end-to-end test projects;
* any nested `AGENTS.md` files.

Use the package manager selected by the checked-in lockfile:

* `package-lock.json` → npm;
* `pnpm-lock.yaml` → pnpm;
* `yarn.lock` → Yarn.

Do not replace the package manager or regenerate another lockfile unless explicitly requested.

Use SDK and runtime versions pinned by repository files. Do not silently update Angular, Node.js, .NET SDK, NuGet packages, npm packages, or lockfiles.

## Change workflow

For each task:

1. Read the relevant code, tests, configuration, and nearby documentation.
2. Determine the smallest affected frontend and backend boundaries.
3. Search for existing implementations before creating a new pattern.
4. Add or update tests with the production change.
5. Run focused checks first.
6. Run the broader build and test suite when practical.
7. Review the diff for accidental formatting, generated output, secrets, and unrelated changes.
8. Report what changed, what was tested, and anything not verified.

Do not claim a command passed unless it was actually run successfully.

## Commands

Prefer repository scripts and CI-equivalent commands over ad hoc commands. The examples below are fallbacks; adapt them to the discovered project layout.

### Frontend

Run from the Angular workspace directory:

```bash
npm ci
npm run start
npm run build
npm test -- --watch=false
npm run lint
```

When scripts do not exist, use the local Angular CLI through the package manager, for example:

```bash
npx ng build
npx ng test --watch=false
```

Do not use a globally installed Angular CLI as the source of truth.

### Backend

Run from the solution directory:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

When configured by the repository, also run:

```bash
dotnet format --verify-no-changes
```

Target a specific solution, solution filter, or project when multiple candidates exist. Do not run database migrations against shared or production-like databases unless explicitly authorized.

### Full-stack verification

Use existing root-level scripts, task runners, Docker Compose profiles, or CI commands when available. Do not invent a parallel developer workflow if the repository already defines one.

## Angular 21 guidelines

### Architecture and organization

* Use standalone components, directives, and pipes for new code unless the surrounding feature deliberately uses NgModules.
* Organize code by business feature or domain, not by generic technical folders such as `components`, `services`, or `utils`.
* Keep closely related component TypeScript, template, styles, and tests together.
* Keep one primary concept per file.
* Use hyphenated file names that match the primary TypeScript identifier.
* Avoid generic file names such as `helpers.ts`, `common.ts`, and `utils.ts`; use a name that communicates the responsibility.
* Lazy-load feature routes where it improves startup cost and matches the existing routing strategy.
* Do not create a new state-management layer when local component state, signals, or an existing feature service is sufficient.

### Components and templates

* Keep components focused on presentation and user interaction.
* Move reusable business rules, transformations, and orchestration out of templates and large components.
* Prefer Angular's built-in control-flow syntax: `@if`, `@for`, `@switch`, and `@defer`.
* Always provide an intentional `track` expression for `@for`.
* Keep template expressions simple. Move complex derived state to a `computed` signal or a clearly named method only when appropriate.
* Prefer `class` and `style` bindings over `ngClass` and `ngStyle`.
* Use semantic HTML first. Add ARIA only where native semantics are insufficient.
* Ensure interactive behavior works with a keyboard and has a visible focus state.
* Preserve or improve accessible names, labels, error associations, and focus management.
* Do not directly manipulate the DOM when Angular bindings, directives, or CDK utilities are suitable.
* Do not bypass Angular sanitization or use unsafe HTML without an explicit, reviewed security reason.

### Signals and reactivity

* Prefer signals for local synchronous UI state and derived state.
* Use `computed` for derived values.
* Use `effect` only for genuine side effects, not for propagating state that can be derived.
* Do not mutate signal-held arrays or objects in place; create updated values.
* Use RxJS for asynchronous streams, cancellation, event composition, and existing observable APIs.
* Do not convert observables to signals and back repeatedly without a clear boundary.
* Prevent subscription leaks by using framework-supported lifecycle utilities such as `takeUntilDestroyed` when manual subscription is necessary.
* Avoid nested subscriptions. Compose streams with RxJS operators.

### Dependency injection and class members

* Prefer `inject()` over constructor parameter injection in new Angular code, while staying consistent with the surrounding file.
* Mark dependencies and Angular-managed properties `readonly` when they are not reassigned.
* Use `protected` for members intended only for the template.
* Keep public members intentional because they form part of the class API.
* Do not use service locators or retrieve dependencies from a global injector.

### Inputs, outputs, and forms

* Follow the input/output style already used by the feature.
* Prefer signal-based `input`, `output`, and `model` APIs for new code when compatible with the codebase.
* Do not alias inputs or outputs without a compatibility or naming reason.
* Use reactive forms for non-trivial forms unless the feature consistently uses another supported pattern.
* Keep form validation rules explicit, testable, and shared with the backend only through duplicated domain rules or generated contracts—not by trusting client validation.
* Display actionable validation messages and preserve submitted values on recoverable errors.
* Treat experimental Angular APIs as opt-in. Do not introduce them without an explicit project decision.

### HTTP and API integration

* Keep HTTP calls in dedicated API/data-access boundaries rather than presentation components.
* Use typed request and response models.
* Preserve cancellation behavior for route changes, repeated searches, and destroyed components.
* Handle loading, empty, success, validation-error, authorization-error, and unexpected-error states.
* Do not swallow errors with empty `catchError` branches.
* Centralize cross-cutting concerns such as authentication headers, correlation IDs, and consistent error mapping in interceptors when the repository already uses that pattern.
* Never hand-edit generated API client files. Update the source OpenAPI contract and regenerate using the repository command.
* Avoid duplicating backend DTOs manually when a maintained generated-client workflow exists.

### Angular testing

Angular 21 CLI projects use Vitest by default, but preserve the repository's configured runner.

* Co-locate unit tests with the code under test using `.spec.ts`.
* Test observable behavior, rendered output, accessibility-relevant behavior, and user interactions.
* Prefer public behavior over private implementation details.
* Add regression tests for bug fixes.
* Avoid brittle tests tied to incidental DOM structure, exact internal method calls, or timing.
* Use Angular testing utilities and the project's established helpers.
* Mock only external boundaries; do not mock the unit's core behavior.
* For asynchronous tests, use deterministic framework utilities instead of arbitrary timeouts.
* Update end-to-end tests for critical user journeys when behavior crosses routing, authentication, or backend integration boundaries.

## .NET 10 and ASP.NET Core guidelines

### Language and style

* Use the C# language version configured by the project. .NET 10 commonly uses C# 14, but repository configuration is authoritative.
* Follow `.editorconfig`, analyzers, nullable settings, and `Directory.Build.props`.
* Keep nullable reference types enabled where configured. Fix nullability issues instead of using the null-forgiving operator without proof.
* Use PascalCase for types and public members, camelCase for locals and parameters, and the repository convention for private fields.
* Prefer file-scoped namespaces when consistent with the project.
* Use `var` when the type is obvious from the right-hand side; otherwise favor clarity.
* Prefer immutable data and `record` types for value-like DTOs when suitable.
* Avoid regions, deeply nested control flow, long methods, and catch-all utility classes.
* Do not suppress analyzer warnings globally to avoid fixing a local issue.

### Application architecture

* Respect existing project and dependency boundaries.
* Keep domain logic out of controllers, endpoints, middleware, EF Core configurations, and mapping code.
* Use dependency injection and explicit interfaces where they represent a real boundary or improve testability.
* Do not add repository, unit-of-work, mediator, result, or specification abstractions merely for pattern compliance.
* Prefer feature-oriented organization when the existing solution supports it.
* Keep transport DTOs separate from persistence entities and sensitive internal models.
* Do not return EF Core entities directly from public APIs.
* Map only fields intended for the client.

### Controllers and Minimal APIs

ASP.NET Core supports both controllers and Minimal APIs. Follow the style already selected by the application.

For controllers:

* Derive API controllers from `ControllerBase`.
* Use `[ApiController]` and explicit route conventions used by the project.
* Prefer typed results such as `ActionResult<T>` where appropriate.

For Minimal APIs:

* Group related endpoints.
* Keep endpoint handlers thin.
* Move domain and application logic into focused services or handlers.
* Use typed results when they improve contract clarity and OpenAPI output.

For all endpoints:

* Validate input at the boundary.
* Return consistent status codes and error bodies.
* Use `ProblemDetails` or the repository's established error contract.
* Do not expose exception details, stack traces, database information, or internal identifiers.
* Enforce authorization on the server for every protected operation.
* Do not rely on hidden UI controls as authorization.
* Preserve backward compatibility unless the task explicitly permits a breaking API change.
* Update OpenAPI and client generation when the contract changes.

### Asynchronous code and cancellation

* Use asynchronous APIs for I/O.
* Use the `Async` suffix for asynchronous methods except framework-defined endpoint names where the existing convention differs.
* Accept and propagate `CancellationToken` through HTTP, database, messaging, and long-running application operations.
* Never use `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` in request-processing code.
* Avoid unnecessary `Task.Run` for server-side I/O.
* Use `ValueTask` only when measurement or an existing API justifies it.

### Configuration, secrets, and logging

* Use strongly typed options for related configuration.
* Validate critical options at startup when practical.
* Use `appsettings.json` only for non-secret defaults.
* Use development secrets, environment variables, workload identity, or the repository's secret store for sensitive values.
* Never log passwords, tokens, authorization headers, connection strings, private keys, or unnecessary personal data.
* Use structured logging with named properties rather than interpolated diagnostic strings.
* Include useful context, but avoid duplicate logging at multiple layers.
* Do not catch exceptions only to log and rethrow unless adding meaningful context not logged elsewhere.

### Persistence and EF Core

When Entity Framework Core is used:

* Use asynchronous query and save methods.
* Pass cancellation tokens.
* Project read models at the database when possible instead of loading full entity graphs.
* Use `AsNoTracking` for read-only queries unless tracking is required.
* Avoid N+1 queries and unbounded result sets.
* Apply pagination to list endpoints that can grow.
* Keep transactions as short as possible.
* Use optimistic concurrency where concurrent updates matter.
* Do not call `SaveChanges` repeatedly inside loops without a justified need.
* Add migrations through the established migration project and command.
* Review generated migrations for destructive or unintended operations.
* Never edit an already-applied shared migration to change history; add a corrective migration.
* Do not automatically apply production migrations from an agent session.

### API contracts

* Use explicit DTOs for requests and responses.
* Treat enum serialization, date/time formats, decimal precision, nullability, and collection semantics as contract decisions.
* Use UTC for persisted instants and API timestamps unless the domain explicitly requires another representation.
* Do not use local server time for business decisions.
* Version breaking public API changes according to the project's strategy.
* Keep OpenAPI descriptions, response types, validation constraints, and authorization metadata accurate.
* Regenerate Angular clients through the checked-in tool or script after contract changes.
* Review generated diffs, but do not manually customize generated files.

### Backend testing

* Keep unit tests fast and isolated from network, file system, clock, and database unless those are the explicit subject.
* Keep integration tests in separate projects or categories from unit tests.
* Use the repository's chosen framework, such as xUnit, NUnit, or MSTest.
* Name tests to express the scenario and expected result.
* Use Arrange–Act–Assert or the established local convention.
* Test behavior and contracts, not private implementation details.
* Add regression tests for defects.
* Use `WebApplicationFactory` or the established host fixture for ASP.NET Core integration tests.
* Prefer realistic infrastructure for integration tests when the repository already uses containers or dedicated test services.
* Do not make integration tests depend on developer-specific machine state or shared mutable environments.
* Keep test data deterministic.
* Avoid control flow and complex logic inside tests.
* Verify authorization, validation, status codes, error contracts, serialization, and persistence behavior for changed endpoints.

## Frontend/backend contract changes

When a task changes an API contract:

1. Update the backend contract and validation.
2. Update OpenAPI generation or the checked-in specification.
3. Regenerate the frontend client using the repository command.
4. Update Angular adapters, state, and UI handling.
5. Add or update backend contract/integration tests.
6. Add or update frontend tests.
7. Verify both applications build.
8. Check whether the change is backward compatible.

Do not make independent frontend and backend interpretations of the same field. Document intentional differences in naming, timezone handling, optionality, or enum representation.

## Security requirements

For every change, consider:

* authentication and authorization;
* tenant or ownership boundaries;
* input validation and output encoding;
* SQL injection and unsafe dynamic queries;
* cross-site scripting;
* cross-site request forgery where cookie authentication is used;
* server-side request forgery for user-controlled URLs;
* unsafe redirects;
* file upload type, size, name, and storage handling;
* rate limits and abuse scenarios;
* sensitive information in logs and errors;
* dependency and supply-chain risk.

Use framework security features. Do not implement custom cryptography, token validation, password storage, or authorization protocols.

## Performance requirements

* Measure before making broad performance changes.
* Avoid repeated HTTP requests, duplicate subscriptions, unnecessary change propagation, and large eager bundles in Angular.
* Avoid materializing large database results, N+1 queries, synchronous I/O, and excessive allocations in .NET.
* Keep pagination, filtering, and sorting server-side for large datasets.
* Do not add caching without defining ownership, keying, invalidation, expiration, and authorization behavior.
* Preserve correctness and observability when optimizing.

## Generated files and dependencies

Generated files commonly include:

* OpenAPI clients;
* API specifications produced from code;
* EF Core migrations and snapshots;
* compiled frontend assets;
* coverage reports;
* generated version files.

Do not manually edit generated output unless the repository explicitly treats it as source. Change the generator input and run the established generation command.

Before adding a dependency:

1. Confirm the platform or current dependency set does not already solve the need.
2. Check compatibility with Angular 21 or .NET 10.
3. Prefer actively maintained, narrowly scoped packages.
4. Consider licensing, bundle/runtime cost, security, and transitive dependencies.
5. Update the correct manifest and lock or central package file.
6. Add the minimal required configuration.
7. Explain the reason in the final report.

## Documentation

Update documentation when changing:

* setup or developer commands;
* configuration or environment variables;
* public API behavior;
* architecture or project boundaries;
* database migration procedures;
* authentication or authorization behavior;
* deployment or operational requirements.

Keep comments focused on why a decision exists, not on restating the code. Remove obsolete comments while editing nearby code.

## Git and diff hygiene

* Do not discard, reset, or overwrite uncommitted user changes.
* Do not amend commits or rewrite history unless explicitly requested.
* Do not commit build output, local settings, IDE metadata, logs, secrets, or temporary files.
* Keep diffs focused.
* Avoid whole-file formatting caused by line-ending or formatter changes.
* Review `git diff` and `git status` before reporting completion.
* Do not create a commit unless explicitly requested.

## Required verification by change type

### Angular-only change

At minimum, run the relevant unit tests, type/build check, and lint command when configured.

### .NET-only change

At minimum, run the relevant tests and build the affected project or solution. Run formatting/analyzer verification when configured.

### API contract change

Build and test both backend and frontend, regenerate clients, and verify contract-focused tests.

### Database change

Build the backend, run relevant tests, inspect the migration, and verify upgrade behavior in an isolated development/test database when available.

### Cross-cutting or deployment change

Use the repository's full CI-equivalent command set where practical.

If a required check cannot run, report the exact command and reason.

## Definition of done

A change is complete when:

* the requested behavior is implemented;
* the implementation follows existing architecture and these instructions;
* relevant tests are added or updated;
* affected builds, tests, linting, formatting, and generation checks pass;
* API and database changes are synchronized across consumers;
* security, accessibility, error handling, cancellation, and observability were considered;
* documentation is updated where needed;
* the diff contains no unrelated changes or secrets;
* the final report accurately lists changes and verification.

## Final response format

Use a concise completion report:

```text
Summary
- What changed and why.

Tests
- Commands run and their outcomes.

Notes
- Assumptions, migrations, generated files, compatibility concerns, or checks not run.
```

::: 
