# Testing Strategy

> **Goal**: Achieve 10/10 test coverage and confidence for the TeacherSuite Angular frontend  
> **Current Rating**: 2/10  
> **Angular Version**: 21.0.0 | **Test Framework**: Vitest 4.0.8

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Testing Pyramid](#testing-pyramid)
4. [Unit Testing Plan](#unit-testing-plan)
5. [Integration Testing Plan](#integration-testing-plan)
6. [End-to-End Testing Plan](#end-to-end-testing-plan)
7. [Test Infrastructure](#test-infrastructure)
8. [Coverage Targets](#coverage-targets)
9. [Implementation Roadmap](#implementation-roadmap)
10. [Sources & References](#sources--references)

---

## Executive Summary

The TeacherSuite Angular frontend has **virtually no test coverage**. There is exactly **1 spec file** (`app.spec.ts`) with 2 basic tests. No service, page component, guard, pipe, or utility function is tested. This document provides a comprehensive testing strategy covering unit, integration, and E2E tests with a concrete implementation roadmap.

---

## Current State Analysis

### Test Inventory

| Category | Files | Tests | Coverage |
|----------|-------|-------|----------|
| App component | 1 (`app.spec.ts`) | 2 | ~1% |
| Services (7) | 0 | 0 | 0% |
| Page components (9) | 0 | 0 | 0% |
| Shared components (1) | 0 | 0 | 0% |
| Guards (2) | 0 | 0 | 0% |
| Validators | 0 | 0 | 0% |
| Utilities | 0 | 0 | 0% |
| **Total** | **1** | **2** | **~1%** |

### Test Infrastructure

- ✅ Vitest 4.0.8 configured
- ✅ `tsconfig.spec.json` configured
- ✅ Angular TestBed available
- ❌ No mock utilities
- ❌ No test helpers
- ❌ No E2E framework
- ❌ No CI test pipeline
- ❌ No coverage reporting

---

## Testing Pyramid

```
          ┌─────────┐
          │  E2E    │  ~10% of tests
          │  Tests  │  (Playwright/Cypress)
          ├─────────┤
          │ Integ.  │  ~20% of tests
          │ Tests   │  (Component + Service)
          ├─────────┤
          │  Unit   │  ~70% of tests
          │  Tests  │  (Pure functions, pipes, validators)
          └─────────┘
```

### Target Test Distribution

| Level | Count | Purpose |
|-------|-------|---------|
| Unit tests | ~80-100 | Pure functions, validators, pipes, utilities |
| Integration tests | ~40-60 | Component rendering, service+HTTP interaction |
| E2E tests | ~15-20 | Critical user flows (login, CRUD, navigation) |

---

## Unit Testing Plan

### Priority 1 — Pure Functions & Utilities

These are the easiest and highest-value tests — pure functions with no dependencies.

#### Date Validators & Utilities

```typescript
// src/app/shared/validators/date-validators.spec.ts
describe('calculateAge', () => {
  it('should return correct age for past birthday this year', () => {
    const today = new Date();
    const dob = new Date(today.getFullYear() - 25, 0, 1).toISOString();
    expect(calculateAge(dob)).toBe(25);
  });

  it('should return age-1 if birthday has not occurred yet this year', () => { ... });
  it('should return 0 for a baby born this year', () => { ... });
  it('should handle leap year birthdays', () => { ... });
});

describe('minAgeValidator', () => {
  it('should return null for valid age', () => { ... });
  it('should return error for age below minimum', () => { ... });
  it('should return null when control has no value', () => { ... });
});

describe('futureDateValidator', () => {
  it('should return error for future date', () => { ... });
  it('should return null for past date', () => { ... });
  it('should return null for today', () => { ... });
});
```

#### Error Handler Utility

```typescript
// src/app/shared/utils/error-handler.spec.ts
describe('handleApiError', () => {
  it('should detect 409 conflict', () => { ... });
  it('should use detail from ProblemDetails', () => { ... });
  it('should fall back to generic message', () => { ... });
  it('should handle non-ApiError objects', () => { ... });
});
```

### Priority 2 — Services

#### ApiService (Base)

```typescript
// src/app/services/api.service.spec.ts
describe('ApiService', () => {
  describe('error handling', () => {
    it('should parse ProblemDetails error response', () => { ... });
    it('should throw ApiError with status and detail', () => { ... });
    it('should handle 204 No Content', () => { ... });
    it('should handle network errors', () => { ... });
  });

  describe('authentication', () => {
    it('should attach Bearer token to requests', () => { ... });
    it('should refresh expired token before request', () => { ... });
  });
});
```

#### TeacherService

```typescript
// src/app/services/teacher.service.spec.ts
describe('TeacherService', () => {
  let service: TeacherService;
  let httpMock: HttpTestingController; // After HttpClient migration

  it('should fetch paginated teachers', () => {
    service.getAllTeachers({ search: '', page: 1, pageSize: 12 }).subscribe(result => {
      expect(result.items.length).toBe(12);
      expect(result.totalCount).toBe(100);
    });
    const req = httpMock.expectOne('/Teachers?page=1&pageSize=12');
    req.flush(mockPagedResult);
  });

  it('should create a teacher with UTC date', () => { ... });
  it('should handle 409 conflict on delete', () => { ... });
});
```

> **Note**: Service tests become much easier after migrating to `HttpClient` (uses `HttpTestingController`). With the current `fetch()` API, service tests require mocking `global.fetch` which is fragile.

### Priority 3 — Guards

```typescript
// src/app/auth/auth.guard.spec.ts
describe('authGuard', () => {
  it('should allow access when authenticated', () => { ... });
  it('should redirect to home when not authenticated', () => { ... });
});

describe('roleGuard', () => {
  it('should allow access with required role', () => { ... });
  it('should deny access without required role', () => { ... });
  it('should deny when not authenticated', () => { ... });
});
```

### Priority 4 — Pipes (After Creation)

```typescript
// src/app/shared/pipes/format-date.pipe.spec.ts
describe('FormatDatePipe', () => {
  it('should format valid date string', () => {
    const pipe = new FormatDatePipe();
    expect(pipe.transform('2024-06-15')).toBe('June 15, 2024');
  });
  it('should return "N/A" for empty string', () => { ... });
  it('should return "Invalid Date" for malformed input', () => { ... });
});

// src/app/shared/pipes/full-name.pipe.spec.ts
describe('FullNamePipe', () => {
  it('should combine first and last name', () => { ... });
  it('should handle missing firstName', () => { ... });
  it('should handle missing lastName', () => { ... });
  it('should trim whitespace', () => { ... });
});
```

---

## Integration Testing Plan

### Component Tests

Test components with their templates, forms, and mocked services.

#### Teachers Component

```typescript
// src/app/features/teachers/teachers.spec.ts
describe('Teachers Component', () => {
  let component: Teachers;
  let fixture: ComponentFixture<Teachers>;
  let teacherService: jasmine.SpyObj<TeacherService>;

  beforeEach(async () => {
    teacherService = jasmine.createSpyObj('TeacherService', ['getAllTeachers', 'createTeacher', 'deleteTeacher']);
    teacherService.getAllTeachers.and.returnValue(of(mockPagedResult));

    await TestBed.configureTestingModule({
      imports: [Teachers],
      providers: [
        { provide: TeacherService, useValue: teacherService },
        { provide: KeycloakService, useValue: mockKeycloak },
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Teachers);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('rendering', () => {
    it('should display teacher cards', () => { ... });
    it('should show skeleton loading state', () => { ... });
    it('should show empty state when no teachers', () => { ... });
    it('should show error alert on API failure', () => { ... });
  });

  describe('search', () => {
    it('should debounce search input', fakeAsync(() => { ... }));
    it('should update URL query params', () => { ... });
    it('should reset to page 1 on search', () => { ... });
  });

  describe('pagination', () => {
    it('should change page on pagination click', () => { ... });
    it('should change page size', () => { ... });
    it('should show correct total count', () => { ... });
  });

  describe('CRUD modals', () => {
    it('should open add modal', () => { ... });
    it('should validate required fields', () => { ... });
    it('should submit create form', () => { ... });
    it('should open edit modal with pre-filled data', () => { ... });
    it('should show delete confirmation', () => { ... });
    it('should close modal on Escape key', () => { ... });
  });

  describe('role-based visibility', () => {
    it('should hide delete button for non-admin users', () => { ... });
    it('should show seed button for admin users', () => { ... });
  });
});
```

Apply similar patterns for: Students, Courses, Groups, Lessons, LessonDetail, ProgrammingLanguages.

#### PaginationBar Component

```typescript
describe('PaginationBarComponent', () => {
  it('should render page buttons', () => { ... });
  it('should disable prev on first page', () => { ... });
  it('should disable next on last page', () => { ... });
  it('should emit pageChange on click', () => { ... });
  it('should emit pageSizeChange on select', () => { ... });
  it('should show correct visible page range', () => { ... });
  it('should hide when totalPages is 1', () => { ... });
});
```

---

## End-to-End Testing Plan

### Framework Recommendation: Playwright

```bash
npm install --save-dev @playwright/test
npx playwright install
```

### Critical User Flows

```typescript
// e2e/teachers.spec.ts
test.describe('Teachers Page', () => {
  test.beforeEach(async ({ page }) => {
    // Login via Keycloak
    await loginAsAdmin(page);
    await page.goto('/teachers');
  });

  test('should display teacher list', async ({ page }) => {
    await expect(page.locator('.teacher-card')).toHaveCount(12);
  });

  test('should search teachers', async ({ page }) => {
    await page.fill('[aria-label="Search teachers"]', 'John');
    await page.waitForResponse('**/Teachers*');
    await expect(page.locator('.search-count')).toContainText('result');
  });

  test('should create a new teacher', async ({ page }) => {
    await page.click('[aria-label="AddTeacher"]');
    await page.fill('#firstName', 'Test');
    await page.fill('#lastName', 'Teacher');
    // ... fill all fields
    await page.click('button:has-text("Create")');
    await expect(page.locator('.modal-overlay')).not.toBeVisible();
  });

  test('should paginate', async ({ page }) => {
    await page.click('.btn-page:has-text("2")');
    await expect(page).toHaveURL(/page=2/);
  });
});
```

### E2E Test Coverage Map

| Flow | Priority | Est. Tests |
|------|----------|-----------|
| Login/Logout | Critical | 3 |
| Teachers CRUD | High | 6 |
| Students CRUD + Group Assignment | High | 8 |
| Courses CRUD | High | 5 |
| Groups CRUD + Course Assignment | High | 6 |
| Lessons CRUD + File Upload | High | 7 |
| Lesson Detail + Suggestions + Voting | Medium | 5 |
| Programming Languages CRUD | Medium | 4 |
| Navigation + Routing | Medium | 3 |
| Error States | Low | 3 |

---

## Test Infrastructure

### Test Helpers & Mocks

```typescript
// src/testing/mocks/keycloak.mock.ts
export const mockKeycloakService = {
  isAuthenticated: () => true,
  hasRole: (role: string) => role === 'Admin',
  getToken: () => 'mock-jwt-token',
  updateToken: () => Promise.resolve(),
  getEmail: () => 'admin@test.com',
  getUsername: () => 'admin',
};

// src/testing/mocks/mock-data.ts
export const mockTeacher: Teacher = {
  id: '1',
  firstName: 'John',
  lastName: 'Doe',
  email: 'john@test.com',
  dateOfBirth: '1990-01-01',
  programmingLanguages: [],
};

export const mockPagedResult = <T>(items: T[], total?: number): PagedResult<T> => ({
  items,
  totalCount: total ?? items.length,
  page: 1,
  pageSize: 12,
});
```

### CI Integration

```yaml
# .github/workflows/frontend-tests.yml
name: Frontend Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 22
          cache: npm
          cache-dependency-path: src/TeacherSuite.Web/src/teacher-suite-ui/package-lock.json
      - run: npm ci
        working-directory: src/TeacherSuite.Web/src/teacher-suite-ui
      - run: npx vitest run --coverage
        working-directory: src/TeacherSuite.Web/src/teacher-suite-ui
      - uses: actions/upload-artifact@v4
        with:
          name: coverage-report
          path: src/TeacherSuite.Web/src/teacher-suite-ui/coverage/
```

### Vitest Configuration Enhancement

```typescript
// vitest.config.ts
import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    environment: 'jsdom',
    include: ['src/**/*.spec.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov'],
      include: ['src/app/**/*.ts'],
      exclude: ['src/app/**/*.spec.ts', 'src/app/environments/**'],
      thresholds: {
        statements: 80,
        branches: 75,
        functions: 80,
        lines: 80,
      },
    },
    setupFiles: ['src/testing/setup.ts'],
  },
});
```

---

## Coverage Targets

### Phase 1 — Foundation (Target: 40% coverage)

| Area | Target Coverage | Est. Tests |
|------|----------------|-----------|
| Shared validators | 100% | 15 |
| Shared utilities | 100% | 10 |
| Shared pipes | 100% | 8 |
| Guards | 100% | 5 |
| ApiService | 80% | 10 |

### Phase 2 — Services (Target: 60% coverage)

| Area | Target Coverage | Est. Tests |
|------|----------------|-----------|
| TeacherService | 90% | 8 |
| StudentService | 90% | 10 |
| CourseService | 90% | 7 |
| GroupService | 90% | 8 |
| LessonService | 80% | 12 |
| ProgrammingLanguageService | 90% | 6 |

### Phase 3 — Components (Target: 80% coverage)

| Area | Target Coverage | Est. Tests |
|------|----------------|-----------|
| Teachers component | 80% | 15 |
| Students component | 80% | 18 |
| Courses component | 80% | 12 |
| Groups component | 80% | 15 |
| Lessons component | 80% | 14 |
| LessonDetail component | 75% | 12 |
| PaginationBar component | 90% | 7 |

### Phase 4 — E2E (Target: Critical flows covered)

| Area | Est. Tests |
|------|-----------|
| Full CRUD flows | 15 |
| Authentication | 3 |
| Navigation | 3 |

### Final Target: ≥80% Line Coverage, 100% Critical Path Coverage

---

## Implementation Roadmap

| Phase | Timeline | Focus | Tests Added |
|-------|----------|-------|-------------|
| Phase 1 | Week 1-2 | Pure functions, utilities, guards | ~48 |
| Phase 2 | Week 3-4 | Services (requires HttpClient migration) | ~51 |
| Phase 3 | Week 5-8 | Component integration tests | ~93 |
| Phase 4 | Week 9-10 | E2E tests with Playwright | ~21 |
| **Total** | **10 weeks** | **All layers** | **~213** |

---

## Sources & References

| Source | URL |
|--------|-----|
| Angular Testing Guide | https://angular.dev/guide/testing |
| Angular Component Testing | https://angular.dev/guide/testing/components-scenarios |
| Angular Service Testing | https://angular.dev/guide/testing/services |
| Vitest Documentation | https://vitest.dev/guide/ |
| Playwright Documentation | https://playwright.dev/docs/intro |
| Testing Library Angular | https://testing-library.com/docs/angular-testing-library/intro |
| Angular TestBed | https://angular.dev/api/core/testing/TestBed |
| HTTP Testing Controller | https://angular.dev/guide/http/testing |
| Test Pyramid (Martin Fowler) | https://martinfowler.com/articles/practical-test-pyramid.html |
