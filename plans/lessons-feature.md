# Implementation Plan: Lessons Feature

## Summary

Add a comprehensive Lessons feature to TeacherSuite where each Course contains an ordered sequence of Lessons. Each lesson has metadata (title, description, duration, order, requirement icons) and attached material (either inline Markdown stored in the database or a Word document stored on disk). A LessonAttendance entity tracks when a Group attended a specific Lesson on a given date/time. Teachers can propose suggestions (comments) on lesson materials—context-aware for Markdown, general for Word docs—and other teachers can upvote/downvote those suggestions. Only Supervisors and Admins may edit lesson materials; teachers contribute through the suggestion workflow. Course completion is gated by all lessons being attended. Navigation supports both a course-detail drill-down and a standalone Lessons page with course/lesson pickers.

---

## Design Decisions & Trade-offs

| Decision | Rationale |
|---|---|
| **`Lesson.Id` is `int`** | Follows `Course` pattern (reference/template data, not user-generated). Auto-increment PK. |
| **`LessonAttendance.Id` is `Guid`** | Follows `GroupCourse` pattern (activity/event data created at runtime). |
| **`LessonSuggestion.Id` is `Guid`** | User-generated content created at runtime. |
| **`SuggestionVote` uses composite key `(SuggestionId, TeacherId)`** | Enforces one-vote-per-teacher-per-suggestion at the database level, follows `TeacherProgrammingLanguage` pattern. |
| **Single `Lesson` entity with nullable `MarkdownContent` + nullable `FilePath`** | Simpler than a polymorphic `LessonMaterial` table. Exactly one of the two fields is populated; enforced by validation. |
| **`LessonMaterialType` enum** | Discriminator column makes it explicit which content type the lesson uses. Enables clean conditional logic. |
| **`IFileStorageService` interface in Application layer** | Keeps file I/O behind an abstraction. Infrastructure implements with filesystem; could swap to blob storage later without touching Application. |
| **File path stored as relative path in DB** | Base directory configured via `appsettings.json`. Files named `{CourseName}_Lesson{Order:D2}_{sanitized-original-name}.docx`. |
| **Markdown content in DB, not filesystem** | Markdown is text data that benefits from querying, versioning, and atomic transactions with its metadata. |
| **Requirement icons stored as a JSON column** | A `List<LessonRequirementIcon>` value object serialized to JSONB in PostgreSQL. Avoids an extra table for simple display metadata. |
| **Suggestions have `SelectedText` + `SelectionStartIndex` + `SelectionEndIndex` for markdown context** | Enables "right-click on selected text" UX. For Word docs these are null (general comment). |
| **Vote score computed at query time** | `SuggestionVote` table with `IsUpvote` bool. Net score = `SUM(CASE WHEN IsUpvote THEN 1 ELSE -1 END)`. Avoids denormalized counters and race conditions. |
| **Course completion gating** | Modify `UpdateGroupCourseStatus` handler to check all lessons have attendance records for the group before allowing transition to `Completed`. |

---

## Entity Model

```
Course (existing)
 └── Lesson (1:N)  — Course has many Lessons
      ├── LessonAttendance (1:N)  — Lesson attended by Groups on dates
      └── LessonSuggestion (1:N)  — Teacher suggestions on materials
           └── SuggestionVote (1:N)  — Upvote/downvote per teacher
```

### New Entities

**Lesson**
| Property | Type | Notes |
|---|---|---|
| Id | int | PK, auto-increment |
| CourseId | int | FK → Course |
| Title | string | Required, max 200 |
| Description | string? | Optional, max 2000 |
| DurationMinutes | int | Default 90 |
| OrderNumber | int | 1-based position within course |
| MaterialType | LessonMaterialType | Enum: Markdown / WordDocument |
| MarkdownContent | string? | Populated when MaterialType = Markdown |
| FilePath | string? | Relative path on disk when MaterialType = WordDocument |
| OriginalFileName | string? | Original upload filename for download content-disposition |
| RequirementIcons | `List<RequirementIcon>` | JSON column — each has `IconName` (string) and `Tooltip` (string) |
| Course | Course? | Navigation property |
| Attendances | `ICollection<LessonAttendance>` | Navigation |
| Suggestions | `ICollection<LessonSuggestion>` | Navigation |

**LessonAttendance**
| Property | Type | Notes |
|---|---|---|
| Id | Guid | PK |
| LessonId | int | FK → Lesson |
| GroupId | Guid | FK → Group |
| Date | DateOnly | When the lesson was attended |
| StartTime | TimeOnly | Start time |
| Notes | string? | Optional teacher notes |
| Lesson | Lesson? | Navigation |
| Group | Group? | Navigation |
| *Inherits* | BaseAuditableEntity | Created/Modified tracking |

**LessonSuggestion**
| Property | Type | Notes |
|---|---|---|
| Id | Guid | PK |
| LessonId | int | FK → Lesson |
| TeacherId | Guid | FK → Teacher (author) |
| Content | string | The suggestion text, required, max 2000 |
| SelectedText | string? | For markdown: the text the teacher selected |
| SelectionStartIndex | int? | Start character index in markdown |
| SelectionEndIndex | int? | End character index in markdown |
| IsResolved | bool | Default false; supervisor marks resolved |
| Lesson | Lesson? | Navigation |
| Teacher | Teacher? | Navigation |
| Votes | `ICollection<SuggestionVote>` | Navigation |
| *Inherits* | BaseAuditableEntity | Created/Modified tracking |

**SuggestionVote**
| Property | Type | Notes |
|---|---|---|
| SuggestionId | Guid | Composite PK part 1, FK → LessonSuggestion |
| TeacherId | Guid | Composite PK part 2, FK → Teacher |
| IsUpvote | bool | true = upvote, false = downvote |
| Suggestion | LessonSuggestion? | Navigation |
| Teacher | Teacher? | Navigation |

### New Value Object (owned type / JSON serialized)

**RequirementIcon**
| Property | Type | Notes |
|---|---|---|
| IconName | string | Heroicon identifier (e.g., "heroDevicePhoneMobile") |
| Tooltip | string | Tooltip text (e.g., "Students need a phone") |

### New Enums

