# Maintainability Refactor Plan

> **Goal**: Achieve 10/10 maintainability for the TeacherSuite Angular frontend  
> **Current Rating**: 6/10  
> **Angular Version**: 21.0.0

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Component Decomposition](#component-decomposition)
4. [Shared Component Library](#shared-component-library)
5. [Service Refactoring](#service-refactoring)
6. [Subscription Management](#subscription-management)
7. [Naming Conventions](#naming-conventions)
8. [Documentation Standards](#documentation-standards)
9. [Best Practices Checklist](#best-practices-checklist)
10. [Sources & References](#sources--references)

---

## Executive Summary

The codebase has good intent but poor execution in maintainability: 400-600 line monolithic page components, inconsistent subscription management (three different patterns), duplicated utility code, and only 1 shared component. This plan provides a path to a maintainable, modular codebase that developers can confidently extend.

---

## Current State Analysis

### Component Size Metrics

| Component | Lines of TS | Lines of HTML | Modals | Complexity |
|-----------|-------------|---------------|--------|------------|
| `teachers.ts` | 512 | 216 | 4 | High |
| `students.ts` | 618 | 326 | 4 | Very High |
| `courses.ts` | 408 | 216 | 3 | High |
| `groups.ts` | 573 | 262 | 5 | Very High |
| `lessons.ts` | 504 | 182 | 2 | High |
| `lesson-detail.ts` | 432 | 179 | 3 | High |
| `programming-languages.ts` | 228 | 102 | 2 | Medium |
| `age-groups.ts` | 14 | ~10 | 0 | Stub |
| `home.ts` | 32 | ~40 | 0 | Low |

**Average page component size**: ~410 lines — well above the recommended 200-line guideline.

### Subscription Patterns (3 Different Approaches!)

```typescript
// Pattern 1: Manual Subscription array + OnDestroy (teachers.ts, students.ts)
private subscriptions: Subscription[] = [];
ngOnDestroy() { this.subscriptions.forEach(s => s.unsubscribe()); }

// Pattern 2: DestroyRef + takeUntilDestroyed (courses.ts, lessons.ts)
private destroyRef = inject(DestroyRef);
this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(...)

// Pattern 3: Mixed — both patterns in same component! (groups.ts)
private destroyRef = inject(DestroyRef);
private subscriptions: Subscription[] = [];
```

---

## Component Decomposition

### Current Problem — God Components

Each page component handles: data fetching, state management, form building, modal display, formatting, navigation, and event handling.

### Recommended Decomposition Pattern

**Before (Monolithic)**:
```
TeachersPage (512 lines)
└── Everything: list, cards, modals, forms, search, pagination
```

**After (Decomposed)**:
```
TeachersPage (Smart Container — ~120 lines)
├── TeacherSearchBar (Presentational — ~30 lines)
├── TeacherCard (Presentational — ~40 lines)
├── TeacherForm (Presentational — ~80 lines)
├── TeacherDetailModal (Presentational — ~60 lines)
├── ConfirmDialog (Shared — ~30 lines)
└── PaginationBar (Shared — existing)
```

### Example: Extract TeacherCard Component

```typescript
// features/teachers/components/teacher-card.ts
@Component({
  selector: 'app-teacher-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgIconComponent],
  template: `
    <div class="teacher-card">
      <div class="teacher-avatar">
        {{ teacher().firstName.charAt(0) }}{{ teacher().lastName.charAt(0) }}
      </div>
      <div class="teacher-info">
        <h3>{{ teacher() | fullName }}</h3>
        <div class="detail-item">
          <ng-icon name="heroEnvelope" size="16" />
          <span>{{ teacher().email }}</span>
        </div>
      </div>
      <div class="teacher-actions">
        <button class="btn-icon" (click)="edit.emit()" title="Edit">
          <ng-icon name="heroPencil" size="18" />
        </button>
        <button class="btn-icon" (click)="delete.emit()" title="Delete">
          <ng-icon name="heroTrash" size="18" />
        </button>
      </div>
    </div>
  `,
})
export class TeacherCardComponent {
  teacher = input.required<Teacher>();
  edit = output();
  delete = output();
}
```

### Benefits

- Each component is **< 100 lines** — easy to read and test
- **OnPush change detection** on all presentational components
- **Reusable** — `TeacherCard` could be used in group assignments, search results, etc.
- **Testable** — presentational components can be tested without mocking services

> **Angular Style Guide 05-03**: "Components should not directly access the datastore. Keep them focused on presentation."  
> — [Angular Style Guide](https://angular.dev/style-guide#style-05-03)

---

## Shared Component Library

### Current: 1 Shared Component

Only `PaginationBarComponent` is shared. All other UI patterns are duplicated.

### Recommended Shared Components

| Component | Purpose | Used By | Status |
|-----------|---------|---------|--------|
| `PaginationBarComponent` | Page navigation | 3 pages | ✅ Exists |
| `ConfirmDialogComponent` | Delete confirmation | 7 pages | ❌ Duplicated |
| `ModalShellComponent` | Modal overlay + header + footer | 7 pages | ❌ Duplicated |
| `ErrorAlertComponent` | Error display with dismiss | 9 pages | ❌ Duplicated |
| `LoadingSpinnerComponent` | Loading state display | 9 pages | ❌ Duplicated |
| `EmptyStateComponent` | No data state | 6 pages | ❌ Duplicated |
| `SearchBarComponent` | Search with debounce | 2 pages | ❌ Duplicated |
| `SkeletonLoaderComponent` | Card loading skeleton | 4 pages | ❌ Duplicated |

### Example: ConfirmDialog Component

Currently, delete confirmation is copy-pasted across 7 pages (~30 lines each = 210 lines total):

```html
<!-- Duplicated in EVERY page -->
<div class="modal-overlay" *ngIf="showDeleteConfirm" (click)="cancelDelete()">
  <div class="modal modal-small">
    <div class="modal-header">
      <h2>Confirm Delete</h2>
      <button class="modal-close" (click)="cancelDelete()">&times;</button>
    </div>
    <div class="modal-body">
      <p>Are you sure you want to delete <strong>{{ name }}</strong>?</p>
      <div class="modal-actions">
        <button class="btn btn-secondary" (click)="cancelDelete()">Cancel</button>
        <button class="btn btn-danger" (click)="onDelete()">Delete</button>
      </div>
    </div>
  </div>
</div>
```

**Shared version**:

```typescript
// shared/components/confirm-dialog/confirm-dialog.ts
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (visible()) {
      <div class="modal-overlay" (click)="cancel.emit()">
        <div class="modal modal-small" (click)="$event.stopPropagation()"
             cdkTrapFocus cdkTrapFocusAutoCapture
             role="alertdialog" aria-modal="true" [attr.aria-labelledby]="'confirm-title'">
          <div class="modal-header">
            <h2 id="confirm-title">{{ title() }}</h2>
            <button class="modal-close" (click)="cancel.emit()" aria-label="Close">&times;</button>
          </div>
          <div class="modal-body">
            <p>{{ message() }}</p>
            <div class="modal-actions">
              <button class="btn btn-secondary" (click)="cancel.emit()">Cancel</button>
              <button class="btn btn-danger" (click)="confirm.emit()">{{ confirmLabel() }}</button>
            </div>
          </div>
        </div>
      </div>
    }
  `,
})
export class ConfirmDialogComponent {
  visible = input.required<boolean>();
  title = input('Confirm Delete');
  message = input.required<string>();
  confirmLabel = input('Delete');

  confirm = output();
  cancel = output();
}
```

Usage:
```html
<app-confirm-dialog
  [visible]="showDeleteConfirm()"
  [message]="'Are you sure you want to delete ' + teacherToDelete()?.fullName + '?'"
  (confirm)="deleteTeacher()"
  (cancel)="cancelDelete()" />
```

**Savings**: ~210 duplicated lines → 1 shared component, 7 × 5-line usages.

---

## Service Refactoring

### Problem — Cross-Service Dependencies

- `GroupService` calls `/Teachers` and `/Courses` endpoints (should use TeacherService and CourseService)
- `StudentService` calls `/Groups` endpoint (should use GroupService)
- `CourseService` calls `/AgeGroups` and `/ProgrammingLanguages` endpoints

### Fix — Each Service Only Accesses Its Own Domain

```typescript
// Instead of GroupService calling /Teachers directly
// GroupService should use TeacherService:

@Injectable({ providedIn: 'root' })
export class GroupService extends ApiService {
  constructor(private teacherService: TeacherService) { super(); }

  // This endpoint stays in GroupService
  searchTeachersForGroup(search: string): Observable<Teacher[]> {
    return this.teacherService.getAllTeachers({ search, page: 1, pageSize: 10 })
      .pipe(map(result => result.items));
  }
}
```

Alternatively, create a dedicated facade service per feature that coordinates between domain services.

---

## Subscription Management

### Problem — Three Different Patterns

The codebase uses 3 different subscription management patterns.

### Fix — Standardize on `DestroyRef` + `takeUntilDestroyed`

```typescript
// STANDARD — All components
@Component({ ... })
export class MyComponent {
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit() {
    // For long-lived subscriptions (query params, search debounce)
    this.route.queryParams
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => { ... });

    // For one-shot API calls, no need for takeUntilDestroyed
    // (they complete after one emission)
  }
}
```

**Rules**:
- ✅ Use `takeUntilDestroyed(this.destroyRef)` for long-lived subscriptions (queryParams, Subject, valueChanges)
- ✅ Fire-and-forget is OK for one-shot API calls (GET/POST/PUT/DELETE) — they complete after one emission
- ❌ Never use `Subscription[]` + manual `ngOnDestroy` cleanup
- ❌ Never mix patterns in the same component

> **Angular Best Practice**: "Use `takeUntilDestroyed` from `@angular/core/rxjs-interop` to automatically unsubscribe when the component is destroyed."  
> — [Angular RxJS Interop](https://angular.dev/guide/signals/rxjs-interop)

---

## Naming Conventions

### Current Issues

| Issue | Example | Fix |
|-------|---------|-----|
| Inconsistent component class names | `Teachers`, `LessonsPage`, `LessonDetailPage` | Use consistent suffix: `TeachersPage` |
| Page components named as plurals | `Students`, `Groups` | Add `Page` suffix: `StudentsPage` |
| Service model DTOs defined inline | `CreateTeacherDto` in service file | Extract to model files |
| Status enums as magic numbers | `status: 0 \| 1 \| 2 \| 3` | Use TypeScript enum |

### Recommended Naming

```typescript
// Status enum instead of magic numbers
export enum CourseStatus {
  Planned = 0,
  Active = 1,
  Completed = 2,
  Cancelled = 3,
}

// Status label map
export const COURSE_STATUS_LABELS: Record<CourseStatus, string> = {
  [CourseStatus.Planned]: 'Planned',
  [CourseStatus.Active]: 'Active',
  [CourseStatus.Completed]: 'Completed',
  [CourseStatus.Cancelled]: 'Cancelled',
};
```

> **Angular Style Guide 02-01**: "Use consistent names for all symbols. Follow a pattern that describes the symbol's feature then its type."  
> — [Angular Style Guide](https://angular.dev/style-guide#style-02-01)

---

## Documentation Standards

### Add JSDoc to Public APIs

```typescript
/**
 * Service for managing teacher entities.
 * Handles CRUD operations, pagination, and search.
 */
@Injectable({ providedIn: 'root' })
export class TeacherService extends ApiService {
  /**
   * Fetches a paginated list of teachers.
   * @param query - Search, pagination, and sort parameters
   * @returns Observable of paginated teacher results
   */
  getAllTeachers(query: TeacherQuery): Observable<PagedResult<Teacher>> {
    // ...
  }
}
```

### Component Documentation

```typescript
/**
 * Teachers management page.
 *
 * Features:
 * - Paginated teacher list with search
 * - CRUD operations (create, edit, delete)
 * - Programming language assignment
 * - Role-based actions (Admin/Supervisor only)
 *
 * @route /teachers
 * @guards authGuard
 */
@Component({ ... })
export class TeachersPage { ... }
```

---

## Best Practices Checklist

| # | Practice | Status | Priority |
|---|----------|--------|----------|
| 1 | Decompose God components (< 200 lines per file) | ❌ | High |
| 2 | Extract shared ConfirmDialog component | ❌ | High |
| 3 | Extract shared ModalShell component | ❌ | High |
| 4 | Standardize subscription management (DestroyRef) | ⚠️ Mixed | High |
| 5 | Extract shared models to `/models/` | ⚠️ Partial | Medium |
| 6 | Use TypeScript enums for status codes | ❌ | Medium |
| 7 | Consistent naming conventions (Page suffix) | ❌ | Medium |
| 8 | Add JSDoc to public APIs | ❌ | Medium |
| 9 | Extract shared ErrorAlert component | ❌ | Medium |
| 10 | Extract shared EmptyState component | ❌ | Low |
| 11 | Single responsibility per service | ⚠️ Partial | Low |

---

## Sources & References

| Source | URL |
|--------|-----|
| Angular Style Guide | https://angular.dev/style-guide |
| Angular Style Guide 01-01 (One Per File) | https://angular.dev/style-guide#style-01-01 |
| Angular Style Guide 05-03 (Delegate Logic) | https://angular.dev/style-guide#style-05-03 |
| Angular RxJS Interop | https://angular.dev/guide/signals/rxjs-interop |
| Smart/Dumb Components | https://blog.angular-university.io/angular-component-design-how-to-avoid-custom-event-hell/ |
| Single Responsibility Principle | https://en.wikipedia.org/wiki/Single-responsibility_principle |
