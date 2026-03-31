import { Component, OnInit, ChangeDetectorRef, HostListener, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import {
  LessonService,
  LessonDetail,
  LessonSuggestion,
  LessonFile,
  CreateSuggestionDto,
  RecordAttendanceDto,
  VoteDto,
  CourseGroup,
} from '../../services/lesson.service';
import { KeycloakService } from '../../auth/keycloak.service';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { marked } from 'marked';

@Component({
  selector: 'app-lesson-detail',
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './lesson-detail.html',
  styleUrl: './lesson-detail.scss',
})
export class LessonDetailPage implements OnInit {
  private destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  lesson: LessonDetail | null = null;
  loading = false;
  error: string | null = null;

  lessonFiles: LessonFile[] = [];
  rawMarkdownContent: string | null = null;
  markdownContent: SafeHtml | null = null;
  downloadableFiles: LessonFile[] = [];

  showContextMenu = false;
  contextMenuX = 0;
  contextMenuY = 0;
  contextSelectedText = '';
  contextSelectionStart: number | undefined;
  contextSelectionEnd: number | undefined;
  contextSuggestionContent = '';

  showCommentModal = false;
  commentContent = '';
  commentError: string | null = null;

  sortedSuggestions: LessonSuggestion[] = [];

  showAttendanceModal = false;
  attendanceForm: FormGroup;
  attendanceError: string | null = null;
  courseGroups: CourseGroup[] = [];
  courseGroupsLoaded = false;

  constructor(
    private lessonService: LessonService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder,
    private keycloakService: KeycloakService,
    private sanitizer: DomSanitizer
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

  loadLesson(id: number) {
    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();

    this.lessonService.getLessonById(id).subscribe({
      next: (lesson) => {
        this.lesson = lesson;
        this.sortSuggestions();
        this.loading = false;
        this.loadLessonFiles(id);
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

  loadLessonFiles(lessonId: number) {
    this.lessonService.getLessonFiles(lessonId).subscribe({
      next: (files) => {
        this.lessonFiles = files;
        this.downloadableFiles = files.filter(f => {
          const lower = f.name.toLowerCase();
          return lower.endsWith('.docx') || lower.endsWith('.txt');
        });

        const mdFile = files.find(f => f.name.toLowerCase().endsWith('.md'));
        if (mdFile && this.lesson) {
          this.lessonService.downloadMaterialAsText(this.lesson.id, mdFile.uuid).then(text => {
            this.rawMarkdownContent = text;
            const html = marked.parse(text);
            if (typeof html === 'string') {
              this.markdownContent = this.sanitizer.bypassSecurityTrustHtml(html);
            } else {
              (html as Promise<string>).then(h => {
                this.markdownContent = this.sanitizer.bypassSecurityTrustHtml(h);
                this.cdr.detectChanges();
              });
            }
            this.cdr.detectChanges();
          }).catch(() => {
            this.rawMarkdownContent = null;
            this.markdownContent = null;
          });
        } else {
          this.rawMarkdownContent = null;
          this.markdownContent = null;
        }
        this.cdr.detectChanges();
      },
      error: () => {
        this.lessonFiles = [];
        this.downloadableFiles = [];
        this.markdownContent = null;
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

  goBackToLessons() {
    if (this.lesson) {
      this.router.navigate(['/lessons'], { queryParams: { courseId: this.lesson.courseId } });
    } else {
      this.router.navigate(['/lessons']);
    }
  }

  canManage(): boolean {
    return this.keycloakService.hasRole('Admin') || this.keycloakService.hasRole('Supervisor');
  }

  hasMarkdown(): boolean {
    return this.markdownContent !== null;
  }

  hasDownloadableFiles(): boolean {
    return this.downloadableFiles.length > 0;
  }

  hasAnyFiles(): boolean {
    return this.lessonFiles.length > 0;
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

  downloadFile(file: LessonFile) {
    if (!this.lesson) return;
    this.lessonService.downloadMaterial(this.lesson.id, file.uuid).subscribe({
      error: (err) => {
        console.error('Error downloading material:', err);
        this.error = 'Failed to download material.';
        this.cdr.detectChanges();
      },
    });
  }

  onMarkdownContextMenu(event: MouseEvent) {
    event.preventDefault();
    const selection = window.getSelection();
    if (selection && selection.toString().trim().length > 0) {
      this.contextSelectedText = selection.toString().trim();

      const content = this.rawMarkdownContent ?? '';
      const selectedStr = this.contextSelectedText;
      const startIdx = content.indexOf(selectedStr);
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

  openAttendanceModal() {
    this.attendanceError = null;
    this.attendanceForm.reset({ groupId: '', attendedAt: '' });
    this.showAttendanceModal = true;
    this.courseGroupsLoaded = false;
    this.courseGroups = [];

    if (this.lesson) {
      this.lessonService.getCourseGroups(this.lesson.courseId).subscribe({
        next: (groups) => {
          this.courseGroups = groups;
          this.courseGroupsLoaded = true;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error loading course groups:', err);
          this.attendanceError = 'Failed to load groups for this course.';
          this.courseGroupsLoaded = true;
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
        this.attendanceError = err?.detail || 'Failed to record attendance. Please try again.';
        console.error('Error recording attendance:', err);
        this.cdr.detectChanges();
      },
    });
  }

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