**LessonMaterialType**
| Value | Int |
|---|---|
| Markdown | 0 |
| WordDocument | 1 |

**VoteType** *(not needed — using `IsUpvote` bool instead)*

---

## New Files to Create

### Domain Layer — `src/TeacherSuite.Domain/`

| # | File Path | Purpose |
|---|---|---|
| 1 | `Entities/Lesson.cs` | Lesson entity |
| 2 | `Entities/LessonAttendance.cs` | Attendance tracking entity |
| 3 | `Entities/LessonSuggestion.cs` | Teacher suggestion entity |
| 4 | `Entities/SuggestionVote.cs` | Vote entity (composite PK) |
| 5 | `Entities/RequirementIcon.cs` | Value object for JSON column |
| 6 | `Enums/LessonMaterialType.cs` | Markdown / WordDocument enum |
| 7 | `Events/LessonCreatedEvent.cs` | Domain event |
| 8 | `Events/LessonSuggestionCreatedEvent.cs` | Domain event |

### Application Layer — `src/TeacherSuite.Application/`

| # | File Path | Purpose |
|---|---|---|
| 9 | `Common/Interfaces/IFileStorageService.cs` | File storage abstraction |
| 10 | `Lessons/Dtos/LessonDto.cs` | Lesson DTO with embedded AutoMapper profile |
| 11 | `Lessons/Dtos/LessonDetailDto.cs` | Full lesson detail DTO (includes material content) |
| 12 | `Lessons/Dtos/LessonAttendanceDto.cs` | Attendance DTO |
| 13 | `Lessons/Dtos/LessonSuggestionDto.cs` | Suggestion DTO with vote score |
| 14 | `Lessons/Commands/Common/LessonValidationRules.cs` | Shared validation rules |
| 15 | `Lessons/Commands/Create/CreateLesson.cs` | Create lesson command + handler |
| 16 | `Lessons/Commands/Create/CreateLessonCommandValidator.cs` | Validator |
| 17 | `Lessons/Commands/Update/UpdateLesson.cs` | Update lesson command + handler |
| 18 | `Lessons/Commands/Update/UpdateLessonCommandValidator.cs` | Validator |
| 19 | `Lessons/Commands/Delete/DeleteLesson.cs` | Delete lesson command + handler |
| 20 | `Lessons/Commands/UpdateMaterial/UpdateLessonMaterial.cs` | Update markdown content or replace Word file |
| 21 | `Lessons/Commands/UpdateMaterial/UpdateLessonMaterialCommandValidator.cs` | Validator |
| 22 | `Lessons/Commands/Reorder/ReorderLessons.cs` | Reorder lessons within course |
| 23 | `Lessons/Queries/GetLessonsByCourseQuery.cs` | Get all lessons for a course (paginated) |
| 24 | `Lessons/Queries/GetLessonByIdQuery.cs` | Get single lesson with full detail |
| 25 | `Lessons/Queries/GetAllLessonsQuery.cs` | Standalone page: all lessons with course/lesson filtering |
| 26 | `Lessons/Queries/DownloadLessonFileQuery.cs` | Returns file stream + filename for Word download |
| 27 | `Lessons/EventHandlers/LessonCreatedEventHandler.cs` | Event handler (logging) |
| 28 | `LessonAttendances/Commands/Create/RecordLessonAttendance.cs` | Record attendance command + handler |
| 29 | `LessonAttendances/Commands/Create/RecordLessonAttendanceCommandValidator.cs` | Validator |
| 30 | `LessonAttendances/Commands/Delete/DeleteLessonAttendance.cs` | Delete attendance command + handler |
| 31 | `LessonAttendances/Queries/GetAttendancesByGroupQuery.cs` | Get attendance records for a group |
| 32 | `LessonSuggestions/Dtos/SuggestionVoteDto.cs` | Vote DTO |
| 33 | `LessonSuggestions/Commands/Create/CreateLessonSuggestion.cs` | Create suggestion command + handler |
| 34 | `LessonSuggestions/Commands/Create/CreateLessonSuggestionCommandValidator.cs` | Validator |
| 35 | `LessonSuggestions/Commands/Delete/DeleteLessonSuggestion.cs` | Delete own suggestion |
| 36 | `LessonSuggestions/Commands/Resolve/ResolveLessonSuggestion.cs` | Mark suggestion as resolved (Supervisor/Admin) |
| 37 | `LessonSuggestions/Commands/Vote/VoteOnSuggestion.cs` | Upvote/downvote + handler |
| 38 | `LessonSuggestions/Queries/GetSuggestionsByLessonQuery.cs` | Get suggestions for a lesson |
| 39 | `LessonSuggestions/EventHandlers/LessonSuggestionCreatedEventHandler.cs` | Event handler (logging) |

### Infrastructure Layer — `src/TeacherSuite.Infrastructure/`

| # | File Path | Purpose |
|---|---|---|
| 40 | `Configurations/LessonConfiguration.cs` | EF config for Lesson (JSON column, indexes) |
| 41 | `Configurations/LessonAttendanceConfiguration.cs` | EF config for LessonAttendance |
| 42 | `Configurations/LessonSuggestionConfiguration.cs` | EF config for LessonSuggestion |
| 43 | `Configurations/SuggestionVoteConfiguration.cs` | EF config for SuggestionVote |
| 44 | `FileStorage/FileStorageService.cs` | Filesystem implementation of IFileStorageService |
| 45 | `FileStorage/FileStorageOptions.cs` | Configuration options (base path) |
| 46 | `Migrations/{timestamp}_AddLessonsFeature.cs` | EF migration (auto-generated) |

### Web Layer (Backend) — `src/TeacherSuite.Web/`

| # | File Path | Purpose |
|---|---|---|
| 47 | `Endpoints/LessonEndpoints.cs` | Route mapping for lessons API |
| 48 | `Endpoints/Lessons.cs` | Lesson endpoint handler class |
| 49 | `Endpoints/LessonAttendanceEndpoints.cs` | Route mapping for attendance API |
| 50 | `Endpoints/LessonAttendances.cs` | Attendance endpoint handler class |
| 51 | `Endpoints/LessonSuggestionEndpoints.cs` | Route mapping for suggestions API |
| 52 | `Endpoints/LessonSuggestions.cs` | Suggestion endpoint handler class |

