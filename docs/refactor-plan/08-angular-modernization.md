# Angular Modernization Plan

> **Goal**: Achieve 10/10 alignment with Angular 21 best practices and capabilities  
> **Current Rating**: 6/10  
> **Angular Version**: 21.0.0

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current Angular Feature Adoption](#current-angular-feature-adoption)
3. [Control Flow Migration](#control-flow-migration)
4. [Signals Adoption](#signals-adoption)
5. [Modern Output API](#modern-output-api)
6. [HttpClient Migration](#httpclient-migration)
7. [Deferrable Views](#deferrable-views)
8. [Other Modern Features](#other-modern-features)
9. [Migration Roadmap](#migration-roadmap)
10. [Sources & References](#sources--references)

---

## Executive Summary

While the project uses Angular 21 and has adopted **some** modern features (standalone components, signals for pagination, `input()` function, `takeUntilDestroyed`), it hasn't adopted several key Angular 21 features: the new control flow syntax (`@if`, `@for`), the `output()` function, `HttpClient` with functional interceptors, deferrable views (`@defer`), `ChangeDetectionStrategy.OnPush`, or lazy loading. This plan provides a complete modernization path.

---

## Current Angular Feature Adoption

| Feature | Angular Version | Status | Files Affected |
|---------|----------------|--------|---------------|
| Standalone components | 14+ | ✅ Used | All components |
| `signal()` / `computed()` | 16+ | ⚠️ Partial | 5 components (pagination only) |
| `input()` function | 17+ | ⚠️ Partial | PaginationBar only |
| `output()` function | 17+ | ❌ Not used | PaginationBar uses `@Output()` |
| `inject()` function | 14+ | ✅ Used | Most components |
| `takeUntilDestroyed()` | 16+ | ⚠️ Partial | Courses, Groups, Lessons |
| `@if` / `@for` / `@switch` | 17+ | ❌ Not used | 0 uses, 190+ `*ngIf`/`*ngFor` |
| `@defer` blocks | 17+ | ❌ Not used | - |
| `ChangeDetectionStrategy.OnPush` | 2+ | ❌ Not used | - |
| `HttpClient` | 4+ | ❌ Not used | Uses `fetch()` |
| Lazy loading routes | 2+ | ❌ Not used | All eager |
| Functional interceptors | 15+ | ❌ Not used | No interceptors |
| Route `title` property | 14+ | ❌ Not used | - |
| `provideHttpClient()` | 15+ | ❌ Not used | - |

---

## Control Flow Migration

### Problem — Using Legacy Structural Directives

The codebase uses `*ngIf` and `*ngFor` (190+ instances) — the legacy Angular template syntax. Angular 17+ introduced built-in control flow with `@if`, `@for`, and `@switch` blocks, which:

- Are **more performant** (no need for `CommonModule` import)
- Have **built-in `track`** in `@for` (replaces `trackBy`)
- Support **`@empty`** block in `@for`
- Have **clearer syntax** for complex conditions

### Automated Migration

Angular CLI provides an automated migration:

```bash
ng generate @angular/core:control-flow
```

This will convert all templates automatically.

### Manual Examples

**Before (Legacy)**:
```html
<div *ngIf="loading" class="loading-state">...</div>
<div *ngIf="!loading && teachers.length === 0" class="empty-state">...</div>
<div *ngIf="!loading && teachers.length > 0" class="teachers-grid">
  <div *ngFor="let teacher of teachers" class="teacher-card">...</div>
</div>
```

**After (Modern)**:
```html
@if (loading()) {
  <div class="loading-state">...</div>
} @else if (teachers().length === 0) {
  <div class="empty-state">...</div>
} @else {
  <div class="teachers-grid">
    @for (teacher of teachers(); track teacher.id) {
      <div class="teacher-card">...</div>
    } @empty {
      <div class="empty-state">No teachers found.</div>
    }
  </div>
}
```

### Benefits

1. **No more `CommonModule` import** — Built-in syntax doesn't need imports
2. **Automatic `track`** — `@for` requires `track` expression, ensuring DOM reuse
3. **`@empty` block** — Eliminates separate `*ngIf` for empty states
4. **Type narrowing** — `@if` narrows types in the block (like TypeScript)

### Impact Count

| Directive | Count | Replacement |
|-----------|-------|-------------|
| `*ngIf` | ~120 | `@if` / `@else` |
| `*ngFor` | ~70 | `@for (track)` |
| `*ngIf` + `*ngFor` (conditional lists) | ~15 | `@if` + `@for` with `@empty` |

> **Angular Best Practice**: "Use the built-in control flow blocks (`@if`, `@for`, `@switch`) instead of structural directives. The built-in control flow is more performant and provides better type checking."  
> — [Angular Control Flow Guide](https://angular.dev/guide/templates/control-flow)

---

## Signals Adoption

### Current: Partial — Only Pagination Uses Signals

```typescript
// CURRENT — 5 components mix signals + properties
readonly search = signal('');      // Signal
teachers: Teacher[] = [];          // Property
loading = false;                   // Property
```

### Target: Full Signals for All Component State

```typescript
// TARGET — Everything is a signal
readonly search = signal('');
readonly teachers = signal<Teacher[]>([]);
readonly loading = signal(false);
readonly error = signal<string | null>(null);
readonly showModal = signal(false);
readonly isEditMode = signal(false);
readonly totalCount = signal(0);
readonly totalPages = computed(() =>
  Math.ceil(this.totalCount() / this.pageSize()) || 1
);
```

### Template Updates

```html
<!-- Before: plain property -->
<div *ngIf="loading">...</div>

<!-- After: signal call -->
@if (loading()) {
  ...
}
```

### Signal-Based RxJS Interop

Use `toSignal()` for converting Observables to Signals:

```typescript
import { toSignal } from '@angular/core/rxjs-interop';

// Convert route params to signal
readonly routeParams = toSignal(this.route.queryParams);
readonly currentPage = computed(() => {
  const params = this.routeParams();
  return parseInt(params?.['page'] ?? '1', 10);
});
```

> **Angular Best Practice**: "Signals are Angular's recommended approach for reactive state management. They integrate deeply with the framework for optimal performance."  
> — [Angular Signals Guide](https://angular.dev/guide/signals)

---

## Modern Output API

### Problem — `@Output()` Decorator Instead of `output()` Function

```typescript
// CURRENT — PaginationBarComponent
@Output() pageChange = new EventEmitter<number>();
@Output() pageSizeChange = new EventEmitter<number>();
```

### Fix — Use `output()` Function

```typescript
// MODERN — Angular 17+
import { output } from '@angular/core';

export class PaginationBarComponent {
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly totalPages = input.required<number>();
  readonly pageSizeOptions = input<number[]>([12, 20, 30, 50]);

  readonly pageChange = output<number>();
  readonly pageSizeChange = output<number>();
}
```

### Automated Migration

```bash
ng generate @angular/core:output-migration
```

> **Angular Best Practice**: "Use the `output()` function instead of the `@Output()` decorator for consistency with the `input()` function API."  
> — [Angular Output API](https://angular.dev/guide/components/output-fn)

---

## HttpClient Migration

### Problem — Raw `fetch()` API

The `ApiService` uses the native `fetch()` API wrapped in `from(Promise)`:

```typescript
// CURRENT — api.service.ts
protected get<T>(url: string): Observable<T> {
  return from(
    this.getAuthHeaders().then(headers =>
      fetch(url, { method: 'GET', headers })
        .then(response => this.handleResponse<T>(response))
    )
  );
}
```

### Why This Is a Problem

1. **Runs outside Angular zone** → requires manual `detectChanges()`
2. **No interceptor chain** → auth token attached manually per-request
3. **No `HttpTestingController`** → service tests require mocking global `fetch`
4. **No automatic XSRF** → must be handled manually
5. **No progress events** → can't track upload progress

### Fix — Migrate to `HttpClient`

```typescript
// app.config.ts
import { provideHttpClient, withInterceptors } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([authInterceptor, errorInterceptor])
    ),
    // ...
  ],
};
```

```typescript
// api.service.ts — MODERNIZED
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ApiService {
  protected readonly http = inject(HttpClient);

  protected get<T>(url: string): Observable<T> {
    return this.http.get<T>(url);
  }

  protected post<T>(url: string, body: unknown): Observable<T> {
    return this.http.post<T>(url, body);
  }

  protected put<T>(url: string, body: unknown): Observable<T> {
    return this.http.put<T>(url, body);
  }

  protected delete<T>(url: string): Observable<T> {
    return this.http.delete<T>(url);
  }
}
```

```typescript
// core/interceptors/auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const keycloak = inject(KeycloakService);
  if (!keycloak.isAuthenticated()) return next(req);

  return from(keycloak.updateToken(30)).pipe(
    switchMap(() => {
      const token = keycloak.getToken();
      return next(req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      }));
    }),
  );
};
```

```typescript
// core/interceptors/error.interceptor.ts
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Parse ProblemDetails
      const apiError = new ApiError(error.status, error.statusText);
      if (error.error?.detail) apiError.detail = error.error.detail;
      if (error.error?.errors) apiError.errors = error.error.errors;
      return throwError(() => apiError);
    }),
  );
};
```

> **Angular Best Practice**: "Use `provideHttpClient()` with functional interceptors (`withInterceptors()`) for HTTP communication. Functional interceptors are composable, tree-shakeable, and testable."  
> — [Angular HttpClient Guide](https://angular.dev/guide/http)

---

## Deferrable Views

### Angular 17+ Feature — `@defer` Blocks

For components that are expensive to render or only needed for certain interactions:

```html
<!-- Lesson detail — defer markdown rendering until visible -->
@defer (on viewport) {
  <app-markdown-viewer [content]="markdownContent()" />
} @placeholder {
  <div class="skeleton-text">Loading content...</div>
} @loading (minimum 300ms) {
  <div class="spinner"></div>
} @error {
  <p>Failed to load content.</p>
}
```

### Use Cases

| Component | Defer Trigger | Benefit |
|-----------|---------------|---------|
| Modal content | `on interaction` | Don't render until opened |
| Suggestion list | `on viewport` | Below the fold |
| Course details | `on interaction` | Click to load |

> **Angular Best Practice**: "Use `@defer` to lazy-load parts of a template. Deferrable views reduce the initial rendering cost of a component."  
> — [Angular Defer Guide](https://angular.dev/guide/templates/defer)

---

## Other Modern Features

### Route `title` Property

```typescript
// CURRENT — no page titles
{ path: 'teachers', component: Teachers, canActivate: [authGuard] }

// MODERN — route titles
{ path: 'teachers', ..., title: 'TeacherSuite — Teachers' }
```

Angular automatically updates `document.title` when using `provideRouter`.

### `linkedSignal()` (Angular 19+)

For derived mutable state:

```typescript
// Page number that resets when search changes
readonly search = signal('');
readonly page = linkedSignal(() => {
  this.search(); // track search changes
  return 1;      // reset to page 1
});
```

### `resource()` API (Angular 19+)

For declarative async data fetching:

```typescript
readonly teachersResource = resource({
  request: () => ({
    search: this.search(),
    page: this.page(),
    pageSize: this.pageSize(),
  }),
  loader: ({ request }) => {
    return firstValueFrom(
      this.teacherService.getAllTeachers(request)
    );
  },
});

// Access in template:
// teachersResource.value()  — the data
// teachersResource.isLoading() — loading state
// teachersResource.error()  — error state
```

> **Note**: The `resource()` API is still in developer preview as of Angular 19. Evaluate stability before adopting.  
> — [Angular Resource API](https://angular.dev/guide/signals/resource)

---

## Migration Roadmap

### Phase 1 — Quick Wins (1-2 days)

| Task | Automated? | Impact |
|------|-----------|--------|
| Run `ng generate @angular/core:control-flow` | ✅ Auto | Converts all `*ngIf`/`*ngFor` to `@if`/`@for` |
| Run `ng generate @angular/core:output-migration` | ✅ Auto | Converts `@Output()` to `output()` |
| Add `title` to all routes | Manual | SEO + accessibility |
| Remove `CommonModule` imports (after control flow migration) | Manual | Smaller bundle |

### Phase 2 — Signals Conversion (1 week)

| Task | Impact |
|------|--------|
| Convert all component state to signals | Enables OnPush |
| Remove all `ChangeDetectorRef` injections | Cleaner code |
| Add `ChangeDetectionStrategy.OnPush` to all components | Performance |
| Use `toSignal()` for route params | Modern interop |

### Phase 3 — HttpClient Migration (1 week)

| Task | Impact |
|------|--------|
| Add `provideHttpClient(withInterceptors(...))` to app.config | Foundation |
| Create `authInterceptor` functional interceptor | Centralized auth |
| Create `errorInterceptor` functional interceptor | Centralized errors |
| Migrate `ApiService` from `fetch()` to `HttpClient` | All services benefit |
| Remove manual token attachment from services | DRY |

### Phase 4 — Lazy Loading + Defer (2-3 days)

| Task | Impact |
|------|--------|
| Convert all routes to `loadComponent` | Smaller initial bundle |
| Add `PreloadAllModules` strategy | Better UX |
| Add `@defer` for below-fold content | Faster rendering |

---

## Sources & References

| Source | URL |
|--------|-----|
| Angular Control Flow | https://angular.dev/guide/templates/control-flow |
| Angular Signals Guide | https://angular.dev/guide/signals |
| Angular `output()` Function | https://angular.dev/guide/components/output-fn |
| Angular `input()` Function | https://angular.dev/guide/components/inputs |
| Angular HttpClient Guide | https://angular.dev/guide/http |
| Angular HTTP Interceptors | https://angular.dev/guide/http/interceptors |
| Angular Lazy Loading | https://angular.dev/guide/routing/lazy-loading |
| Angular Defer Guide | https://angular.dev/guide/templates/defer |
| Angular `toSignal()` | https://angular.dev/guide/signals/rxjs-interop |
| Angular `resource()` API | https://angular.dev/guide/signals/resource |
| Angular `linkedSignal()` | https://angular.dev/guide/signals/linked-signal |
| Angular Migration Schematics | https://angular.dev/reference/migrations |
| Angular Route Title | https://angular.dev/guide/routing/common-router-tasks#setting-the-page-title |
