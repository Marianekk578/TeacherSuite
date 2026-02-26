# ADR-005: Keycloak as Identity Provider

## Date:
2026-02-26

## Status:
Accepted

## Context:
TeacherSuite currently has no authentication or authorization mechanism. The application needs a secure, standards-based identity provider to manage user identities, roles, and access control for both the Angular frontend and the .NET API backend. The solution must support:
- Self-service user registration (no custom sign-up UI)
- Role-based access control (Global Admin, Teacher, Supervisor)
- OAuth 2.0 / OpenID Connect (OIDC) for single sign-on
- PKCE authorization code flow for the Angular SPA (public client)
- JWT validation on the .NET API backend
- Containerized local development setup

## Decision:
Keycloak will be used as the identity provider for the following reasons:
- **Open-source and self-hosted**: No vendor lock-in; can be deployed alongside the existing Docker Compose stack for development and production.
- **Standards-compliant**: Full support for OAuth 2.0, OpenID Connect, and SAML 2.0 protocols.
- **Built-in registration and login pages**: Eliminates the need for custom authentication UI — Keycloak's self-registration page can be enabled in Realm settings.
- **Role and group management**: Native support for realm roles (e.g., `admin`, `teacher`, `supervisor`) that map directly to .NET authorization policies.
- **Angular integration**: Official `keycloak-js` adapter supports PKCE flow with a public client (no client secret required).
- **.NET integration**: Standard JWT Bearer authentication middleware validates Keycloak-issued tokens without additional libraries.
- **Mature ecosystem**: Extensive documentation, active community, and production-proven at scale.

### Suggested Realm Roles:
- **admin** — Global administrators with full system access
- **teacher** — Teachers who manage their own courses, groups, and students
- **supervisor** — Supervisors who oversee teachers and review course assignments

### Client Configuration:
- **Public Client** (`teachersuite-spa`): For the Angular SPA using PKCE authorization code flow, no client secret.
- **Confidential Client** (`teachersuite-api`): For the .NET backend, used for token introspection and backend-to-backend calls.

## Consequences:
- Adds Keycloak as an infrastructure dependency (Docker container in development).
- Developers must run Keycloak alongside PostgreSQL during local development.
- Initial realm and client configuration must be created (can be exported/imported as JSON).
- All API endpoints will require a valid JWT token, changing the current open-access behavior.
- The Angular app must initialize Keycloak before rendering, introducing a brief loading delay on startup.