### Angular Frontend — `src/TeacherSuite.Web/src/teacher-suite-ui/src/app/`

| # | File Path | Purpose |
|---|---|---|
| 53 | `services/lesson.service.ts` | Lesson API service (extends ApiService) |
| 54 | `services/lesson-suggestion.service.ts` | Suggestion API service |
| 55 | `services/lesson-attendance.service.ts` | Attendance API service |
| 56 | `pages/lessons/lessons.ts` | Standalone lessons page component |
| 57 | `pages/lessons/lessons.html` | Standalone lessons page template |
| 58 | `pages/lessons/lessons.scss` | Standalone lessons page styles |
| 59 | `pages/lesson-detail/lesson-detail.ts` | Lesson detail/material view component |
| 60 | `pages/lesson-detail/lesson-detail.html` | Detail page template |
| 61 | `pages/lesson-detail/lesson-detail.scss` | Detail page styles |
| 62 | `components/markdown-viewer/markdown-viewer.ts` | Markdown renderer with right-click context menu |
| 63 | `components/markdown-viewer/markdown-viewer.html` | Markdown viewer template |
| 64 | `components/markdown-viewer/markdown-viewer.scss` | Markdown viewer styles |
| 65 | `components/suggestion-panel/suggestion-panel.ts` | Suggestion list + voting panel component |
| 66 | `components/suggestion-panel/suggestion-panel.html` | Suggestion panel template |
| 67 | `components/suggestion-panel/suggestion-panel.scss` | Suggestion panel styles |

### Tests

| # | File Path | Purpose |
|---|---|---|
| 68 | `tests/Application.UnitTests/GetLessonsByCourseQueryTests.cs` | Query handler tests |
| 69 | `tests/Application.UnitTests/GetLessonByIdQueryTests.cs` | Single lesson query tests |
| 70 | `tests/Application.UnitTests/RecordLessonAttendanceTests.cs` | Attendance command tests |
| 71 | `tests/Application.UnitTests/CreateLessonSuggestionTests.cs` | Suggestion command tests |
| 72 | `tests/Application.UnitTests/VoteOnSuggestionTests.cs` | Voting logic tests |
| 73 | `tests/Application.UnitTests/CourseCompletionGatingTests.cs` | Course completion gating tests |

---

## Existing Files to Modify

| # | File Path | What Changes |
|---|---|---|
| M1 | `src/TeacherSuite.Domain/Entities/Course.cs` | Add `ICollection<Lesson> Lessons` navigation property |
| M2 | `src/TeacherSuite.Domain/Entities/Group.cs` | Add `ICollection<LessonAttendance> LessonAttendances` navigation property |
| M3 | `src/TeacherSuite.Domain/Entities/Teacher.cs` | Add `ICollection<LessonSuggestion> Suggestions` and `ICollection<SuggestionVote> SuggestionVotes` navigation properties |
| M4 | `src/TeacherSuite.Application/Common/Interfaces/IApplicationDbContext.cs` | Add `DbSet<Lesson>`, `DbSet<LessonAttendance>`, `DbSet<LessonSuggestion>`, `DbSet<SuggestionVote>` properties |
| M5 | `src/TeacherSuite.Application/Groups/Commands/AssignCourse/UpdateGroupCourseStatus.cs` | Add lesson-completion gating check before allowing `Completed` status transition |
| M6 | `src/TeacherSuite.Application/Courses/Dtos/CourseDto.cs` | Add `int LessonCount` mapped field and `List<LessonDto>` optional collection |
| M7 | `src/TeacherSuite.Infrastructure/Data/ApplicationDbContext.cs` | Add four new `DbSet<>` properties |
| M8 | `src/TeacherSuite.Infrastructure/DependencyInjection.cs` | Register `IFileStorageService` and bind `FileStorageOptions` from config |
| M9 | `src/TeacherSuite.Web/Program.cs` | Register `Lessons`, `LessonAttendances`, `LessonSuggestions` endpoint classes; map endpoints |
| M10 | `src/TeacherSuite.Web/src/teacher-suite-ui/src/app/app.routes.ts` | Add lesson routes |
| M11 | `src/TeacherSuite.Web/src/teacher-suite-ui/src/app/app.html` | Add "Lessons" nav item to sidebar |
| M12 | `src/TeacherSuite.Web/src/teacher-suite-ui/src/app/app.ts` | Import `heroDocumentText` (or `heroClipboardDocumentList`) icon |
| M13 | `src/TeacherSuite.Web/src/teacher-suite-ui/src/app/pages/courses/courses.html` | Add "View Lessons" link/button to course cards |
| M14 | `src/TeacherSuite.Web/src/teacher-suite-ui/src/app/pages/courses/courses.ts` | Add navigation method to lessons page |
| M15 | `src/TeacherSuite.Web/src/teacher-suite-ui/package.json` | Add `marked` (markdown parsing) and `DOMPurify` (sanitization) dependencies |
| M16 | `src/TeacherSuite.Web/appsettings.json` | Add `FileStorage:BasePath` configuration |
| M17 | `src/TeacherSuite.Web/appsettings.Development.json` | Add `FileStorage:BasePath` dev value |

---

## Step-by-Step Implementation Order

### Phase 1: Domain Layer (Files 1–8)

These have zero dependencies on other layers and establish the data model.

**Step 1.1 — Create enums**
- File 6: `LessonMaterialType.cs`
  ```
  namespace TeacherSuite.Domain.Enums;
  public enum LessonMaterialType { Markdown = 0, WordDocument = 1 }
  ```

**Step 1.2 — Create value object**
- File 5: `RequirementIcon.cs`
  ```
  namespace TeacherSuite.Domain.Entities;
  public class RequirementIcon { string IconName, string Tooltip }
  ```
  Note: This is a simple class (not an entity) — will be stored as JSON via `OwnsMany` or `ToJson()` in EF config.

