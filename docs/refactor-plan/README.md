# TeacherSuite Frontend — Comprehensive Refactor Plan

> **Angular 21.0.0** | **Date**: April 2026  
> **Scope**: Frontend Angular SPA code review, architecture analysis, and 10/10 improvement plan

---

## 📊 Current Ratings & Targets

| Category | Current | Target | Document |
|----------|---------|--------|----------|
| 🔒 Security | 5/10 | 10/10 | [01-security-optimization.md](./01-security-optimization.md) |
| 🧹 Code Quality | 6/10 | 10/10 | [02-code-quality-refactor.md](./02-code-quality-refactor.md) |
| ⚡ Performance | 6/10 | 10/10 | [03-performance-optimization.md](./03-performance-optimization.md) |
| 🧪 Testing | 2/10 | 10/10 | [04-testing-strategy.md](./04-testing-strategy.md) |
| 🏗️ Architecture & Design Patterns | 6/10 | 10/10 | [05-architecture-and-design-patterns.md](./05-architecture-and-design-patterns.md) |
| ♿ Accessibility | 7/10 | 10/10 | [06-accessibility-optimization.md](./06-accessibility-optimization.md) |
| 🔧 Maintainability | 6/10 | 10/10 | [07-maintainability-refactor.md](./07-maintainability-refactor.md) |
| 🚀 Angular Modernization | 6/10 | 10/10 | [08-angular-modernization.md](./08-angular-modernization.md) |

---

## 🔍 Code Scan Summary

### Codebase Statistics

| Metric | Value |
|--------|-------|
| Angular Version | 21.0.0 |
| TypeScript Files | 26 |
| HTML Templates | 12 |
| SCSS Files | 23 |
| Total TypeScript LOC | ~4,500 |
| Page Components | 9 |
| Services | 7 |
| Shared Components | 1 |
| Test Files | 1 |
| Test Coverage | ~1% |

### Key Findings

| Finding | Severity | Category |
|---------|----------|----------|
| XSS via `bypassSecurityTrustHtml()` | 🔴 Critical | Security |
| No HttpClient — uses raw `fetch()` | 🟠 High | Security, Performance, Modernization |
| 100+ manual `cdr.detectChanges()` calls | 🟠 High | Code Quality, Performance |
| 0% lazy loading — all routes eager | 🟠 High | Performance |
| Only 1 test file (~1% coverage) | 🟠 High | Testing |
| No `@if`/`@for` — uses legacy `*ngIf`/`*ngFor` | 🟡 Medium | Angular Modernization |
| No `ChangeDetectionStrategy.OnPush` | 🟡 Medium | Performance |
| No focus trapping in modals | 🟡 Medium | Accessibility |
| Duplicated interfaces (`ProgrammingLanguage`) | 🟡 Medium | Code Quality |
| God components (400-600 lines each) | 🟡 Medium | Maintainability |
| Mixed subscription management (3 patterns) | 🟡 Medium | Maintainability |
| No shared components for modals/confirm dialogs | 🟡 Medium | Maintainability |
| Magic numbers throughout codebase | 🔵 Low | Code Quality |
| No ESLint configured | 🔵 Low | Code Quality |

---

## 📋 Master Implementation Roadmap

### Phase 1 — Critical Security (Week 1)
- [ ] Install DOMPurify and sanitize markdown HTML
- [ ] Remove `bypassSecurityTrustHtml()` usage
- [ ] Add CSP meta tag

### Phase 2 — Angular Modernization Quick Wins (Week 2)
- [ ] Run `ng generate @angular/core:control-flow` migration
- [ ] Run `ng generate @angular/core:output-migration`
- [ ] Add `title` to all routes
- [ ] Remove `CommonModule` imports

### Phase 3 — HttpClient Migration (Week 3)
- [ ] Add `provideHttpClient(withInterceptors(...))` to app config
- [ ] Create auth interceptor
- [ ] Create error interceptor
- [ ] Migrate ApiService from `fetch()` to `HttpClient`

