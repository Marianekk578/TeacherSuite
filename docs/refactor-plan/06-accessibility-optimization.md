# Accessibility Optimization Plan

> **Goal**: Achieve 10/10 accessibility (WCAG 2.2 AA compliance) for the TeacherSuite Angular frontend  
> **Current Rating**: 7/10  
> **Angular Version**: 21.0.0

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Focus Trapping in Modals](#focus-trapping-in-modals)
4. [Keyboard Navigation](#keyboard-navigation)
5. [Screen Reader Support](#screen-reader-support)
6. [Color & Contrast](#color--contrast)
7. [Form Accessibility](#form-accessibility)
8. [Dynamic Content Announcements](#dynamic-content-announcements)
9. [Best Practices Checklist](#best-practices-checklist)
10. [Sources & References](#sources--references)

---

## Executive Summary

The application has a reasonable accessibility baseline: ARIA roles on modals, `aria-label` attributes on many buttons, keyboard Escape handling, and semantic HTML structure. However, it lacks focus trapping in modals, proper ARIA live regions for dynamic updates, consistent keyboard navigation for all interactive elements, and the custom context menu is not keyboard-accessible.

---

## Current State Analysis

### What Works ✅

| Feature | Details |
|---------|---------|
| **Modal ARIA** | `role="dialog"`, `aria-modal="true"`, `aria-labelledby` on most modals |
| **Button Labels** | `aria-label` on Add/Edit/Delete buttons |
| **Search Input** | `aria-label="Search students/teachers"` |
| **Navigation** | `role="list"` on nav sidebar |
| **Keyboard Escape** | `@HostListener('document:keydown.escape')` on all components |
| **Semantic HTML** | `<h1>`, `<h2>`, `<h3>` hierarchy in pages |
| **Home Page** | Cards have `role="button"`, `tabindex="0"`, Enter key handler |

### What's Missing ❌

| Issue | Location | WCAG Criteria |
|-------|----------|---------------|
| No focus trapping in modals | All modal components | 2.4.3 Focus Order |
| No focus restoration after modal close | All modal components | 2.4.3 Focus Order |
| Context menu not keyboard accessible | `lesson-detail.ts` | 2.1.1 Keyboard |
| No live regions for dynamic updates | Search results, CRUD success | 4.1.3 Status Messages |
| Status indicators are color-only | Course status badges | 1.4.1 Use of Color |
| Missing aria-labels on some buttons | Various close buttons | 4.1.2 Name, Role, Value |
| No skip navigation link | `app.html` | 2.4.1 Bypass Blocks |
| Page title not updated on navigation | All routes | 2.4.2 Page Titled |

---

## Focus Trapping in Modals

### Problem

When a modal opens, focus is not trapped inside it. Users can Tab out of the modal and interact with background elements, which is confusing and violates WCAG 2.4.3.

### Fix — Angular CDK Focus Trap

```bash
npm install @angular/cdk
```

```typescript
// Option 1: cdkTrapFocus directive
import { A11yModule } from '@angular/cdk/a11y';

@Component({
  imports: [A11yModule],
  template: `
    <div class="modal" cdkTrapFocus cdkTrapFocusAutoCapture>
      <!-- modal content -->
    </div>
  `,
})
```

```typescript
// Option 2: Custom directive for reuse
import { FocusTrap, FocusTrapFactory } from '@angular/cdk/a11y';

@Directive({ selector: '[appTrapFocus]', standalone: true })
export class TrapFocusDirective implements AfterViewInit, OnDestroy {
  private focusTrap: FocusTrap | null = null;

  constructor(
    private el: ElementRef,
    private focusTrapFactory: FocusTrapFactory
  ) {}

  ngAfterViewInit() {
    this.focusTrap = this.focusTrapFactory.create(this.el.nativeElement);
    this.focusTrap.focusInitialElement();
  }

  ngOnDestroy() {
    this.focusTrap?.destroy();
  }
}
```

### Focus Restoration After Modal Close

```typescript
// Track the trigger element
private previouslyFocused: HTMLElement | null = null;

openModal() {
  this.previouslyFocused = document.activeElement as HTMLElement;
  this.showModal.set(true);
}

closeModal() {
  this.showModal.set(false);
  // Restore focus to the element that triggered the modal
  this.previouslyFocused?.focus();
  this.previouslyFocused = null;
}
```

> **WCAG 2.4.3**: "If a Web page can be navigated sequentially and the navigation sequences affect meaning or operation, focusable components receive focus in an order that preserves meaning and operability."  
> — [WCAG 2.2 — 2.4.3 Focus Order](https://www.w3.org/WAI/WCAG22/Understanding/focus-order.html)

---

## Keyboard Navigation

### Problem 1 — Context Menu Not Keyboard Accessible

The lesson detail context menu is triggered by right-click only:

```html
<!-- CURRENT — mouse-only -->
<div class="markdown-area" (contextmenu)="onMarkdownContextMenu($event)">
```

### Fix — Add Keyboard Trigger

```html
<!-- Add keyboard shortcut for context menu -->
<div class="markdown-area"
     (contextmenu)="onMarkdownContextMenu($event)"
     (keydown.shift.f10)="onMarkdownContextMenu($event)"
     tabindex="0"
     role="region"
     aria-label="Lesson content — right-click or press Shift+F10 to add a suggestion">
```

### Problem 2 — Missing Skip Navigation

The sidebar navigation must be tabbed through before reaching main content.

### Fix — Add Skip Link

```html
<!-- app.html — first element in body -->
<a class="skip-link" href="#main-content">Skip to main content</a>

<nav class="sidebar">...</nav>

<main id="main-content">
  <router-outlet></router-outlet>
</main>
```

```scss
.skip-link {
  position: absolute;
  left: -9999px;
  z-index: 999;
  padding: 0.5rem 1rem;
  background: #667eea;
  color: white;
  text-decoration: none;

  &:focus {
    left: 0.5rem;
    top: 0.5rem;
  }
}
```

### Problem 3 — Page Title Not Updated on Route Change

### Fix — Angular Title Strategy

```typescript
// app.routes.ts
export const routes: Routes = [
  { path: '', component: Home, title: 'TeacherSuite — Home' },
  { path: 'teachers', ..., title: 'TeacherSuite — Teachers' },
  { path: 'courses', ..., title: 'TeacherSuite — Courses' },
  // ...
];

// app.config.ts
import { TitleStrategy } from '@angular/router';

// Angular uses the route title automatically with provideRouter
```

> **WCAG 2.4.2**: "Web pages have titles that describe topic or purpose."  
> — [WCAG 2.2 — 2.4.2 Page Titled](https://www.w3.org/WAI/WCAG22/Understanding/page-titled.html)

---

## Screen Reader Support

### Problem — No ARIA Live Regions

When a search completes, items are added/deleted, or errors appear, screen readers don't announce these changes.

### Fix — ARIA Live Region Component

```typescript
// src/app/shared/components/live-announcer/live-announcer.ts
import { LiveAnnouncer } from '@angular/cdk/a11y';

@Injectable({ providedIn: 'root' })
export class AppAnnouncer {
  constructor(private liveAnnouncer: LiveAnnouncer) {}

  announceSearchResults(count: number) {
    this.liveAnnouncer.announce(`${count} results found`, 'polite');
  }

  announceSuccess(action: string) {
    this.liveAnnouncer.announce(`${action} successful`, 'polite');
  }

  announceError(message: string) {
    this.liveAnnouncer.announce(`Error: ${message}`, 'assertive');
  }
}
```

Usage in components:

```typescript
// teachers.ts
constructor(private announcer: AppAnnouncer) {}

loadTeachers() {
  this.teacherService.getAllTeachers(query).subscribe({
    next: (result) => {
      this.teachers.set(result.items);
      this.announcer.announceSearchResults(result.totalCount);
    },
    error: () => {
      this.announcer.announceError('Failed to load teachers');
    },
  });
}

createTeacher() {
  // ... on success
  this.announcer.announceSuccess('Teacher created');
}
```

> **WCAG 4.1.3**: "In content implemented using markup languages, status messages can be programmatically determined through role or properties such that they can be presented to the user by assistive technologies without receiving focus."  
> — [WCAG 2.2 — 4.1.3 Status Messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages.html)

---

## Color & Contrast

### Problem — Color-Only Status Indicators

Course status badges use color classes:

```html
<span class="status-badge" [ngClass]="getStatusClass(assignment.status)">
  {{ getStatusLabel(assignment.status) }}
</span>
```

Classes: `status-planned`, `status-active`, `status-completed`, `status-cancelled` — distinguished only by color.

### Fix — Add Icons or Patterns

```html
<span class="status-badge" [ngClass]="getStatusClass(assignment.status)">
  <span class="status-icon" aria-hidden="true">{{ getStatusIcon(assignment.status) }}</span>
  {{ getStatusLabel(assignment.status) }}
</span>
```

```typescript
getStatusIcon(status: number): string {
  switch (status) {
    case 0: return '📋'; // Planned
    case 1: return '▶️'; // Active
    case 2: return '✅'; // Completed
    case 3: return '❌'; // Cancelled
    default: return '';
  }
}
```

### Contrast Verification

Ensure all text meets WCAG 2.2 AA minimum contrast ratios:
- Normal text: 4.5:1
- Large text (≥18pt or ≥14pt bold): 3:1
- UI components: 3:1

Use tools like [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/) to verify.

---

## Form Accessibility

### Problem — Error Messages Not Associated

Form errors are displayed as generic text, not linked to the invalid field:

```html
<!-- CURRENT -->
<div *ngIf="modalError" class="alert alert-error">{{ modalError }}</div>
```

### Fix — Associate Errors with Fields

```html
<!-- Per-field error association -->
<div class="form-group">
  <label for="firstName">First Name *</label>
  <input id="firstName"
         formControlName="firstName"
         [attr.aria-invalid]="form.get('firstName')?.invalid && form.get('firstName')?.touched"
         [attr.aria-describedby]="form.get('firstName')?.errors ? 'firstName-error' : null" />
  <div id="firstName-error" class="field-error"
       *ngIf="form.get('firstName')?.invalid && form.get('firstName')?.touched"
       role="alert">
    First name is required
  </div>
</div>
```

### Required Field Indicators

```html
<!-- Explicit required indicators for screen readers -->
<label for="firstName">
  First Name <span aria-hidden="true">*</span>
  <span class="sr-only">(required)</span>
</label>
```

```scss
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
```

---

## Dynamic Content Announcements

### Problem — Loading States Not Announced

When data is loading, sighted users see a spinner. Screen reader users have no indication.

### Fix

```html
<!-- Loading state -->
<div *ngIf="loading()" role="status" aria-live="polite">
  <div class="spinner" aria-hidden="true"></div>
  <span class="sr-only">Loading teachers...</span>
</div>

<!-- Empty state -->
<div *ngIf="!loading() && teachers().length === 0" role="status">
  <p>No teachers found.</p>
</div>
```

---

## Best Practices Checklist

| # | WCAG Criteria | Practice | Status | Priority |
|---|---------------|----------|--------|----------|
| 1 | 2.4.3 | Focus trapping in modals | ❌ | Critical |
| 2 | 2.4.3 | Focus restoration after modal close | ❌ | Critical |
| 3 | 4.1.3 | ARIA live regions for dynamic content | ❌ | High |
| 4 | 2.1.1 | Keyboard-accessible context menu | ❌ | High |
| 5 | 2.4.1 | Skip navigation link | ❌ | High |
| 6 | 2.4.2 | Page title updates on navigation | ❌ | High |
| 7 | 1.4.1 | Non-color status indicators | ⚠️ Partial | Medium |
| 8 | 1.3.1 | Associate errors with form fields | ❌ | Medium |
| 9 | 4.1.2 | ARIA invalid on form fields | ❌ | Medium |
| 10 | 1.4.3 | Verify contrast ratios | ⚠️ Unverified | Medium |
| 11 | 2.4.7 | Visible focus indicators | ✅ | Done |
| 12 | 4.1.2 | Modal roles and labels | ✅ | Done |
| 13 | 2.1.1 | Escape key handling | ✅ | Done |
| 14 | 4.1.2 | Button aria-labels | ⚠️ Most | Low |

---

## Sources & References

| Source | URL |
|--------|-----|
| WCAG 2.2 Specification | https://www.w3.org/TR/WCAG22/ |
| Angular Accessibility Guide | https://angular.dev/best-practices/a11y |
| Angular CDK A11y Module | https://material.angular.io/cdk/a11y/overview |
| ARIA Authoring Practices — Dialog | https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/ |
| WebAIM Contrast Checker | https://webaim.org/resources/contrastchecker/ |
| Angular CDK FocusTrap | https://material.angular.io/cdk/a11y/overview#focustrap |
| WCAG Quick Reference | https://www.w3.org/WAI/WCAG22/quickref/ |
