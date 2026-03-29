import { Component, OnInit, ChangeDetectorRef, HostListener, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { LessonService, Lesson, CreateLessonDto, UpdateLessonDto } from '../../services/lesson.service';
import { CourseService, Course } from '../../services/course.service';
import { KeycloakService } from '../../auth/keycloak.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  heroClipboardDocumentList,
  heroPlus,
  heroPencil,
  heroTrash,
  heroDocumentText,
  heroDocument,
  heroClock,
  heroArrowUpTray,
  heroArrowUp,
  heroArrowDown,
} from '@ng-icons/heroicons/outline';

/** Material type enum values matching the backend */
const MaterialType = { None: 0, Markdown: 1, Word: 2 } as const;

/** Requirement icon definitions with emoji fallbacks and tooltip labels */
const REQUIREMENT_ICON_DEFS: { key: string; emoji: string; label: string }[] = [
  { key: 'phone', emoji: '📱', label: 'Mobile phone needed' },
  { key: 'laptop', emoji: '💻', label: 'Laptop/computer needed' },
  { key: 'arduino', emoji: '🔌', label: 'Arduino board needed' },
];

@Component({
  selector: 'app-lessons',
  imports: [CommonModule, ReactiveFormsModule, NgIconComponent],
  providers: [
    provideIcons({
      heroClipboardDocumentList,
      heroPlus,
      heroPencil,
      heroTrash,
      heroDocumentText,
      heroDocument,
      heroClock,
      heroArrowUpTray,
      heroArrowUp,
      heroArrowDown,
    }),
  ],
  templateUrl: './lessons.html',
  styleUrl: './lessons.scss',
})
export class LessonsPage implements OnInit {
  private destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  // Course selector
  courses: Course[] = [];
  selectedCourseId: number | null = null;
  coursesLoading = false;

  // Lessons list
  lessons: Lesson[] = [];
  loading = false;
  error: string | null = null;

  // Add/Edit modal
  showModal = false;
  isEditMode = false;
  currentLessonId: number | null = null;
  modalError: string | null = null;
  selectedRequirementIcons: string[] = [];
  lessonForm: FormGroup;

  // Delete confirmation modal
  showDeleteConfirm = false;
  lessonToDelete: Lesson | null = null;

  // File upload
  uploadingLessonId: number | null = null;

  // Requirement icon definitions (exposed to template)
  readonly requirementIconDefs = REQUIREMENT_ICON_DEFS;

  constructor(
    private lessonService: LessonService,
    private courseService: CourseService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder,
    private keycloakService: KeycloakService
  ) {
    this.lessonForm = this.fb.group({
      title: ['', [Validators.required]],
      description: [''],
      durationMinutes: [90, [Validators.required, Validators.min(1)]],
      materialType: [MaterialType.None, [Validators.required]],
      markdownContent: [''],
    });
  }

