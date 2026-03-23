import { Component, OnInit, ChangeDetectorRef, HostListener, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import {
  LessonService,
  LessonDetail,
  LessonSuggestion,
  LessonAttendance,
  CreateSuggestionDto,
  RecordAttendanceDto,
  VoteDto,
} from '../../services/lesson.service';
import { GroupService, Group } from '../../services/group.service';
import { KeycloakService } from '../../auth/keycloak.service';

/** Material type enum values matching the backend */
const MaterialType = { None: 0, Markdown: 1, Word: 2 } as const;

/** Requirement icon definitions */
const REQUIREMENT_ICON_DEFS: { key: string; emoji: string; label: string }[] = [
  { key: 'phone', emoji: '📱', label: 'Mobile phone needed' },
  { key: 'laptop', emoji: '💻', label: 'Laptop/computer needed' },
  { key: 'headphones', emoji: '🎧', label: 'Headphones needed' },
  { key: 'book', emoji: '📖', label: 'Textbook needed' },
  { key: 'calculator', emoji: '🔢', label: 'Calculator needed' },
  { key: 'scissors', emoji: '✂️', label: 'Scissors needed' },
  { key: 'pen', emoji: '✏️', label: 'Pen/pencil needed' },
];

@Component({
  selector: 'app-lesson-detail',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './lesson-detail.html',
  styleUrl: './lesson-detail.scss',
})
export class LessonDetailPage implements OnInit {
  private destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  // Lesson data
  lesson: LessonDetail | null = null;
  loading = false;
  error: string | null = null;

  // Context menu (right-click on markdown)
  showContextMenu = false;
  contextMenuX = 0;
  contextMenuY = 0;
  contextSelectedText = '';
  contextSelectionStart: number | undefined;
  contextSelectionEnd: number | undefined;
  contextSuggestionContent = '';

  // Add comment modal (for Word docs or general comments)
  showCommentModal = false;
  commentContent = '';
  commentError: string | null = null;

  // Suggestions
  sortedSuggestions: LessonSuggestion[] = [];

  // Attendance
  showAttendanceModal = false;
  attendanceForm: FormGroup;
  attendanceError: string | null = null;
  groups: Group[] = [];
  groupsLoaded = false;

  constructor(
    private lessonService: LessonService,
    private groupService: GroupService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder,
    private keycloakService: KeycloakService
  ) {
    this.attendanceForm = this.fb.group({
      groupId: ['', [Validators.required]],
      attendedAt: ['', [Validators.required]],
    });
  }

  ngOnInit() {
    this.route.params
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const id = parseInt(params['id'], 10);
        if (Number.isFinite(id)) {
          this.loadLesson(id);
        }
      });
  }

  // --- Data loading ---

  loadLesson(id: number) {
    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();

    this.lessonService.getLessonById(id).subscribe({
      next: (lesson) => {
        this.lesson = lesson;
        this.sortSuggestions();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = 'Failed to load lesson details. Please try again.';
        this.loading = false;
        console.error('Error loading lesson:', err);
        this.cdr.detectChanges();
      },
    });
  }

  refreshSuggestions() {
    if (!this.lesson) return;
    this.lessonService.getSuggestions(this.lesson.id).subscribe({
      next: (suggestions) => {
        this.lesson!.suggestions = suggestions;
        this.sortSuggestions();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error refreshing suggestions:', err);
      },
    });
  }

  refreshAttendances() {
    if (!this.lesson) return;
    this.lessonService.getAttendances(this.lesson.id).subscribe({
      next: (attendances) => {
        this.lesson!.attendances = attendances;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error refreshing attendances:', err);
      },
    });
  }

  private sortSuggestions() {
    if (!this.lesson) return;
    this.sortedSuggestions = [...this.lesson.suggestions].sort(
      (a, b) => (b.upvoteCount - b.downvoteCount) - (a.upvoteCount - a.downvoteCount)
    );
  }

  // --- Navigation ---

  goBackToLessons() {
    if (this.lesson) {
      this.router.navigate(['/lessons'], { queryParams: { courseId: this.lesson.courseId } });
    } else {
      this.router.navigate(['/lessons']);
    }
  }

  // --- Display helpers ---

  canManage(): boolean {
    return this.keycloakService.hasRole('Admin') || this.keycloakService.hasRole('Supervisor');
  }

  isMarkdown(): boolean {
    return this.lesson?.materialType === MaterialType.Markdown;
  }

  isWord(): boolean {
    return this.lesson?.materialType === MaterialType.Word;
  }

  isNone(): boolean {
    return this.lesson?.materialType === MaterialType.None;
  }

  getRequirementEmoji(key: string): string {
    return REQUIREMENT_ICON_DEFS.find(d => d.key === key)?.emoji ?? key;
  }

  getRequirementTooltip(key: string): string {
    return REQUIREMENT_ICON_DEFS.find(d => d.key === key)?.label ?? key;
  }

  formatDate(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  formatDateTime(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-GB', {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit',
    });
  }

  // --- Material download ---

  downloadMaterial() {
    if (!this.lesson) return;
    this.lessonService.downloadMaterial(this.lesson.id).subscribe({
      error: (err) => {
        console.error('Error downloading material:', err);
        this.error = 'Failed to download material.';
        this.cdr.detectChanges();
      },
    });
  }

  // --- Context menu (right-click on markdown) ---

  onMarkdownContextMenu(event: MouseEvent) {
    event.preventDefault();

    // Capture any selected text from the pre element
    const selection = window.getSelection();
    if (selection && selection.toString().trim().length > 0) {
      this.contextSelectedText = selection.toString().trim();
      // Try to compute selection offsets relative to markdown content
      const markdownContent = this.lesson?.markdownContent ?? '';
      const selectedStr = this.contextSelectedText;
      const startIdx = markdownContent.indexOf(selectedStr);
      if (startIdx >= 0) {
        this.contextSelectionStart = startIdx;
        this.contextSelectionEnd = startIdx + selectedStr.length;
      } else {
        this.contextSelectionStart = undefined;
        this.contextSelectionEnd = undefined;
      }
    } else {
      this.contextSelectedText = '';
      this.contextSelectionStart = undefined;
      this.contextSelectionEnd = undefined;
    }

    this.contextSuggestionContent = '';
    this.contextMenuX = event.clientX;
    this.contextMenuY = event.clientY;
    this.showContextMenu = true;
    this.cdr.detectChanges();
  }

  closeContextMenu() {
    this.showContextMenu = false;
    this.contextSuggestionContent = '';
  }

  submitContextSuggestion() {
    if (!this.lesson || !this.contextSuggestionContent.trim()) return;

    const dto: CreateSuggestionDto = {
      content: this.contextSuggestionContent.trim(),
      selectedText: this.contextSelectedText || undefined,
      selectionStart: this.contextSelectionStart,
      selectionEnd: this.contextSelectionEnd,
    };

    this.lessonService.createSuggestion(this.lesson.id, dto).subscribe({
      next: () => {
        this.closeContextMenu();
        this.refreshSuggestions();
      },
      error: (err) => {
        console.error('Error creating suggestion:', err);
      },
    });
  }

  // --- Add comment modal ---

  openCommentModal() {
    this.commentContent = '';
    this.commentError = null;
    this.showCommentModal = true;
  }

  closeCommentModal() {
    this.showCommentModal = false;
    this.commentContent = '';
    this.commentError = null;
  }

  submitComment() {
    if (!this.lesson || !this.commentContent.trim()) {
      this.commentError = 'Please enter a comment.';
      return;
    }

    const dto: CreateSuggestionDto = {
      content: this.commentContent.trim(),
    };

    this.lessonService.createSuggestion(this.lesson.id, dto).subscribe({
      next: () => {
        this.closeCommentModal();
        this.refreshSuggestions();
      },
      error: (err) => {
        this.commentError = 'Failed to submit comment. Please try again.';
        console.error('Error creating comment:', err);
        this.cdr.detectChanges();
      },
    });
  }

  // --- Suggestion voting ---

  voteSuggestion(suggestion: LessonSuggestion, vote: number) {
    const dto: VoteDto = { vote };
    this.lessonService.voteSuggestion(suggestion.id, dto).subscribe({
      next: () => {
        this.refreshSuggestions();
      },
      error: (err) => {
        console.error('Error voting on suggestion:', err);
      },
    });
  }

  deleteSuggestion(suggestion: LessonSuggestion) {
    this.lessonService.deleteSuggestion(suggestion.id).subscribe({
      next: () => {
        this.refreshSuggestions();
      },
      error: (err) => {
        console.error('Error deleting suggestion:', err);
      },
    });
  }

  canDeleteSuggestion(suggestion: LessonSuggestion): boolean {
    if (this.canManage()) return true;
    const currentUser = this.keycloakService.getUsername();
    return suggestion.teacherId === currentUser;
  }

  getVoteScore(suggestion: LessonSuggestion): number {
    return suggestion.upvoteCount - suggestion.downvoteCount;
  }

  // --- Attendance ---

  openAttendanceModal() {
    this.attendanceError = null;
    this.attendanceForm.reset({ groupId: '', attendedAt: '' });
    this.showAttendanceModal = true;

    if (!this.groupsLoaded) {
      this.groupService.getAllGroups().subscribe({
        next: (groups) => {
          this.groups = groups;
          this.groupsLoaded = true;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error loading groups:', err);
          this.attendanceError = 'Failed to load groups.';
          this.cdr.detectChanges();
        },
      });
    }
  }

  closeAttendanceModal() {
    this.showAttendanceModal = false;
    this.attendanceError = null;
  }

  submitAttendance() {
    if (!this.lesson) return;
    this.attendanceError = null;

    if (this.attendanceForm.invalid) {
      this.attendanceForm.markAllAsTouched();
      this.attendanceError = 'Please select a group and date.';
      return;
    }

    const formValue = this.attendanceForm.getRawValue();
    const dto: RecordAttendanceDto = {
      groupId: formValue.groupId,
      attendedAt: new Date(formValue.attendedAt).toISOString(),
    };

    this.lessonService.recordAttendance(this.lesson.id, dto).subscribe({
      next: () => {
        this.closeAttendanceModal();
        this.refreshAttendances();
      },
      error: (err) => {
        this.attendanceError = 'Failed to record attendance. Please try again.';
        console.error('Error recording attendance:', err);
        this.cdr.detectChanges();
      },
    });
  }

  // --- Keyboard ---

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(_event: Event) {
    if (this.showContextMenu) {
      this.closeContextMenu();
    }
    if (this.showCommentModal) {
      this.closeCommentModal();
    }
    if (this.showAttendanceModal) {
      this.closeAttendanceModal();
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(_event: Event) {
    if (this.showContextMenu) {
      this.closeContextMenu();
    }
  }
}
