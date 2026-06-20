# Code Quality Refactor Plan

> **Goal**: Achieve 10/10 code quality for the TeacherSuite Angular frontend  
> **Current Rating**: 6/10  
> **Angular Version**: 21.0.0

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Duplicated Interfaces & Models](#duplicated-interfaces--models)
4. [Inconsistent State Management](#inconsistent-state-management)
5. [ChangeDetectorRef Overuse](#changedetectorref-overuse)
6. [Form Validation Duplication](#form-validation-duplication)
7. [Magic Numbers & Constants](#magic-numbers--constants)
8. [Error Handling Improvements](#error-handling-improvements)
9. [Code Organization](#code-organization)
10. [Linting & Formatting](#linting--formatting)
11. [Best Practices Checklist](#best-practices-checklist)
12. [Sources & References](#sources--references)

---

## Executive Summary

The codebase functions correctly but suffers from inconsistent patterns, duplicated code, excessive manual change detection (~100+ `cdr.detectChanges()` calls), and mixed state management approaches. This plan provides a concrete path to clean, maintainable, DRY code.

---

## Current State Analysis

### Codebase Statistics

| Metric | Value |
|--------|-------|
| TypeScript files | 26 |
| HTML templates | 12 |
| SCSS files | 23 |
| Total `cdr.detectChanges()` calls | 100+ |
| Duplicated interfaces | 2 (`ProgrammingLanguage`, `AgeGroup`) |
| Components with mixed signal/property state | 5 |
| Spec files | 1 (only `app.spec.ts`) |

---

## Duplicated Interfaces & Models

### Problem

`ProgrammingLanguage` is defined in **two** service files:

```typescript
// programming-language.service.ts:5
export interface ProgrammingLanguage {
  id: number;
  name: string;
  label: string;
  color: string;
}

// course.service.ts:14
export interface ProgrammingLanguage {
  id: number;
  name: string;
  label: string;
  color: string;
}
```

`AgeGroup` is defined in `course.service.ts` but also used by `group.service.ts` (imported from course service).

### Fix — Shared Models Directory

```
src/app/models/
├── paged-result.model.ts        # Existing
├── programming-language.model.ts # New — shared interface
├── age-group.model.ts           # New — shared interface
├── teacher.model.ts             # New — extract from teacher.service.ts
├── student.model.ts             # New — extract from student.service.ts
├── course.model.ts              # New — extract from course.service.ts
├── group.model.ts               # New — extract from group.service.ts
└── lesson.model.ts              # New — extract from lesson.service.ts
```

```typescript
// src/app/models/programming-language.model.ts
export interface ProgrammingLanguage {
  id: number;
  name: string;
  label: string;
  color: string;
}

// Then import in services:
import { ProgrammingLanguage } from '../models/programming-language.model';
```

> **Angular Best Practice**: "Define data models in dedicated files and share them across services and components."  
> — [Angular Style Guide — 03-03](https://angular.dev/style-guide#style-03-03)

---

## Inconsistent State Management

### Problem — Mixed Signals and Properties

Components use Angular signals for _some_ state but plain class properties for others:

```typescript
// teachers.ts — Mixed pattern
readonly search = signal('');           // ✅ Signal
readonly page = signal(1);              // ✅ Signal
readonly totalCount = signal(0);        // ✅ Signal
teachers: Teacher[] = [];              // ❌ Plain property
loading = false;                        // ❌ Plain property
error: string | null = null;           // ❌ Plain property
showModal = false;                      // ❌ Plain property
```

This inconsistency forces manual `cdr.detectChanges()` calls because Angular's automatic change detection doesn't know when plain properties change (especially in async callbacks from `fetch()`-wrapped observables).

### Fix — Consistent Signals-First Approach

```typescript
// teachers.ts — Fully signals-based
readonly search = signal('');
readonly page = signal(1);
readonly pageSize = signal(12);
readonly totalCount = signal(0);
readonly totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()) || 1);

readonly teachers = signal<Teacher[]>([]);
readonly loading = signal(false);
readonly error = signal<string | null>(null);
readonly showModal = signal(false);
readonly isEditMode = signal(false);
```

When all state is signals-based:
- No need for `ChangeDetectorRef` at all
- Angular's automatic change detection handles everything
- State changes are trackable and debuggable
- Enables `ChangeDetectionStrategy.OnPush` for better performance

> **Angular Best Practice**: "Use signals for component state. Signals provide fine-grained reactivity and integrate natively with Angular's change detection."  
> — [Angular Signals Guide](https://angular.dev/guide/signals)

---

## ChangeDetectorRef Overuse

### Problem

The codebase has **100+ `cdr.detectChanges()` calls** across 7 components:

| Component | `cdr.detectChanges()` Count |
|-----------|-----------------------------|
| `teachers.ts` | 18 |
| `students.ts` | 15 |
| `groups.ts` | 20 |
| `courses.ts` | 14 |
| `lessons.ts` | 23 |
| `lesson-detail.ts` | 13 |
| `programming-languages.ts` | 8 |

**Root Cause**: Using `fetch()` API (via `from(Promise)`) pushes resolution outside Angular's zone. Subscribing to these Observables triggers callbacks outside zone, so Angular doesn't detect changes automatically.

### Fix — Two-Part Solution

**Step 1**: Migrate to `HttpClient` (see Security document) — this runs inside Angular's zone automatically.

**Step 2**: Convert all component state to signals — signals trigger change detection automatically.

**Step 3**: Remove all `ChangeDetectorRef` injections and `detectChanges()` calls.

After refactoring, **zero** `cdr.detectChanges()` calls should remain.

> **Angular Best Practice**: "Avoid calling `ChangeDetectorRef.detectChanges()` manually. If you find yourself needing it, it usually indicates a design issue."  
> — [Angular Change Detection Guide](https://angular.dev/best-practices/runtime-performance)

---

## Form Validation Duplication

### Problem

Date-of-birth validation logic is duplicated across:
- `teachers.ts:433-465` — `dateOfBirthValidator` (18+ years, max 122 years)
- `students.ts:540-563` — `dateOfBirthValidator` (7+ years, different rules)
- `students.ts:529-538` — `calculateAge()` utility

Age calculation appears in 3+ places.

### Fix — Shared Validators and Utilities

```typescript
// src/app/shared/validators/date-validators.ts
import { AbstractControl, ValidationErrors } from '@angular/forms';

export function calculateAge(dateString: string): number {
  const date = new Date(dateString);
  const today = new Date();
  let age = today.getFullYear() - date.getFullYear();
  const m = today.getMonth() - date.getMonth();
  if (m < 0 || (m === 0 && today.getDate() < date.getDate())) {
    age--;
  }
  return Math.max(0, age);
}

export function minAgeValidator(minAge: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const age = calculateAge(control.value);
    if (age < minAge) return { tooYoung: { requiredAge: minAge, actualAge: age } };
    return null;
  };
}

export function maxAgeValidator(maxAge: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const age = calculateAge(control.value);
    if (age > maxAge) return { tooOld: { maxAge, actualAge: age } };
    return null;
  };
}

export function futureDateValidator(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const date = new Date(control.value);
  if (date >= new Date()) return { futureDate: true };
  return null;
}
```

### Date Formatting — Also Duplicated

```typescript
// src/app/shared/utils/date-utils.ts
export function formatDate(dateString: string, locale = 'en-US'): string {
  if (!dateString) return 'N/A';
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return 'Invalid Date';
  return date.toLocaleDateString(locale, {
    year: 'numeric', month: 'long', day: 'numeric', timeZone: 'UTC'
  });
}

export function getCurrentDateString(): string {
  const today = new Date();
  return today.toISOString().split('T')[0];
}

export function toDateInputValue(isoString: string): string {
  return isoString?.split('T')[0] || '';
}
```

> **Angular Style Guide 01-01**: "Do define one thing, such as a service or component, per file."  
> Shared utilities should be in their own files.  
> — [Angular Style Guide](https://angular.dev/style-guide#style-01-01)

---

## Magic Numbers & Constants

### Problem — Hardcoded Values Throughout

```typescript
// Scattered across multiple files:
debounceTime(300)                 // teachers.ts, students.ts, groups.ts
pageSize: 12                      // 4 components
pageSizeOptions: [12, 20, 30, 50] // 4 components
Validators.min(1)                 // lessons
Validators.max(180)               // lessons
age >= 18                         // students
age < 7                           // students
age > 122                         // teachers
pageSize: 1000                    // lessons loading all courses
```

### Fix — Centralized Constants

```typescript
// src/app/shared/constants/app.constants.ts
export const APP_CONSTANTS = {
  SEARCH_DEBOUNCE_MS: 300,
  DEFAULT_PAGE_SIZE: 12,
  PAGE_SIZE_OPTIONS: [12, 20, 30, 50] as readonly number[],
  MAX_PAGE_SIZE: 1000,
} as const;

export const VALIDATION_CONSTANTS = {
  MIN_STUDENT_AGE: 7,
  MIN_TEACHER_AGE: 18,
  MAX_HUMAN_AGE: 122,
  MIN_LESSON_DURATION: 1,
  MAX_LESSON_DURATION: 180,
  DEFAULT_LESSON_DURATION: 90,
  TOKEN_REFRESH_VALIDITY_SECONDS: 30,
} as const;

export const FILE_CONSTANTS = {
  ALLOWED_EXTENSIONS: ['.md', '.docx', '.txt'] as readonly string[],
  ALLOWED_MIME_TYPES: [
    'text/markdown',
    'text/plain',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  ] as readonly string[],
  MAX_FILE_SIZE_BYTES: 10 * 1024 * 1024, // 10MB
} as const;
```

---

## Error Handling Improvements

### Problem — Inconsistent Error Patterns

```typescript
// Pattern 1: Generic message, log to console
error: (error) => {
  this.error = 'Failed to load teachers. Please try again.';
  console.error('Error loading teachers:', error);
}

// Pattern 2: Check status code
error: (error) => {
  const status = error?.status as number | undefined;
  if (status === 409) { ... }
}

// Pattern 3: Use error.detail from ProblemDetails
error: (err) => {
  this.modalError = err?.detail || 'Failed to update lesson.';
}
```

### Fix — Unified Error Handler Utility

```typescript
// src/app/shared/utils/error-handler.ts
import { ApiError } from '../services/api.service';

export interface UserFriendlyError {
  message: string;
  isConflict: boolean;
  details?: string;
}

export function handleApiError(error: unknown, fallbackMessage: string): UserFriendlyError {
  if (error instanceof ApiError) {
    if (error.status === 409) {
      return {
        message: error.detail || 'A conflict occurred. The resource may be in use.',
        isConflict: true,
        details: error.detail,
      };
    }
    return {
      message: error.detail || fallbackMessage,
      isConflict: false,
      details: error.detail,
    };
  }
  return { message: fallbackMessage, isConflict: false };
}
```

---

## Code Organization

### Current Structure — Flat Pages

```
src/app/
├── pages/           # All page components
├── services/        # All services
├── auth/            # Keycloak
├── components/      # 1 shared component
├── models/          # 1 model
├── styles/          # SCSS partials
└── environments/    # Config
```

### Recommended Structure — Feature-Based

```
src/app/
├── core/                      # Singleton services, interceptors, guards
│   ├── interceptors/
│   │   ├── auth.interceptor.ts
│   │   └── error.interceptor.ts
│   ├── guards/
│   │   └── auth.guard.ts
│   ├── services/
│   │   ├── keycloak.service.ts
│   │   └── api.service.ts
│   └── core.providers.ts
├── shared/                    # Shared components, pipes, directives, utils
│   ├── components/
│   │   ├── pagination-bar/
│   │   ├── confirm-dialog/
│   │   └── error-alert/
│   ├── validators/
│   │   └── date-validators.ts
│   ├── utils/
│   │   ├── date-utils.ts
│   │   └── error-handler.ts
│   ├── constants/
│   │   └── app.constants.ts
│   ├── models/
│   │   ├── paged-result.model.ts
│   │   ├── programming-language.model.ts
│   │   └── age-group.model.ts
│   └── pipes/
│       └── format-date.pipe.ts
├── features/                  # Feature modules (lazy-loaded)
│   ├── teachers/
│   │   ├── services/teacher.service.ts
│   │   ├── teachers.component.ts
│   │   └── teachers.routes.ts
│   ├── students/
│   ├── courses/
│   ├── groups/
│   ├── lessons/
│   ├── programming-languages/
│   └── age-groups/
├── app.ts
├── app.html
├── app.routes.ts
└── app.config.ts
```

> **Angular Style Guide 04-07**: "Create feature areas for application features."  
> — [Angular Style Guide](https://angular.dev/style-guide#style-04-07)

---

## Linting & Formatting

### Current State
- Prettier configured in `package.json` but no ESLint
- No lint script in `package.json`
- No pre-commit hooks

### Fix — Add Angular ESLint

```bash
ng add @angular-eslint/schematics
```

Add to `package.json`:
```json
{
  "scripts": {
    "lint": "ng lint",
    "lint:fix": "ng lint --fix",
    "format": "prettier --write \"src/**/*.{ts,html,scss}\""
  }
}
```

Add husky for pre-commit:
```bash
npm install husky lint-staged --save-dev
npx husky init
```

---

## Best Practices Checklist

| # | Practice | Status | Priority |
|---|----------|--------|----------|
| 1 | Extract duplicated interfaces to shared models | ❌ | High |
| 2 | Convert all component state to signals | ⚠️ Partial | High |
| 3 | Remove all `cdr.detectChanges()` calls | ❌ | High |
| 4 | Extract shared validators (date, age) | ❌ | High |
| 5 | Extract shared utilities (date formatting) | ❌ | Medium |
| 6 | Centralize constants (magic numbers) | ❌ | Medium |
| 7 | Unified error handler utility | ❌ | Medium |
| 8 | Reorganize to feature-based structure | ❌ | Medium |
| 9 | Add Angular ESLint | ❌ | Medium |
| 10 | Add Prettier pre-commit hooks | ❌ | Low |
| 11 | One thing per file (style guide 01-01) | ⚠️ Partial | Low |

---

## Sources & References

| Source | URL |
|--------|-----|
| Angular Style Guide | https://angular.dev/style-guide |
| Angular Signals Guide | https://angular.dev/guide/signals |
| Angular Change Detection | https://angular.dev/best-practices/runtime-performance |
| Angular ESLint | https://github.com/angular-eslint/angular-eslint |
| DRY Principle | https://en.wikipedia.org/wiki/Don%27t_repeat_yourself |
| Angular Project Structure Best Practices | https://angular.dev/style-guide#application-structure-and-ngmodules |
