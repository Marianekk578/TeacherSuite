# Architecture & Design Patterns

> **Goal**: Document the current architecture "as-is" and provide a scalability outlook  
> **Angular Version**: 21.0.0

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Current Architecture (As-Is)](#current-architecture-as-is)
3. [Component Architecture](#component-architecture)
4. [Data Flow Patterns](#data-flow-patterns)
5. [State Management](#state-management)
6. [Design Patterns Used](#design-patterns-used)
7. [Design Patterns Missing](#design-patterns-missing)
8. [Scalability Outlook](#scalability-outlook)
9. [Recommended Target Architecture](#recommended-target-architecture)
10. [Sources & References](#sources--references)

---

## System Overview

TeacherSuite is a **school management application** for managing teachers, students, courses, groups, lessons, and attendance. The Angular frontend is a Single Page Application (SPA) that communicates with an ASP.NET Core backend via REST APIs, authenticated via Keycloak (OpenID Connect).

### System Context Diagram

```
┌──────────────┐     OIDC/PKCE      ┌──────────────┐
│   Browser     │ ◄─────────────────► │  Keycloak    │
│  (Angular 21) │                     │  Auth Server │
└──────┬───────┘                     └──────────────┘
       │ REST API
       │ (Bearer JWT)
       ▼
┌──────────────┐     EF Core         ┌──────────────┐
│  ASP.NET Core │ ◄─────────────────► │  PostgreSQL  │
│  Backend API  │                     │  Database    │
└──────────────┘                     └──────────────┘
```

### Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend Framework | Angular | 21.0.0 |
| Build Tool | Angular CLI | 21.0.1 |
| Language | TypeScript | 5.9.2 |
| Styling | SCSS | - |
| Icons | @ng-icons (Heroicons) | 33.1.0 |
| Auth | Keycloak JS | 26.0.7 |
| Markdown | marked | 17.0.5 |
| Reactive | RxJS | 7.8.0 |
| Testing | Vitest | 4.0.8 |

---

## Current Architecture (As-Is)

### Directory Structure

```
src/app/
├── auth/                          # Authentication
│   ├── keycloak.service.ts        #   Keycloak wrapper (singleton)
│   └── auth.guard.ts              #   Route guards (auth + role)
│
├── services/                      # Data access layer
│   ├── api.service.ts             #   Base HTTP service (fetch-based)
│   ├── teacher.service.ts         #   Teacher CRUD
│   ├── student.service.ts         #   Student CRUD + group assignment
│   ├── course.service.ts          #   Course CRUD + age groups + languages
│   ├── group.service.ts           #   Group CRUD + course assignment + teacher search
│   ├── lesson.service.ts          #   Lesson CRUD + files + suggestions + attendance
│   └── programming-language.service.ts  # PL CRUD + teacher assignment
│
├── models/                        # Shared data models
│   └── paged-result.model.ts      #   Generic PagedResult<T>
│
├── components/                    # Shared UI components
│   └── pagination-bar/            #   Reusable pagination control
│
├── pages/                         # Page (smart) components
│   ├── home/                      #   Landing page (unauthenticated)
│   ├── teachers/                  #   Teacher management
│   ├── students/                  #   Student management
│   ├── courses/                   #   Course management
│   ├── groups/                    #   Group management
│   ├── lessons/                   #   Lesson list (by course)
│   ├── lesson-detail/             #   Lesson content + suggestions
│   ├── age-groups/                #   Age groups (stub)
│   └── programming-languages/     #   Programming language management
│
├── styles/                        # SCSS shared partials
│   ├── _buttons.scss              
│   ├── _forms.scss                
│   ├── _modals.scss               
│   ├── _cards.scss                
│   ├── _skeleton.scss             
│   ├── _alerts.scss               
│   ├── _empty-state.scss          
│   ├── _page-layout.scss          
│   ├── _placeholder.scss          
│   ├── _search.scss               
│   └── _loading.scss              
│
├── environments/                  # Config
│   ├── environment.ts             #   Development
│   └── environment.prod.ts        #   Production (template)
│
├── app.ts                         # Root component
├── app.html                       # Root template (sidebar + router-outlet)
├── app.routes.ts                  # Route definitions
├── app.config.ts                  # App providers
└── app.spec.ts                    # Only test file
```

### Architecture Layer Diagram

```
┌─────────────────────────────────────────────────────┐
│                     PRESENTATION                     │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐          │
│  │ Teachers  │  │ Students │  │ Courses  │  ...     │
│  │   Page    │  │   Page   │  │   Page   │          │
│  └─────┬────┘  └─────┬────┘  └─────┬────┘          │
│        │              │              │               │
│  ┌─────┴──────────────┴──────────────┴─────────┐    │
│  │            SHARED COMPONENTS                 │    │
│  │  PaginationBar, (future: ConfirmDialog...)   │    │
│  └──────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────┤
│                     DATA ACCESS                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐          │
│  │ Teacher   │  │ Student  │  │ Course   │  ...     │
│  │ Service   │  │ Service  │  │ Service  │          │
│  └─────┬────┘  └─────┬────┘  └─────┬────┘          │
│        │              │              │               │
│  ┌─────┴──────────────┴──────────────┴─────────┐    │
│  │              ApiService (Base)                │    │
│  │  fetch() + Bearer token + error parsing      │    │
│  └──────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────┤
│                     AUTHENTICATION                   │
│  ┌──────────────────────────────────────────────┐    │
│  │           KeycloakService (Singleton)         │    │
│  │  init() → login → token refresh → logout     │    │
│  └──────────────────────────────────────────────┘    │
│  ┌──────────────────────────────────────────────┐    │
│  │           Route Guards (auth + role)          │    │
│  └──────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────┘
```

---

## Component Architecture

### Component Classification

| Type | Count | Description |
|------|-------|-------------|
| **Smart (Container) Components** | 9 | Page-level components that manage state, call services, handle routing |
| **Presentational (Dumb) Components** | 1 | `PaginationBarComponent` — receives inputs, emits events |
| **Root Component** | 1 | `App` — shell with sidebar nav + router outlet |

### Current Pattern — "Fat Smart Components"

Each page component is a monolith that handles:
- State management (loading, error, data, modal visibility)
- API calls (directly subscribing to service methods)
- Form building and validation
- UI logic (format dates, calculate ages, filter lists)
- Keyboard event handling

**Example — Teachers component responsibilities**:
```
Teachers (512 lines)
├── State: search, page, teachers[], loading, error, showModal...
├── API: loadTeachers(), createTeacher(), updateTeacher(), deleteTeacher()
├── Forms: teacherForm with 5 validators
├── Modals: add/edit, delete confirm, programming languages, details
├── Formatting: getFullName(), formatDate(), getCurrentDate()
├── Navigation: navigateWithParams(), goToPage()
└── Events: @HostListener('escape'), onSearchInput()
```

### Anti-Pattern: God Components

Most page components are 400-600 lines, handling too many concerns. This makes them:
- Hard to test (too many dependencies)
- Hard to reuse (tightly coupled to specific data)
- Hard to maintain (changes ripple through many methods)

---

## Data Flow Patterns

### Current Data Flow

```
User Action → Component Method → Service.method() → fetch() API
                                                        │
                                                        ▼
Update Component State ← .subscribe(next/error) ← Observable<T>
                 │
                 ▼
  cdr.detectChanges() → Template Re-render
```

### Problems with Current Flow

1. **No centralized state** — Each component independently manages its data
2. **No caching** — Every navigation re-fetches data from the API
3. **No optimistic updates** — UI waits for server confirmation
4. **Manual change detection** — 100+ `cdr.detectChanges()` calls needed

---

## State Management

### Current: Component-Level State (Signals + Properties)

```typescript
// Mixed pattern across components
readonly search = signal('');       // ← Angular Signal
teachers: Teacher[] = [];           // ← Plain property
loading = false;                    // ← Plain property
showModal = false;                  // ← Plain property
readonly totalPages = computed(...); // ← Computed signal
```

### Why This Is Problematic

- Signals and plain properties coexist, requiring different update mechanisms
- No state sharing between components (e.g., refreshing teachers list after assignment)
- No state persistence across navigation (back button loses context)

---

## Design Patterns Used

### 1. Service Inheritance (Template Method) ✅

```typescript
// ApiService provides template methods
class ApiService {
  protected get<T>(url: string): Observable<T> { ... }
  protected post<T>(url: string, body: any): Observable<T> { ... }
}

// Concrete services extend and use
class TeacherService extends ApiService {
  getAllTeachers(query: TeacherQuery) {
    return this.get<PagedResult<Teacher>>(`/Teachers?...`);
  }
}
```

### 2. Factory Pattern (Keycloak Initialization) ✅

```typescript
// APP_INITIALIZER factory
export function initializeKeycloak(keycloakService: KeycloakService) {
  return () => keycloakService.init();
}
```

### 3. Observer Pattern (RxJS) ✅

All API calls return Observables, and components subscribe to them.

### 4. Strategy Pattern (Route Guards) ✅

`authGuard` and `roleGuard` are functional guards implementing the `CanActivateFn` strategy.

### 5. Signal-Based Reactivity (Partial) ⚠️

Used for pagination state but not universally adopted.

---

## Design Patterns Missing

### 1. Smart/Dumb Component Decomposition ❌

Page components should be split into:
- **Smart (Container)**: Manages data, calls services
- **Dumb (Presentational)**: Renders data, emits events

```
TeachersPage (Smart)                TeacherCard (Dumb)
├── Handles API calls          →    ├── @Input teacher: Teacher
├── Manages pagination              ├── @Output edit
├── Manages modals                  ├── @Output delete
└── Routes navigation               └── Pure display logic
```

### 2. Interceptor Chain ❌

No HTTP interceptors for cross-cutting concerns (auth, error handling, logging).

### 3. State Management Pattern ❌

No centralized state (NgRx, NGXS, or even simple Signal stores).

### 4. Repository/Facade Pattern ❌

Services directly expose HTTP calls. A facade pattern would abstract the data source and allow caching, optimistic updates, etc.

### 5. Error Boundary Pattern ❌

No global error handling component or error boundary.

---

## Scalability Outlook

### Current Scale

| Metric | Value |
|--------|-------|
| Pages | 9 |
| Services | 7 |
| Shared components | 1 |
| TypeScript LOC | ~4,500 |
| Routes | 10 |

### Scaling Challenges

#### 1. Bundle Size Growth

**Current**: All routes eagerly loaded in one bundle.  
**At 20+ pages**: Initial bundle exceeds 1MB budget.  
**Fix**: Lazy loading (see Performance document).

#### 2. State Coordination

**Current**: Each page independently fetches and manages data.  
**At scale**: Cross-feature interactions become painful. Example: assigning a student to a group should update both the student detail and group detail views.  
**Fix**: Signal-based state stores or NgRx.

#### 3. Component Complexity

**Current**: Monolithic 400-600 line page components.  
**At scale**: New features (notifications, real-time updates, offline) would balloon these to 1000+ lines.  
**Fix**: Smart/dumb decomposition, feature-level services with facades.

#### 4. API Layer

**Current**: Raw `fetch()` without interceptors.  
**At scale**: Adding logging, retry, caching, rate limiting requires changes to every service.  
**Fix**: `HttpClient` with interceptor chain.

#### 5. Shared Code

**Current**: 1 shared component, 0 shared pipes, duplicated utilities.  
**At scale**: More duplication, more inconsistency.  
**Fix**: Shared library with components, pipes, directives, validators, utils.

### Scalability Roadmap

```
Current (9 pages)          Medium (20 pages)           Large (50+ pages)
┌────────────────┐         ┌────────────────────┐      ┌────────────────────────┐
│ Monolithic SPA │   →     │ Feature-based       │  →   │ Micro-frontends or     │
│ Eager loading  │         │ Lazy loading         │      │ Module Federation      │
│ No state mgmt  │         │ Signal stores        │      │ NgRx + effects         │
│ 1 shared comp  │         │ Shared component lib │      │ Published design system│
│ Raw fetch()    │         │ HttpClient + intrcpt │      │ API gateway + caching  │
└────────────────┘         └────────────────────┘      └────────────────────────┘
```

---

## Recommended Target Architecture

### Phase 1 — Immediate (Current Scale)

```
src/app/
├── core/                         # Singleton services
│   ├── interceptors/             # Auth, error, logging
│   ├── guards/                   # Auth, role
│   ├── services/                 # Keycloak, API base
│   └── core.providers.ts
│
├── shared/                       # Shared reusable code
│   ├── components/               # PaginationBar, ConfirmDialog, ErrorAlert, ModalShell
│   ├── pipes/                    # FormatDate, FullName
│   ├── directives/               # (future: AutoFocus, TrapFocus)
│   ├── validators/               # Date, age validators
│   ├── utils/                    # Date utils, error handler
│   ├── constants/                # App constants
│   └── models/                   # Shared interfaces
│
├── features/                     # Feature components (lazy-loaded)
│   ├── teachers/
│   │   ├── components/           # TeacherCard, TeacherForm
│   │   ├── services/             # TeacherService, TeacherStore
│   │   ├── teachers.component.ts # Smart container
│   │   └── teachers.routes.ts    # Feature routes
│   ├── students/
│   ├── courses/
│   ├── groups/
│   ├── lessons/
│   ├── programming-languages/
│   └── age-groups/
│
└── app configuration files
```

### Phase 2 — Signal-Based State Stores

```typescript
// features/teachers/services/teacher.store.ts
@Injectable({ providedIn: 'root' })
export class TeacherStore {
  private readonly _teachers = signal<Teacher[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _totalCount = signal(0);

  // Public readonly signals
  readonly teachers = this._teachers.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly totalCount = this._totalCount.asReadonly();

  constructor(private teacherService: TeacherService) {}

  loadTeachers(query: TeacherQuery): void {
    this._loading.set(true);
    this._error.set(null);
    this.teacherService.getAllTeachers(query).subscribe({
      next: (result) => {
        this._teachers.set(result.items);
        this._totalCount.set(result.totalCount);
        this._loading.set(false);
      },
      error: (err) => {
        this._error.set(handleApiError(err, 'Failed to load teachers.').message);
        this._loading.set(false);
      },
    });
  }

  // ... create, update, delete methods
}
```

### Phase 3 — Full Scalability (20+ Pages)

Consider:
- **NgRx Signals** for complex state management with effects
- **Shared Component Library** (publishable via npm)
- **API Response Caching** with TTL and invalidation
- **Real-time Updates** via WebSocket/SignalR integration
- **Offline Support** via Service Worker + IndexedDB

---

## Sources & References

| Source | URL |
|--------|-----|
| Angular Architecture Guide | https://angular.dev/style-guide#application-structure-and-ngmodules |
| Angular Signals | https://angular.dev/guide/signals |
| Smart/Dumb Components Pattern | https://blog.angular-university.io/angular-component-design-how-to-avoid-custom-event-hell/ |
| NgRx Signal Store | https://ngrx.io/guide/signals |
| Angular Lazy Loading | https://angular.dev/guide/routing/lazy-loading |
| Micro-Frontends with Angular | https://www.angulararchitects.io/en/blog/the-microfrontend-revolution-part-2-module-federation-with-angular/ |
| Clean Architecture (Robert C. Martin) | https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html |
| SOLID Principles in Angular | https://medium.com/@nicemak/solid-principles-in-angular-f4e58b0e0a1c |