**Step 1.3 — Create entities**
- File 1: `Lesson.cs` — int Id, CourseId FK, Title, Description, DurationMinutes=90, OrderNumber, MaterialType, MarkdownContent?, FilePath?, OriginalFileName?, `List<RequirementIcon>` RequirementIcons, navigation props. Does NOT inherit BaseAuditableEntity (follows Course pattern as template data). **Actually reconsider**: Lessons are edited over time by supervisors so audit trail IS useful. → **Inherit BaseAuditableEntity**.
- File 2: `LessonAttendance.cs` — Guid Id, LessonId FK, GroupId FK, DateOnly Date, TimeOnly StartTime, string? Notes. Inherits BaseAuditableEntity.
- File 3: `LessonSuggestion.cs` — Guid Id, LessonId FK, TeacherId FK, Content, SelectedText?, SelectionStartIndex?, SelectionEndIndex?, IsResolved. Inherits BaseAuditableEntity.
- File 4: `SuggestionVote.cs` — composite PK (SuggestionId, TeacherId), IsUpvote bool. No BaseAuditableEntity (follows TeacherProgrammingLanguage pattern for simple join with payload).

**Step 1.4 — Modify existing entities**
- M1: `Course.cs` — add `public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();`
- M2: `Group.cs` — add `public ICollection<LessonAttendance> LessonAttendances { get; set; } = new List<LessonAttendance>();`
- M3: `Teacher.cs` — add `public ICollection<LessonSuggestion> Suggestions { get; set; } = new List<LessonSuggestion>();` and `public ICollection<SuggestionVote> SuggestionVotes { get; set; } = new List<SuggestionVote>();`

**Step 1.5 — Create domain events**
- File 7: `LessonCreatedEvent.cs` — `record LessonCreatedEvent(Lesson Lesson) : INotification`
- File 8: `LessonSuggestionCreatedEvent.cs` — `record LessonSuggestionCreatedEvent(LessonSuggestion Suggestion) : INotification`

### Phase 2: Application Layer — Interfaces & DTOs (Files 9–13)

**Step 2.1 — File storage interface**
- File 9: `IFileStorageService.cs`
  ```csharp
  namespace TeacherSuite.Application.Common.Interfaces;
  public interface IFileStorageService
  {
      Task<string> SaveFileAsync(string fileName, Stream content, CancellationToken ct);
      Task<(Stream Content, string FileName)> GetFileAsync(string filePath, CancellationToken ct);
      Task DeleteFileAsync(string filePath, CancellationToken ct);
      bool FileExists(string filePath);
  }
  ```
  Returns relative path from `SaveFileAsync`. The implementation handles base path.

**Step 2.2 — Modify IApplicationDbContext**
- M4: Add four new DbSet properties:
  ```csharp
  DbSet<Lesson> Lessons { get; }
  DbSet<LessonAttendance> LessonAttendances { get; }
  DbSet<LessonSuggestion> LessonSuggestions { get; }
  DbSet<SuggestionVote> SuggestionVotes { get; }
  ```

**Step 2.3 — Create DTOs**
- File 10: `LessonDto.cs` — id, courseId, courseName, title, description, durationMinutes, orderNumber, materialType, requirementIcons, hasFile. Embedded AutoMapper `Profile` mapping. Used for list views.
- File 11: `LessonDetailDto.cs` — extends LessonDto with markdownContent, originalFileName, suggestions list, attendances list. Used for detail view.
- File 12: `LessonAttendanceDto.cs` — id, lessonId, groupId, groupName, date, startTime, notes, created. Embedded mapping.
- File 13: `LessonSuggestionDto.cs` — id, lessonId, teacherId, teacherName, content, selectedText, selectionStartIndex, selectionEndIndex, isResolved, voteScore (computed: `Votes.Count(v => v.IsUpvote) - Votes.Count(v => !v.IsUpvote)`), currentUserVote (nullable bool), created. Embedded mapping.
- File 32: `SuggestionVoteDto.cs` — simple DTO if needed for response.

### Phase 3: Application Layer — Commands & Queries (Files 14–39)

**Step 3.1 — Validation rules**
- File 14: `LessonValidationRules.cs`
  - Title: NotEmpty, MaxLength(200)
  - DurationMinutes: GreaterThan(0), LessThanOrEqualTo(480)
  - OrderNumber: GreaterThan(0)
  - MaterialType: IsInEnum
  - When MaterialType = Markdown: MarkdownContent NotEmpty
  - When MaterialType = WordDocument: File stream not null (on create/update)

**Step 3.2 — Lesson CRUD commands**
- File 15: `CreateLesson.cs`
  - `record CreateLessonCommand(int CourseId, string Title, string? Description, int DurationMinutes, int OrderNumber, LessonMaterialType MaterialType, string? MarkdownContent, Stream? FileContent, string? OriginalFileName, List<RequirementIcon>? RequirementIcons) : IRequest<int>, ICacheInvalidationCommand`
  - Tags: `["lessons", $"course-{CourseId}-lessons"]`
  - Handler: validate course exists, auto-assign OrderNumber if 0 (max+1), save file if Word, create entity, publish event
  - File naming: sanitize course name + lesson order + original filename
  - `[Authorize(Roles = AppRoles.Policies.AdminOrSupervisor)]`
- File 16: `CreateLessonCommandValidator.cs` — uses LessonValidationRules

- File 17: `UpdateLesson.cs`
  - `record UpdateLessonCommand(int Id, string Title, string? Description, int DurationMinutes, int OrderNumber, List<RequirementIcon>? RequirementIcons) : IRequest<Unit>, ICacheInvalidationCommand`
  - Note: This only updates metadata, NOT material content (separate command)
  - `[Authorize(Roles = AppRoles.Policies.AdminOrSupervisor)]`
- File 18: `UpdateLessonCommandValidator.cs`

- File 19: `DeleteLesson.cs`
  - `record DeleteLessonCommand(int Id) : IRequest<Unit>, ICacheInvalidationCommand`
  - Handler: delete file from disk if Word, cascade delete attendances/suggestions in DB, reorder remaining lessons
  - `[Authorize(Roles = AppRoles.Policies.AdminOrSupervisor)]`

