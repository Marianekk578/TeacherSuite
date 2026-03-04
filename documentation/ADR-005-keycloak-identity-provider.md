# ADR-005: Keycloak as Identity Provider

## Date:
2026-02-26

## Status:
Accepted

## Context:
TeacherSuite currently has no authentication or authorization mechanism. The application needs a secure, standards-based identity provider to manage user identities, roles, and access control for both the Angular frontend and the .NET API backend. 

## Decision:
Keycloak will be used as the identity provider for the following reasons:
- **Standards-compliant**: Full support for OAuth 2.0, OpenID Connect, and SAML 2.0 protocols.
- **Built-in registration and login pages**: Eliminates the need for custom authentication UI — Keycloak's self-registration page can be enabled in Realm settings.
- **Role and group management**: Native support for realm roles that map directly to .NET authorization policies.
- **Angular integration**: Official `keycloak-js` adapter supports PKCE flow with a public client (no client secret required).
- **.NET integration**: Standard JWT Bearer authentication middleware validates Keycloak-issued tokens without additional libraries.

## Consequences:
- Adds Keycloak as an infrastructure dependency (Docker container in development).
- Initial realm and client configuration must be created (can be exported/imported as JSON).
- All API endpoints will require a valid JWT token, changing the current open-access behavior.
- The Angular app must initialize Keycloak before rendering, introducing a brief loading delay on startup.