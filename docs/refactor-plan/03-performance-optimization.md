# Performance Optimization Plan

> **Goal**: Achieve 10/10 performance for the TeacherSuite Angular frontend  
> **Current Rating**: 6/10  
> **Angular Version**: 21.0.0

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Lazy Loading Routes](#lazy-loading-routes)
4. [Change Detection Strategy](#change-detection-strategy)
5. [Template Performance](#template-performance)
6. [Bundle Size Optimization](#bundle-size-optimization)
7. [Data Loading Optimization](#data-loading-optimization)
8. [Best Practices Checklist](#best-practices-checklist)
9. [Sources & References](#sources--references)

---

## Executive Summary

The app currently eagerly loads all routes (~4,500 lines of TypeScript in one bundle), uses the default change detection strategy with 100+ manual `detectChanges()` calls, has no `trackBy` functions on `*ngFor` loops, and uses method calls in templates for repeated computation. These issues compound into a slower-than-necessary user experience.

---

## Current State Analysis

### Performance Metrics

| Metric | Current | Target |
|--------|---------|--------|
| Route loading strategy | Eager (all-at-once) | Lazy (on-demand) |
| Change detection strategy | Default | OnPush with signals |
| `trackBy` on `*ngFor` | 0 uses | All list iterations |
| Template method calls | 30+ per-render methods | 0 (use pipes/computed) |
| Manual `detectChanges()` calls | 100+ | 0 |
| Bundle budgets | 500KB warn / 1MB error | Tuned per-feature |

---

## Lazy Loading Routes

### Problem — All Routes Eagerly Loaded

```typescript
// CURRENT — app.routes.ts — ALL components imported eagerly
import { Teachers } from './pages/teachers/teachers';
import { Courses } from './pages/courses/courses';
// ... 7 more imports

export const routes: Routes = [
  { path: 'teachers', component: Teachers, canActivate: [authGuard] },
  // ...
];
```

All 9 page components (including their services) are bundled into the initial JavaScript payload. Users visiting the home page download code for Teachers, Courses, Lessons, etc. — even if they never visit those pages.

### Fix — Lazy Load Every Protected Route

```typescript
// RECOMMENDED — app.routes.ts — lazy-loaded
export const routes: Routes = [
  { path: '', component: Home },
  {
    path: 'teachers',
    loadComponent: () => import('./features/teachers/teachers').then(m => m.Teachers),
    canActivate: [authGuard],
  },
  {
    path: 'courses',
    loadComponent: () => import('./features/courses/courses').then(m => m.Courses),
    canActivate: [authGuard],
  },
  {
    path: 'lessons',
    loadComponent: () => import('./features/lessons/lessons').then(m => m.LessonsPage),
    canActivate: [authGuard],
  },
  {
    path: 'lessons/:id',
    loadComponent: () => import('./features/lesson-detail/lesson-detail').then(m => m.LessonDetailPage),
    canActivate: [authGuard],
  },
  {
    path: 'groups',
    loadComponent: () => import('./features/groups/groups').then(m => m.Groups),
    canActivate: [authGuard],
  },
  {
    path: 'students',
    loadComponent: () => import('./features/students/students').then(m => m.Students),
    canActivate: [authGuard],
  },
  {
    path: 'age-groups',
    loadComponent: () => import('./features/age-groups/age-groups').then(m => m.AgeGroups),
    canActivate: [authGuard],
  },
  {
    path: 'programming-languages',
    loadComponent: () => import('./features/programming-languages/programming-languages').then(m => m.ProgrammingLanguages),
    canActivate: [authGuard],
  },
  { path: '**', redirectTo: '' },
];
```

**Expected Impact**: Reduces initial bundle by ~60-70%, each route loads on-demand.

> **Angular Best Practice**: "Lazy-load feature routes to reduce initial load time. Use `loadComponent` for standalone components."  
> — [Angular Lazy Loading Guide](https://angular.dev/guide/routing/lazy-loading)

---

## Change Detection Strategy

### Problem — Default Strategy with Manual Detection

Every component uses the default `ChangeDetectionStrategy.Default`, which checks the **entire** component tree on every event (click, timer, HTTP response). Combined with 100+ manual `detectChanges()` calls (needed because `fetch()` runs outside Angular zone), this is doubly wasteful.

### Fix — `OnPush` + Signals

```typescript
// RECOMMENDED — All components
@Component({
  selector: 'app-teachers',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // ...
})
export class Teachers {
  // All state as signals
  readonly teachers = signal<Teacher[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly totalCount = signal(0);
  readonly totalPages = computed(() => 
    Math.ceil(this.totalCount() / this.pageSize()) || 1
  );
}
```

With `OnPush`:
- Component only re-renders when signal values change, inputs change, or events fire
- No need for `ChangeDetectorRef` at all
- Dramatically reduces change detection cycles

> **Angular Best Practice**: "Use `ChangeDetectionStrategy.OnPush` to optimize change detection. Combine with signals for fine-grained reactivity."  
> — [Angular Performance Guide](https://angular.dev/best-practices/runtime-performance)

---

## Template Performance

### Problem 1 — No `trackBy` on List Iterations

```html
<!-- CURRENT — No trackBy -->
<div class="teacher-card" *ngFor="let teacher of teachers">
```

Without `trackBy`, Angular recreates every DOM element when the list changes (pagination, search). With 12-50 cards per page, this is significant.

### Fix — Add `trackBy` to All `*ngFor` / `@for` Loops

```html
<!-- Using new @for syntax (preferred in Angular 21) -->
@for (teacher of teachers(); track teacher.id) {
  <div class="teacher-card">...</div>
}

<!-- Or with *ngFor + trackBy -->
<div *ngFor="let teacher of teachers; trackBy: trackById" class="teacher-card">
```

```typescript
trackById(index: number, item: { id: string | number }): string | number {
  return item.id;
}
```

### Problem 2 — Method Calls in Templates

```html
<!-- CURRENT — Called on every change detection cycle -->
<h3>{{ getFullName(teacher) }}</h3>
<span>{{ formatDate(teacher.dateOfBirth) }}</span>
<span class="language-label" [class.selected]="isLanguageAssigned(lang)">
```

Each method call runs on every change detection cycle (could be 50+ times per second during scrolling or typing).

### Fix — Use Pipes or Precomputed Values

**Option A — Angular Pipes (recommended)**:

```typescript
// src/app/shared/pipes/format-date.pipe.ts
@Pipe({ name: 'formatDate', standalone: true, pure: true })
export class FormatDatePipe implements PipeTransform {
  transform(dateString: string, locale = 'en-US'): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return 'Invalid Date';
    return date.toLocaleDateString(locale, {
      year: 'numeric', month: 'long', day: 'numeric', timeZone: 'UTC'
    });
  }
}

// src/app/shared/pipes/full-name.pipe.ts
@Pipe({ name: 'fullName', standalone: true, pure: true })
export class FullNamePipe implements PipeTransform {
  transform(entity: { firstName?: string; lastName?: string }): string {
    return `${entity.firstName ?? ''} ${entity.lastName ?? ''}`.trim();
  }
}
```

```html
<!-- Template usage -->
<h3>{{ teacher | fullName }}</h3>
<span>{{ teacher.dateOfBirth | formatDate }}</span>
```

Pure pipes are memoized — they only recalculate when their input reference changes.

**Option B — Precomputed in component**:

```typescript
readonly teacherDisplayItems = computed(() =>
  this.teachers().map(t => ({
    ...t,
    fullName: `${t.firstName} ${t.lastName}`.trim(),
    formattedDate: this.formatDate(t.dateOfBirth),
  }))
);
```

> **Angular Best Practice**: "Avoid complex expressions in templates. Use pure pipes for transformations — they are memoized and only re-evaluated when inputs change."  
> — [Angular Pipes Guide](https://angular.dev/guide/pipes)

---

## Bundle Size Optimization

### Current Budget Configuration

```json
"budgets": [
  { "type": "initial", "maximumWarning": "500kB", "maximumError": "1MB" },
  { "type": "anyComponentStyle", "maximumWarning": "4kB", "maximumError": "12kB" }
]
```

### Recommended Improvements

1. **Preload Strategy** — Preload lazy routes after initial load:

```typescript
// app.config.ts
import { PreloadAllModules } from '@angular/router';

provideRouter(routes, withPreloading(PreloadAllModules))
```

2. **Tree-Shake `marked` Library** — Only import what's needed:

```typescript
// Instead of importing all of marked
import { marked } from 'marked';

// Configure only needed extensions
marked.setOptions({
  gfm: true,
  breaks: true,
});
```

3. **Optimize Icon Imports** — Import only used icons per component (already done ✅)

4. **Image Optimization** — Use `NgOptimizedImage` directive for any images:

```typescript
import { NgOptimizedImage } from '@angular/common';

// In template
<img ngSrc="/logo.png" width="160" height="160" priority />
```

> **Angular Best Practice**: "Use `NgOptimizedImage` for automatic image optimization including lazy loading, srcset generation, and LCP prioritization."  
> — [Angular Image Optimization](https://angular.dev/guide/image-optimization)

---

## Data Loading Optimization

### Problem 1 — Loading All Courses (pageSize: 1000)

```typescript
// lessons.ts:110
this.courseService.getAllCourses({ page: 1, pageSize: 1000 })
```

This loads potentially thousands of courses just for a dropdown.

### Fix — Server-Side Search or Caching

```typescript
// Option 1: Server-side search for course dropdown
onCourseSearch(term: string) {
  this.courseService.searchCourses(term).subscribe(courses => {
    this.courseOptions.set(courses);
  });
}

// Option 2: Cache with TTL
private coursesCache = signal<Course[] | null>(null);
private cacheTimestamp = 0;
private readonly CACHE_TTL = 5 * 60 * 1000; // 5 minutes

loadCourses() {
  if (this.coursesCache() && Date.now() - this.cacheTimestamp < this.CACHE_TTL) {
    return; // Use cached data
  }
  // ... fetch and update cache
}
```

### Problem 2 — Lesson Files N+1 Loading

```typescript
// lessons.ts:166-176 — fires one request per lesson
loadAllLessonFiles() {
  for (const lesson of this.lessons) {
    if (lesson.albumId) {
      this.lessonService.getLessonFiles(lesson.id).subscribe(...)
    }
  }
}
```

### Fix — Batch API or Limit Parallel Requests

```typescript
// Using forkJoin with limited concurrency
import { forkJoin, from, mergeMap } from 'rxjs';

loadAllLessonFiles() {
  const lessonsWithAlbums = this.lessons.filter(l => l.albumId);
  from(lessonsWithAlbums).pipe(
    mergeMap(lesson =>
      this.lessonService.getLessonFiles(lesson.id).pipe(
        map(files => ({ lessonId: lesson.id, files }))
      ),
      3 // max 3 concurrent requests
    )
  ).subscribe(({ lessonId, files }) => {
    this.lessonFiles.set(lessonId, files);
  });
}
```

### Problem 3 — No Debounce on Re-Fetching After Mutations

After create/update/delete, the component immediately calls `loadTeachers()` / `loadStudents()` etc. If a user rapidly creates multiple items, this fires many redundant API calls.

### Fix — Debounce reload or use optimistic updates.

---

## Best Practices Checklist

| # | Practice | Status | Impact |
|---|----------|--------|--------|
| 1 | Lazy load all routes with `loadComponent` | ❌ | High |
| 2 | Use `ChangeDetectionStrategy.OnPush` | ❌ | High |
| 3 | Add `trackBy` / `track` to all list iterations | ❌ | High |
| 4 | Replace template method calls with pure pipes | ❌ | High |
| 5 | Remove all `cdr.detectChanges()` calls | ❌ | High |
| 6 | Cache frequently-accessed data (courses, age groups) | ❌ | Medium |
| 7 | Limit concurrent API requests (lesson files) | ❌ | Medium |
| 8 | Use `PreloadAllModules` for lazy routes | ❌ | Medium |
| 9 | Optimize `marked` import (tree-shake) | ❌ | Low |
| 10 | Use `NgOptimizedImage` for images | ❌ | Low |

---

## Sources & References

| Source | URL |
|--------|-----|
| Angular Lazy Loading | https://angular.dev/guide/routing/lazy-loading |
| Angular Performance Best Practices | https://angular.dev/best-practices/runtime-performance |
| Angular Change Detection | https://angular.dev/guide/change-detection |
| Angular Pipes Guide | https://angular.dev/guide/pipes |
| Angular Image Optimization | https://angular.dev/guide/image-optimization |
| Angular @for Control Flow | https://angular.dev/guide/templates/control-flow |
| Web Vitals (Google) | https://web.dev/vitals/ |