- File 20: `UpdateLessonMaterial.cs`
  - `record UpdateLessonMaterialCommand(int LessonId, LessonMaterialType MaterialType, string? MarkdownContent, Stream? FileContent, string? OriginalFileName) : IRequest<Unit>, ICacheInvalidationCommand`
  - Handler: if switching from Word → Markdown, delete old file; if switching Markdown → Word, clear markdown field; update accordingly
  - `[Authorize(Roles = AppRoles.Policies.AdminOrSupervisor)]`
- File 21: `UpdateLessonMaterialCommandValidator.cs`

- File 22: `ReorderLessons.cs`
  - `record ReorderLessonsCommand(int CourseId, List<int> LessonIdsInOrder) : IRequest<Unit>, ICacheInvalidationCommand`
  - Handler: validate all IDs belong to course, update OrderNumber for each
  - `[Authorize(Roles = AppRoles.Policies.AdminOrSupervisor)]`

**Step 3.3 — Lesson queries**
- File 23: `GetLessonsByCourseQuery.cs`
  - `record GetLessonsByCourseQuery(int CourseId, int? Page, int? PageSize) : IRequest<PagedResult<LessonDto>>, ICacheableQuery`
  - CacheKey: `$"teachersuite:course-{CourseId}-lessons:page:{Page}:size:{PageSize}"`
  - Tags: `[$"course-{CourseId}-lessons"]`
  - Handler: query lessons where CourseId matches, order by OrderNumber

- File 24: `GetLessonByIdQuery.cs`
  - `record GetLessonByIdQuery(int Id) : IRequest<LessonDetailDto?>`
  - Handler: include suggestions (with votes + teacher), attendances (with group). Map to LessonDetailDto.
  - For suggestions: compute vote score and current user's vote direction.

- File 25: `GetAllLessonsQuery.cs`
  - `record GetAllLessonsQuery(int? CourseId, int? Page, int? PageSize) : IRequest<PagedResult<LessonDto>>, ICacheableQuery`
  - For standalone lessons page. Optional CourseId filter.
  - CacheKey includes CourseId filter.

- File 26: `DownloadLessonFileQuery.cs`
  - `record DownloadLessonFileQuery(int LessonId) : IRequest<LessonFileResult>`
  - Returns: `record LessonFileResult(Stream Content, string FileName, string ContentType)`
  - Handler: validate lesson exists, has Word material, call IFileStorageService.GetFileAsync

**Step 3.4 — Event handlers**
- File 27: `LessonCreatedEventHandler.cs` — log creation (follows existing pattern)
- File 39: `LessonSuggestionCreatedEventHandler.cs` — log creation

**Step 3.5 — Attendance commands & queries**
- File 28: `RecordLessonAttendance.cs`
  - `record RecordLessonAttendanceCommand(int LessonId, Guid GroupId, DateOnly Date, TimeOnly StartTime, string? Notes) : IRequest<Guid>, ICacheInvalidationCommand`
  - Handler: validate lesson exists, validate group exists, check group is assigned to the lesson's course (via GroupCourses), check no duplicate (same lesson+group+date), create entity
  - Tags: `["lesson-attendances", $"group-{GroupId}-attendances"]`
  - `[Authorize]` (any authenticated user — teachers record attendance)
- File 29: `RecordLessonAttendanceCommandValidator.cs`

- File 30: `DeleteLessonAttendance.cs`
  - `record DeleteLessonAttendanceCommand(Guid Id) : IRequest<Unit>`
  - `[Authorize(Roles = AppRoles.Policies.AdminOrSupervisor)]`

- File 31: `GetAttendancesByGroupQuery.cs`
  - `record GetAttendancesByGroupQuery(Guid GroupId, int? CourseId) : IRequest<List<LessonAttendanceDto>>`
  - Returns all attendance records for a group, optionally filtered by course
  - Include lesson title for display

**Step 3.6 — Suggestion commands & queries**
- File 33: `CreateLessonSuggestion.cs`
  - `record CreateLessonSuggestionCommand(int LessonId, string Content, string? SelectedText, int? SelectionStartIndex, int? SelectionEndIndex) : IRequest<Guid>, ICacheInvalidationCommand`
  - Handler: validate lesson exists, get current user's TeacherId, create entity, publish event
  - `[Authorize]` (any teacher)
  - Context validation: if SelectedText provided, SelectionStartIndex and SelectionEndIndex must also be provided
- File 34: `CreateLessonSuggestionCommandValidator.cs`

- File 35: `DeleteLessonSuggestion.cs`
  - `record DeleteLessonSuggestionCommand(Guid Id) : IRequest<Unit>`
  - Handler: verify current user is the suggestion author OR is Admin/Supervisor
  - `[Authorize]`

- File 36: `ResolveLessonSuggestion.cs`
  - `record ResolveLessonSuggestionCommand(Guid Id) : IRequest<Unit>`
  - Handler: set IsResolved = true
  - `[Authorize(Roles = AppRoles.Policies.AdminOrSupervisor)]`

- File 37: `VoteOnSuggestion.cs`
  - `record VoteOnSuggestionCommand(Guid SuggestionId, bool IsUpvote) : IRequest<Unit>`
  - Handler: get current user's TeacherId, upsert vote (if exists, update; if not, create). Teachers cannot vote on own suggestions.
  - `[Authorize]`

- File 38: `GetSuggestionsByLessonQuery.cs`
  - `record GetSuggestionsByLessonQuery(int LessonId) : IRequest<List<LessonSuggestionDto>>`
  - Include votes, compute score, include current user's vote direction
  - Order by: unresolved first, then by score descending

**Step 3.7 — Modify course completion gating**
- M5: `UpdateGroupCourseStatus.cs`
  - Before allowing transition to `Completed`:
    ```csharp
    var course = await context.Courses
        .Include(c => c.Lessons)
        .FirstOrDefaultAsync(c => c.Id == entity.CourseId, cancellationToken);
    var lessonIds = course.Lessons.Select(l => l.Id).ToList();
    var attendedLessonIds = await context.LessonAttendances
        .Where(a => a.GroupId == entity.GroupId && lessonIds.Contains(a.LessonId))
        .Select(a => a.LessonId)
        .Distinct()
        .ToListAsync(cancellationToken);
    if (lessonIds.Count > 0 && attendedLessonIds.Count < lessonIds.Count)
    {
        var missing = lessonIds.Count - attendedLessonIds.Count;
        throw new ValidationException(...$"{missing} lesson(s) have not been attended yet.");
    }
    ```