  ngOnInit() {
    this.loadCourses();

    this.route.queryParams
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const courseId = parseInt(params['courseId'], 10);
        if (Number.isFinite(courseId) && courseId !== this.selectedCourseId) {
          this.selectedCourseId = courseId;
          this.loadLessons();
        }
      });
  }

  // --- Role helpers ---

  canManage(): boolean {
    return this.keycloakService.hasRole('Admin') || this.keycloakService.hasRole('Supervisor');
  }

  // --- Course selector ---

  loadCourses() {
    this.coursesLoading = true;
    this.courseService.getAllCourses({ page: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.courses = result.items;
        this.coursesLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading courses:', err);
        this.error = 'Failed to load courses.';
        this.coursesLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  onCourseChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    const courseId = value ? parseInt(value, 10) : null;
    this.selectedCourseId = courseId;

    // Update URL query param
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: courseId ? { courseId: String(courseId) } : {},
    });

    if (courseId) {
      this.loadLessons();
    } else {
      this.lessons = [];
    }
  }

  // --- Lessons list ---

  loadLessons() {
    if (!this.selectedCourseId) return;

    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();

    this.lessonService.getLessonsByCourse(this.selectedCourseId).subscribe({
      next: (lessons) => {
        this.lessons = lessons.sort((a, b) => a.order - b.order);
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = 'Failed to load lessons. Please try again.';
        this.loading = false;
        console.error('Error loading lessons:', err);
        this.cdr.detectChanges();
      },
    });
  }

  navigateToDetail(lesson: Lesson) {
    this.router.navigate(['/lessons', lesson.id]);
  }

  // --- Display helpers ---

  getMaterialTypeLabel(materialType: number): string {
    switch (materialType) {
      case MaterialType.Markdown: return 'Markdown';
      case MaterialType.Word: return 'Word';
      default: return 'None';
    }
  }

  getMaterialTypeIcon(materialType: number): string {
    switch (materialType) {
      case MaterialType.Markdown: return 'heroDocumentText';
      case MaterialType.Word: return 'heroDocument';
      default: return '';
    }
  }

  getRequirementEmoji(key: string): string {
    return REQUIREMENT_ICON_DEFS.find(d => d.key === key)?.emoji ?? key;
  }

  getRequirementTooltip(key: string): string {
    return REQUIREMENT_ICON_DEFS.find(d => d.key === key)?.label ?? key;
  }

  isMarkdownType(): boolean {
    return this.lessonForm.get('materialType')?.value === MaterialType.Markdown;
  }

  isWordMaterial(lesson: Lesson): boolean {
    return lesson.materialType === MaterialType.Word;
  }

  // --- Add / Edit modal ---

  openAddModal() {
    this.isEditMode = false;
    this.currentLessonId = null;
    this.modalError = null;
    this.selectedRequirementIcons = [];

    this.lessonForm.reset({
      title: '',
      description: '',
      durationMinutes: 90,
      materialType: MaterialType.None,
      markdownContent: '',
    });
    this.showModal = true;
  }

  openEditModal(lesson: Lesson) {
    this.isEditMode = true;
    this.currentLessonId = lesson.id;
    this.modalError = null;
    this.selectedRequirementIcons = [...(lesson.requirementIcons || [])];

    // Load full detail to get markdownContent
    this.lessonService.getLessonById(lesson.id).subscribe({
      next: (detail) => {
        this.lessonForm.reset({
          title: detail.title,
          description: detail.description || '',
          durationMinutes: detail.durationMinutes,
          materialType: detail.materialType,
          markdownContent: detail.markdownContent || '',
        });
        this.showModal = true;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading lesson detail:', err);
        this.error = 'Failed to load lesson details for editing.';
        this.cdr.detectChanges();
      },
    });
  }

  closeModal() {
    this.showModal = false;
    this.isEditMode = false;
    this.currentLessonId = null;
    this.modalError = null;
    this.selectedRequirementIcons = [];
  }

  toggleRequirementIcon(key: string) {
    const index = this.selectedRequirementIcons.indexOf(key);
    if (index >= 0) {
      this.selectedRequirementIcons.splice(index, 1);
    } else {
      this.selectedRequirementIcons.push(key);
    }
  }

  isRequirementSelected(key: string): boolean {
    return this.selectedRequirementIcons.includes(key);
  }

  saveLesson() {
    this.modalError = null;

    if (this.lessonForm.invalid) {
      this.lessonForm.markAllAsTouched();
      this.modalError = this.getFormErrorMessage();
      this.cdr.detectChanges();
      return;
    }

    const formValue = this.lessonForm.getRawValue();

    if (this.isEditMode && this.currentLessonId !== null) {
      const payload: UpdateLessonDto = {
        title: formValue.title,
        description: formValue.description || undefined,
        durationMinutes: formValue.durationMinutes,
        materialType: formValue.materialType,
        markdownContent: formValue.materialType === MaterialType.Markdown ? formValue.markdownContent : undefined,
        requirementIcons: this.selectedRequirementIcons,
      };

      this.lessonService.updateLesson(this.currentLessonId, payload).subscribe({
        next: () => {
          this.loadLessons();
          this.closeModal();
        },
        error: (err) => {
          this.modalError = 'Failed to update lesson. Please check your input and try again.';
          console.error('Error updating lesson:', err);
          this.cdr.detectChanges();
        },
      });
    } else {
      const payload: CreateLessonDto = {
        courseId: this.selectedCourseId!,
        title: formValue.title,
        description: formValue.description || undefined,
        durationMinutes: formValue.durationMinutes,
        materialType: formValue.materialType,
        markdownContent: formValue.materialType === MaterialType.Markdown ? formValue.markdownContent : undefined,
        requirementIcons: this.selectedRequirementIcons,
      };

      this.lessonService.createLesson(payload).subscribe({
        next: () => {
          this.loadLessons();
          this.closeModal();
        },
        error: (err) => {
          this.modalError = 'Failed to create lesson. Please check your input and try again.';
          console.error('Error creating lesson:', err);
          this.cdr.detectChanges();
        },
      });
    }
  }

  // --- Delete ---

  confirmDelete(lesson: Lesson) {
    this.lessonToDelete = lesson;
    this.showDeleteConfirm = true;
  }

  cancelDelete() {
    this.lessonToDelete = null;
    this.showDeleteConfirm = false;
  }

  deleteLesson() {
    if (this.lessonToDelete) {
      this.lessonService.deleteLesson(this.lessonToDelete.id).subscribe({
        next: () => {
          this.loadLessons();
          this.cancelDelete();
        },
        error: (err) => {
          this.error = 'Failed to delete lesson. Please try again.';
          console.error('Error deleting lesson:', err);
          this.cancelDelete();
        },
      });
    }
  }

  // --- File upload ---

  onFileSelected(event: Event, lesson: Lesson) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploadingLessonId = lesson.id;
    this.cdr.detectChanges();

    this.lessonService.uploadMaterial(lesson.id, file).subscribe({
      next: () => {
        this.uploadingLessonId = null;
        this.loadLessons();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.uploadingLessonId = null;
        this.error = 'Failed to upload material file.';
        console.error('Error uploading material:', err);
        this.cdr.detectChanges();
      },
    });

    // Reset input so the same file can be re-selected
    input.value = '';
  }

  // --- Keyboard ---

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(_event: Event) {
    if (this.showModal) {
      this.closeModal();
    }
    if (this.showDeleteConfirm) {
      this.cancelDelete();
    }
  }

  // --- Reorder ---

  moveLessonUp(lesson: Lesson) {
    this.lessonService.reorderLesson(lesson.id, 'up').subscribe({
      next: () => this.loadLessons(),
      error: (err) => {
        this.error = 'Failed to reorder lesson.';
        console.error('Error reordering lesson:', err);
        this.cdr.detectChanges();
      },
    });
  }

  moveLessonDown(lesson: Lesson) {
    this.lessonService.reorderLesson(lesson.id, 'down').subscribe({
      next: () => this.loadLessons(),
      error: (err) => {
        this.error = 'Failed to reorder lesson.';
        console.error('Error reordering lesson:', err);
        this.cdr.detectChanges();
      },
    });
  }

  isFirstLesson(lesson: Lesson): boolean {
    return this.lessons.length > 0 && this.lessons[0].id === lesson.id;
  }

  isLastLesson(lesson: Lesson): boolean {
    return this.lessons.length > 0 && this.lessons[this.lessons.length - 1].id === lesson.id;
  }

  // --- Form validation helper ---

  private getFormErrorMessage(): string | null {
    const controls = this.lessonForm.controls;

    if (controls['title']?.errors?.['required']) {
      return 'Lesson title is required';
    }
    if (controls['durationMinutes']?.errors?.['required']) {
      return 'Duration is required';
    }
    if (controls['durationMinutes']?.errors?.['min']) {
      return 'Duration must be at least 1 minute';
    }

    return 'Please fix the errors in the form.';
  }
}
