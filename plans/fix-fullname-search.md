# Plan: Fix Full Name Search for Teachers and Students

## Summary

The search functionality in `GetAllTeachersQuery` and `GetAllStudentsQuery` only checks `FirstName`, `LastName`, and email fields individually against the search term. When a user searches by full name (e.g., "Marcin Wójcik"), no field individually contains the full string, so zero results are returned. The fix adds a concatenated `FirstName + " " + LastName` check to the `Where` clause in both query handlers, and adds corresponding unit tests — including a new test file for the teacher query handler which currently has no tests.

## Implementation Steps

### Step 1 · Update `GetAllTeachersQuery.cs` search filter
**File:** `src/TeacherSuite.Application/Teachers/Queries/Get/GetAllTeachersQuery.cs`

In the `Handle` method, modify the `Where` clause inside the `if (!string.IsNullOrWhiteSpace(request.Search))` block. Add a fourth `||` condition that concatenates `FirstName + " " + LastName` and checks `.Contains(search)`.

**Current code (lines ~30-34):**
```csharp
query = query.Where(t =>
    (t.FirstName != null && t.FirstName.ToLower().Contains(search)) ||
    (t.LastName != null && t.LastName.ToLower().Contains(search)) ||
    t.Email.ToLower().Contains(search));
```

**Replace with:**
```csharp
query = query.Where(t =>
    (t.FirstName != null && t.FirstName.ToLower().Contains(search)) ||
    (t.LastName != null && t.LastName.ToLower().Contains(search)) ||
    t.Email.ToLower().Contains(search) ||
    (t.FirstName + " " + t.LastName).ToLower().Contains(search));
```

**Notes:**
- `string + null` in C# produces the original string (no NRE), and EF Core translates `+` to SQL `CONCAT`/`||`, so this is safe even when `FirstName` or `LastName` is null.
- `.ToLower()` is already the established pattern in this codebase for case-insensitive matching.

---

### Step 2 · Update `GetAllStudentsQuery.cs` search filter
**File:** `src/TeacherSuite.Application/Students/Queries/GetAllStudentsQuery.cs`

Apply the identical fix to the student search `Where` clause.

**Current code (lines ~36-40):**
```csharp
query = query.Where(s =>
    (s.FirstName != null && s.FirstName.ToLower().Contains(search)) ||
    (s.LastName != null && s.LastName.ToLower().Contains(search)) ||
    s.ContactEmail.ToLower().Contains(search));
```

**Replace with:**
```csharp
query = query.Where(s =>
    (s.FirstName != null && s.FirstName.ToLower().Contains(search)) ||
    (s.LastName != null && s.LastName.ToLower().Contains(search)) ||
    s.ContactEmail.ToLower().Contains(search) ||
    (s.FirstName + " " + s.LastName).ToLower().Contains(search));
```

> **Steps 1 and 2 can be executed in parallel** — they modify independent files.

---

### Step 3 · Add full-name search test to `GetAllStudentsQueryTests.cs`
**File:** `tests/Application.UnitTests/GetAllStudentsQueryTests.cs`

Add a new `[Fact]` test method `Handle_FiltersStudentsByFullName` after the existing `Handle_FiltersStudentsBySearch` test. The test should:

1. **Arrange** — Create a list of 3+ students with distinct first/last names (e.g., "Marcin" / "Wójcik", "Anna" / "Kowalska", "Jan" / "Nowak"). Use the existing `CreateMockDbSet` helper and `Mock<IApplicationDbContext>` pattern already in the file.
2. **Act** — Call the handler with `Search = "marcin wójcik"` (lowercase to also validate case-insensitive behavior).
3. **Assert** — Exactly 1 result is returned, and it matches "Marcin" / "Wójcik".

Follow the exact pattern of the existing `Handle_FiltersStudentsBySearch` test (same Arrange scaffolding, same assertion style).

---

### Step 4 · Create new `GetAllTeachersQueryTests.cs` test file
**File:** `tests/Application.UnitTests/GetAllTeachersQueryTests.cs` (new file)

No teacher query test file exists. Create one following the established conventions from `GetAllStudentsQueryTests.cs`:

**Structure:**
- Namespace: `Application.UnitTests`
- Class: `GetAllTeachersQueryTests`
- Constructor: set up `IMapper` (using `MapperConfiguration` with `cfg.AddMaps(typeof(TeacherDto).Assembly)`) — no `ICurrentUserService` needed since `GetAllTeachersQueryHandler` doesn't use it.
- Include `CreateMockDbSet<T>` private static helper (same as in other test files).
- Copy the `TestAsyncQueryProvider<T>`, `TestAsyncEnumerable<T>`, and `TestAsyncEnumerator<T>` helper classes at the bottom of the file (matching the established pattern — these are currently duplicated per test file, not shared).