- M6: `CourseDto.cs` — add `public int LessonCount { get; init; }` mapped from `src.Lessons.Count`

### Phase 4: Infrastructure Layer (Files 40–46)

**Step 4.1 — EF Configurations**
- File 40: `LessonConfiguration.cs`
  ```csharp
  builder.HasKey(l => l.Id);
  builder.Property(l => l.Title).HasMaxLength(200).IsRequired();
  builder.Property(l => l.Description).HasMaxLength(2000);
  builder.Property(l => l.DurationMinutes).HasDefaultValue(90);
  builder.HasIndex(l => new { l.CourseId, l.OrderNumber }).IsUnique();
  builder.HasOne(l => l.Course).WithMany(c => c.Lessons).HasForeignKey(l => l.CourseId).OnDelete(DeleteBehavior.Cascade);
  builder.OwnsMany(l => l.RequirementIcons, icon => { icon.ToJson(); });
  // OR: builder.Property(l => l.RequirementIcons).HasColumnType("jsonb");
  ```

- File 41: `LessonAttendanceConfiguration.cs`
  ```csharp
  builder.HasKey(la => la.Id);
  builder.HasIndex(la => new { la.LessonId, la.GroupId, la.Date }).IsUnique();
  builder.HasOne(la => la.Lesson).WithMany(l => l.Attendances).HasForeignKey(la => la.LessonId).OnDelete(DeleteBehavior.Cascade);
  builder.HasOne(la => la.Group).WithMany(g => g.LessonAttendances).HasForeignKey(la => la.GroupId).OnDelete(DeleteBehavior.Cascade);
  ```

- File 42: `LessonSuggestionConfiguration.cs`
  ```csharp
  builder.HasKey(ls => ls.Id);
  builder.Property(ls => ls.Content).HasMaxLength(2000).IsRequired();
  builder.HasOne(ls => ls.Lesson).WithMany(l => l.Suggestions).HasForeignKey(ls => ls.LessonId).OnDelete(DeleteBehavior.Cascade);
  builder.HasOne(ls => ls.Teacher).WithMany(t => t.Suggestions).HasForeignKey(ls => ls.TeacherId).OnDelete(DeleteBehavior.Cascade);
  ```

- File 43: `SuggestionVoteConfiguration.cs`
  ```csharp
  builder.HasKey(sv => new { sv.SuggestionId, sv.TeacherId });
  builder.HasOne(sv => sv.Suggestion).WithMany(s => s.Votes).HasForeignKey(sv => sv.SuggestionId).OnDelete(DeleteBehavior.Cascade);
  builder.HasOne(sv => sv.Teacher).WithMany(t => t.SuggestionVotes).HasForeignKey(sv => sv.TeacherId).OnDelete(DeleteBehavior.Cascade);
  ```

**Step 4.2 — File storage implementation**
- File 44: `FileStorageService.cs`
  ```csharp
  namespace TeacherSuite.Infrastructure.FileStorage;
  public class FileStorageService(IOptions<FileStorageOptions> options) : IFileStorageService
  {
      // SaveFileAsync: create directory if needed, write to BasePath/{relative}, return relative path
      // GetFileAsync: open FileStream from BasePath/{relative}, return stream + filename
      // DeleteFileAsync: File.Delete(BasePath/{relative})
      // FileExists: File.Exists(BasePath/{relative})
  }
  ```

- File 45: `FileStorageOptions.cs`
  ```csharp
  namespace TeacherSuite.Infrastructure.FileStorage;
  public class FileStorageOptions { public string BasePath { get; set; } = "./uploads"; }
  ```

**Step 4.3 — Modify ApplicationDbContext**
- M7: Add:
  ```csharp
  public DbSet<Lesson> Lessons => Set<Lesson>();
  public DbSet<LessonAttendance> LessonAttendances => Set<LessonAttendance>();
  public DbSet<LessonSuggestion> LessonSuggestions => Set<LessonSuggestion>();
  public DbSet<SuggestionVote> SuggestionVotes => Set<SuggestionVote>();
  ```

**Step 4.4 — Modify Infrastructure DI**
- M8: Add to `AddInfrastructureServices`:
  ```csharp
  services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
  services.AddScoped<IFileStorageService, FileStorageService>();
  ```

**Step 4.5 — Generate migration**
- File 46: Run `dotnet ef migrations add AddLessonsFeature` — auto-generated

### Phase 5: Web Layer — Backend Endpoints (Files 47–52)

**Step 5.1 — Lesson endpoints**
- File 47: `LessonEndpoints.cs`
  ```
  GET    /Courses/{courseId}/Lessons          → GetLessonsByCourse
  GET    /Lessons                             → GetAllLessons (standalone page)
  GET    /Lessons/{id}                        → GetLessonById
  POST   /Courses/{courseId}/Lessons          → CreateLesson (multipart/form-data)
  PUT    /Lessons/{id}                        → UpdateLesson (metadata only)
  PUT    /Lessons/{id}/Material               → UpdateLessonMaterial (multipart/form-data)
  DELETE /Lessons/{id}                        → DeleteLesson
  PUT    /Courses/{courseId}/Lessons/Reorder   → ReorderLessons
  GET    /Lessons/{id}/Download               → DownloadLessonFile
  ```

- File 48: `Lessons.cs` — handler methods
  - `CreateLesson` must handle `IFormFile` for Word uploads + JSON fields for metadata
  - `DownloadLessonFile` returns `Results<FileStreamHttpResult, NotFound>`
  - Use `TypedResults.File(stream, contentType, fileName)` for download

**Step 5.2 — Attendance endpoints**
- File 49: `LessonAttendanceEndpoints.cs`
  ```
  POST   /LessonAttendances                   → RecordAttendance
  DELETE /LessonAttendances/{id}              → DeleteAttendance
  GET    /Groups/{groupId}/Attendances         → GetAttendancesByGroup
  ```

- File 50: `LessonAttendances.cs` — handler methods