### Phase 4 — Signals & Change Detection (Week 4)
- [ ] Convert all component state to signals
- [ ] Remove all `ChangeDetectorRef` injections (100+ calls)
- [ ] Add `ChangeDetectionStrategy.OnPush` to all components

### Phase 5 — Performance (Week 5)
- [ ] Convert all routes to lazy loading (`loadComponent`)
- [ ] Add `trackBy`/`track` to all list iterations
- [ ] Replace template method calls with pure pipes
- [ ] Add data caching for dropdown data

### Phase 6 — Code Quality & Maintainability (Week 6-7)
- [ ] Extract shared models to `/models/` directory
- [ ] Extract shared validators and utilities
- [ ] Create shared components (ConfirmDialog, ModalShell, ErrorAlert)
- [ ] Decompose God components into Smart/Dumb pairs
- [ ] Standardize subscription management (DestroyRef)
- [ ] Centralize constants
- [ ] Add Angular ESLint

### Phase 7 — Accessibility (Week 8)
- [ ] Install @angular/cdk, add focus trapping to all modals
- [ ] Add focus restoration after modal close
- [ ] Add skip navigation link
- [ ] Add ARIA live regions for dynamic content
- [ ] Add keyboard accessibility to context menu
- [ ] Associate form errors with fields

### Phase 8 — Testing (Week 9-14)
- [ ] Phase 1: Unit tests for validators, utilities, pipes (~48 tests)
- [ ] Phase 2: Service tests with HttpTestingController (~51 tests)
- [ ] Phase 3: Component integration tests (~93 tests)
- [ ] Phase 4: E2E tests with Playwright (~21 tests)
- [ ] Configure CI pipeline for automated testing
- [ ] Achieve ≥80% code coverage

---

## 📚 All Sources & References

| Category | Source | URL |
|----------|--------|-----|
| Angular | Official Style Guide | https://angular.dev/style-guide |
| Angular | Signals Guide | https://angular.dev/guide/signals |
| Angular | Control Flow | https://angular.dev/guide/templates/control-flow |
| Angular | HttpClient Guide | https://angular.dev/guide/http |
| Angular | Security Guide | https://angular.dev/best-practices/security |
| Angular | Performance Guide | https://angular.dev/best-practices/runtime-performance |
| Angular | Accessibility Guide | https://angular.dev/best-practices/a11y |
| Angular | Testing Guide | https://angular.dev/guide/testing |
| Angular | Lazy Loading | https://angular.dev/guide/routing/lazy-loading |
| Angular | Defer Guide | https://angular.dev/guide/templates/defer |
| Angular | Pipes Guide | https://angular.dev/guide/pipes |
| Angular | Image Optimization | https://angular.dev/guide/image-optimization |
| Angular | Migration Schematics | https://angular.dev/reference/migrations |
| Security | OWASP XSS Prevention | https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html |
| Security | OWASP CSP Cheat Sheet | https://cheatsheetseries.owasp.org/cheatsheets/Content_Security_Policy_Cheat_Sheet.html |
| Security | DOMPurify | https://github.com/cure53/DOMPurify |
| Accessibility | WCAG 2.2 | https://www.w3.org/TR/WCAG22/ |
| Accessibility | ARIA Patterns | https://www.w3.org/WAI/ARIA/apg/patterns/ |
| Accessibility | Angular CDK A11y | https://material.angular.io/cdk/a11y/overview |
| Testing | Vitest | https://vitest.dev/guide/ |
| Testing | Playwright | https://playwright.dev/docs/intro |
| Testing | Test Pyramid | https://martinfowler.com/articles/practical-test-pyramid.html |
| Architecture | Clean Architecture | https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html |
| Architecture | NgRx Signal Store | https://ngrx.io/guide/signals |
| Tooling | Angular ESLint | https://github.com/angular-eslint/angular-eslint |