**Required `using` statements:**
```csharp
using AutoMapper;
using Moq;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Teachers.Dtos;
using TeacherSuite.Application.Teachers.Queries.Get;
using TeacherSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
```

**Test methods to include:**

| Test Name | Purpose |
|---|---|
| `Handle_ReturnsPagedResult_WithDefaults` | 15 teachers, default pagination → TotalCount=15, Page=1, PageSize=12, Items.Count=12 |
| `Handle_FiltersTeachersBySearch` | 3 teachers, search by first name "alice" → 1 result |
| `Handle_FiltersTeachersByFullName` | 3 teachers, search "marcin wójcik" → 1 result matching "Marcin" / "Wójcik" |

**Teacher entity construction pattern** (note: `TeacherProgrammingLanguages` collection must be initialized since the query does `.Include()` on it):
```csharp
new Teacher
{
    Id = Guid.NewGuid(),
    FirstName = "Marcin",
    LastName = "Wójcik",
    Email = "marcin@example.com",
    PhoneNumber = "+48 123 456 001",
    DateOfBirth = DateTimeOffset.UtcNow.AddYears(-30),
    TeacherProgrammingLanguages = new List<TeacherProgrammingLanguage>()
}
```

**Mock setup:** The `GetAllTeachersQueryHandler` takes `(IApplicationDbContext db, IMapper mapper)`. Mock `db.Teachers` to return the mock DbSet.

> **Steps 3 and 4 can be executed in parallel** — they modify/create independent test files.

---

### Step 5 · Build and run tests
**Command:** `dotnet build && dotnet test` from solution root.

Verify:
- All new tests pass.
- All existing tests still pass (no regressions).
- No build warnings introduced.

---

## Parallelization Summary

```
Step 1 (Teachers query) ──┐
                           ├── then ── Step 3 (Students test) ──┐
Step 2 (Students query) ──┘            Step 4 (Teachers test) ──┼── Step 5 (Build + Test)
                                                                 │
```

- Steps 1 + 2 are parallel (independent source files).
- Steps 3 + 4 are parallel (independent test files), and can also run in parallel with Steps 1 + 2.
- Step 5 is sequential after all code changes.

## Edge Cases to Handle

1. **Null FirstName or LastName** — `string + null` in C# produces the non-null part (e.g., `"Marcin" + " " + null` → `"Marcin "`). In SQL (via EF Core), `CONCAT` handles nulls similarly. The existing individual-field null checks (`t.FirstName != null &&`) remain in place, so the concatenation line is purely additive — no risk of breaking existing behavior.

2. **Extra whitespace in search term** — Already handled by the existing `.Trim()` call on the search string. The concatenation uses a single space, matching natural "FirstName LastName" input.

3. **Search matches partial full name** — `.Contains()` semantics means searching "Marc Wó" would also match "Marcin Wójcik" — this is correct and expected behavior (consistent with how individual field search works).

4. **Case insensitivity** — `.ToLower()` on the concatenated result ensures case-insensitive matching, consistent with existing behavior.

5. **EF Core SQL translation** — `string.Concat` / `+` operator and `.ToLower().Contains()` are all supported by EF Core's SQL translation for all major providers (SQL Server, PostgreSQL, SQLite). No client-side evaluation risk.

6. **Reversed name order** — Searching "Wójcik Marcin" (LastName first) will **not** match with this implementation. This is acceptable for the current scope — the concatenation is `FirstName + " " + LastName` only.

## Open Questions

1. **Should we also support "LastName FirstName" order?** — Some cultures put last name first. Adding `(t.LastName + " " + t.FirstName).ToLower().Contains(search)` would handle this but adds query complexity. **Recommendation:** Skip for now; can be added later if users request it.

2. **Should the `TestAsync*` helper classes be extracted into a shared file?** — They're currently duplicated across `GetAllStudentsQueryTests.cs`, `GetAllGroupsQueryTests.cs`, and `GetAllCoursesQueryTests.cs`. Adding yet another copy in `GetAllTeachersQueryTests.cs` increases the duplication. **Recommendation:** Out of scope for this bug fix; track as tech debt for a separate cleanup PR.