**Step 5.3 — Suggestion endpoints**
- File 51: `LessonSuggestionEndpoints.cs`
  ```
  GET    /Lessons/{lessonId}/Suggestions      → GetSuggestionsByLesson
  POST   /Lessons/{lessonId}/Suggestions      → CreateSuggestion
  DELETE /LessonSuggestions/{id}              → DeleteSuggestion
  PUT    /LessonSuggestions/{id}/Resolve      → ResolveSuggestion
  POST   /LessonSuggestions/{id}/Vote         → VoteOnSuggestion
  ```

- File 52: `LessonSuggestions.cs` — handler methods

**Step 5.4 — Register in Program.cs**
- M9: Add:
  ```csharp
  builder.Services.AddScoped<Lessons>();
  builder.Services.AddScoped<LessonAttendances>();
  builder.Services.AddScoped<LessonSuggestions>();
  // ...
  app.MapLessonEndpoints();
  app.MapLessonAttendanceEndpoints();
  app.MapLessonSuggestionEndpoints();
  ```

**Step 5.5 — Configuration files**
- M16: `appsettings.json` — add `"FileStorage": { "BasePath": "/data/lesson-files" }`
- M17: `appsettings.Development.json` — add `"FileStorage": { "BasePath": "./uploads" }`

### Phase 6: Angular Frontend (Files 53–67)

**Step 6.1 — Install dependencies**
- M15: Add to `package.json`:
  - `marked` (^15.x) — markdown → HTML rendering
  - `@types/dompurify` + `dompurify` (^3.x) — HTML sanitization for rendered markdown
  - `isomorphic-dompurify` if SSR is needed, otherwise standard `dompurify`

**Step 6.2 — Create services**
- File 53: `lesson.service.ts`
  ```typescript
  @Injectable({ providedIn: 'root' })
  export class LessonService extends ApiService {
    getLessonsByCourse(courseId: number, query): Observable<PagedResult<Lesson>>
    getAllLessons(query): Observable<PagedResult<Lesson>>
    getLessonById(id: number): Observable<LessonDetail>
    createLesson(courseId: number, formData: FormData): Observable<number>  // multipart
    updateLesson(id: number, data): Observable<void>
    updateLessonMaterial(id: number, formData: FormData): Observable<void>  // multipart
    deleteLesson(id: number): Observable<void>
    reorderLessons(courseId: number, lessonIds: number[]): Observable<void>
    downloadFile(lessonId: number): void  // direct window.open or fetch+blob
  }
  ```
  Note: The base `ApiService` only does JSON. For multipart uploads, add a `protected postFormData<T>(url, formData)` method to `ApiService` (or add it directly in `LessonService`). This method must set auth headers but NOT set Content-Type (browser sets it with boundary).

- File 54: `lesson-suggestion.service.ts`
  ```typescript
  getSuggestions(lessonId: number): Observable<LessonSuggestion[]>
  createSuggestion(lessonId: number, data): Observable<string>  // returns GUID
  deleteSuggestion(id: string): Observable<void>
  resolveSuggestion(id: string): Observable<void>
  vote(id: string, isUpvote: boolean): Observable<void>
  ```

- File 55: `lesson-attendance.service.ts`
  ```typescript
  getAttendances(groupId: string, courseId?: number): Observable<LessonAttendance[]>
  recordAttendance(data): Observable<string>
  deleteAttendance(id: string): Observable<void>
  ```

**Step 6.3 — Create shared components**
- File 62–64: `MarkdownViewerComponent`
  - Input: `markdownContent` (string), `suggestions` (array)
  - Renders markdown using `marked` library → sanitized HTML via DOMPurify
  - Highlights text ranges where suggestions exist (overlays with colored backgrounds)
  - Right-click context menu on text selection: "Add Suggestion" option
  - Emits `(suggestionRequest)` event with `{ selectedText, startIndex, endIndex }`

- File 65–67: `SuggestionPanelComponent`
  - Input: `suggestions` (array), `lessonId`, `currentUserRoles`
  - Displays list of suggestions with vote score, upvote/downvote buttons, resolve button
  - For markdown suggestions: clicking a suggestion scrolls to/highlights the referenced text
  - Emits `(vote)`, `(resolve)`, `(delete)` events

**Step 6.4 — Create pages**
- File 56–58: `LessonsComponent` (standalone lessons page)
  - Course dropdown picker (loads all courses)
  - Displays lesson cards for selected course, ordered by OrderNumber
  - Each card shows: order number, title, duration, requirement icons with tooltips, material type badge
  - Click card → navigate to lesson detail
  - Admin/Supervisor: "Add Lesson" button, drag-to-reorder support
  - Pagination via `PaginationBarComponent`

- File 59–61: `LessonDetailComponent`
  - Route: `/lessons/:id`
  - Header: lesson title, duration, requirement icons
  - Material section:
    - If Markdown: render via `MarkdownViewerComponent`
    - If Word: show file info card with download button
  - Suggestion panel on the right side (or below on mobile)
  - Attendance section: show which groups have attended, with date
  - Admin/Supervisor toolbar: Edit Lesson, Upload New Material, Delete Lesson

**Step 6.5 — Update routing & navigation**
- M10: `app.routes.ts` — add:
  ```typescript
  { path: 'lessons', component: Lessons, canActivate: [authGuard] },
  { path: 'lessons/:id', component: LessonDetail, canActivate: [authGuard] },
  ```

- M11: `app.html` — add nav item:
  ```html
  <li>
    <a routerLink="/lessons" routerLinkActive="active" class="nav-link">
      <ng-icon name="heroDocumentText" size="24" />
      <span>Lessons</span>
    </a>
  </li>
  ```
  Place after "Courses" in the sidebar.

- M12: `app.ts` — import `heroDocumentText` from `@ng-icons/heroicons/outline` and add to `provideIcons()`

- M13/M14: `courses.html`/`courses.ts` — add "View Lessons" button to course cards that navigates to `/lessons?courseId={id}`

### Phase 7: Tests (Files 68–73)

**Step 7.1 — Query handler tests**
- File 68: `GetLessonsByCourseQueryTests.cs`
  - Test: returns paginated results ordered by OrderNumber
  - Test: empty result for non-existent course
  - Test: pagination normalization
  - Uses `CreateMockDbSet<Lesson>` pattern from existing tests

- File 69: `GetLessonByIdQueryTests.cs`
  - Test: returns full detail with suggestions and attendances
  - Test: returns null for non-existent lesson

**Step 7.2 — Command handler tests**
- File 70: `RecordLessonAttendanceTests.cs`
  - Test: creates attendance record
  - Test: throws ConflictException for duplicate (same lesson + group + date)
  - Test: throws NotFoundException for non-existent lesson
  - Test: throws ConflictException when group is not assigned to lesson's course

- File 71: `CreateLessonSuggestionTests.cs`
  - Test: creates suggestion successfully
  - Test: validates content is not empty
  - Test: context fields are optional

- File 72: `VoteOnSuggestionTests.cs`
  - Test: creates new vote
  - Test: updates existing vote (change upvote to downvote)
  - Test: prevents voting on own suggestion

- File 73: `CourseCompletionGatingTests.cs`
  - Test: allows completion when all lessons attended
  - Test: blocks completion when some lessons not attended
  - Test: allows completion when course has zero lessons

---

## Edge Cases to Handle

| # | Edge Case | How to Handle |
|---|---|---|
| 1 | Course has zero lessons → can still be marked Completed | Gate check: `if (lessonIds.Count > 0 && ...)` — skip check when no lessons |
| 2 | Deleting a lesson that has attendance records | Cascade delete in DB config; consider soft-delete alternative in future |
| 3 | Reordering leaves gaps in OrderNumber | `ReorderLessons` handler assigns contiguous numbers 1..N |
| 4 | File upload > reasonable size | Add `IFormFile` size validation in validator (e.g., max 50MB); configure Kestrel `MaxRequestBodySize` |
| 5 | Malicious filename in upload | Sanitize filename: remove path separators, special chars; use GUID prefix for uniqueness |
| 6 | Markdown content with XSS | Sanitize on frontend with DOMPurify before rendering; backend stores raw markdown |
| 7 | Concurrent lesson reordering | Unique index on `(CourseId, OrderNumber)` prevents duplicate order numbers; transaction wraps reorder |
| 8 | Teacher votes on own suggestion | Handler checks `suggestion.TeacherId == currentUser.TeacherId` → throw ForbiddenAccessException |
| 9 | Suggestion references text that was later edited | Store `SelectedText` as a snapshot; UI shows "context may have changed" warning if text no longer matches |
| 10 | File deleted from disk but path still in DB | `DownloadLessonFileQuery` handler: check `IFileStorageService.FileExists()` → throw NotFoundException if missing |
| 11 | Multiple groups attend same lesson on same date | Allowed — unique index is `(LessonId, GroupId, Date)`, different groups can attend same lesson on same date |
| 12 | Same group attends same lesson on different dates | Allowed — represents repeat attendance (e.g., makeup class) |
| 13 | Word file replaced via UpdateMaterial | Delete old file from disk before saving new one |
| 14 | Long markdown content | No hard limit in DB (text column); consider frontend lazy-loading or pagination for very long content |
| 15 | File storage directory doesn't exist | `FileStorageService.SaveFileAsync` creates directory recursively on first use |

---

## API Endpoints Summary

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/Courses/{courseId}/Lessons` | Any auth | List lessons for a course |
| GET | `/Lessons` | Any auth | List all lessons (standalone page, optional courseId filter) |
| GET | `/Lessons/{id}` | Any auth | Get lesson detail with material, suggestions, attendances |
| POST | `/Courses/{courseId}/Lessons` | Supervisor+ | Create lesson (multipart for file upload) |
| PUT | `/Lessons/{id}` | Supervisor+ | Update lesson metadata |
| PUT | `/Lessons/{id}/Material` | Supervisor+ | Update lesson material (multipart) |
| DELETE | `/Lessons/{id}` | Supervisor+ | Delete lesson |
| PUT | `/Courses/{courseId}/Lessons/Reorder` | Supervisor+ | Reorder lessons |
| GET | `/Lessons/{id}/Download` | Any auth | Download Word file |
| POST | `/LessonAttendances` | Any auth | Record attendance |
| DELETE | `/LessonAttendances/{id}` | Supervisor+ | Delete attendance |
| GET | `/Groups/{groupId}/Attendances` | Any auth | Get group's attendance records |
| GET | `/Lessons/{lessonId}/Suggestions` | Any auth | List suggestions for a lesson |
| POST | `/Lessons/{lessonId}/Suggestions` | Any auth | Create suggestion |
| DELETE | `/LessonSuggestions/{id}` | Author or Supervisor+ | Delete suggestion |
| PUT | `/LessonSuggestions/{id}/Resolve` | Supervisor+ | Mark suggestion resolved |
| POST | `/LessonSuggestions/{id}/Vote` | Any auth | Vote on suggestion |

---

## Open Questions

1. **Should lessons support versioning?** If a supervisor edits markdown content, should previous versions be preserved? Current plan overwrites in place. A `LessonMaterialVersion` table could be added later.

2. **Should attendance track individual students or just the group?** Current plan tracks group-level attendance. Student-level attendance would require a `StudentLessonAttendance` join table — significantly more complexity.

3. **Should there be a lesson "status" (Draft / Published)?** The current plan makes all lessons immediately visible. A draft status would let supervisors prepare lessons before teachers see them.

4. **File size limits for Word uploads?** Suggest 50MB max but this should be confirmed with stakeholders. Need to configure Kestrel's `MaxRequestBodySize` accordingly.

5. **Should the `ApiService` base class be modified to support `FormData` uploads?** Or should `LessonService` implement its own upload method? Recommend adding a `protected postFormData<T>(url, formData)` to `ApiService` since file uploads may be needed elsewhere in the future.

6. **Markdown editor for supervisors?** The plan covers viewing and commenting on markdown. Should supervisors have a rich markdown editor inline, or is editing via a textarea sufficient? A WYSIWYG editor (e.g., ngx-markdown-editor) would enhance UX but adds dependency weight.

7. **Should suggestion notifications be sent?** When a teacher creates a suggestion, should supervisors receive a notification? Not in scope for this plan but could use the existing domain events infrastructure to add later.
