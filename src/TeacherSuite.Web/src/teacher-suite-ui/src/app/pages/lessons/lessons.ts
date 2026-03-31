import { Component, OnInit, ChangeDetectorRef, HostListener, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { concat } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { LessonService, Lesson, LessonFile, CreateLessonDto, UpdateLessonDto, RequirementIconDto } from '../../services/lesson.service';
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

const ALLOWED_EXTENSIONS = ['.md', '.docx', '.txt'];

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

  courses: Course[] = [];
  selectedCourseId: number | null = null;
  coursesLoading = false;

  lessons: Lesson[] = [];
  loading = false;
  error: string | null = null;

  showModal = false;
  isEditMode = false;
  currentLessonId: number | null = null;
  modalError: string | null = null;
  selectedRequirementIcons: number[] = [];
  lessonForm: FormGroup;
  pendingFiles: File[] = [];
  fileError: string | null = null;

  showDeleteConfirm = false;
  lessonToDelete: Lesson | null = null;

  uploadingLessonId: number | null = null;
  uploadSuccess: string | null = null;

  lessonFiles: Map<number, LessonFile[]> = new Map();

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
      durationMinutes: [90, [Validators.required, Validators.min(1), Validators.max(180)]],
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

  canManage(): boolean {
    return this.keycloakService.hasRole('Admin') || this.keycloakService.hasRole('Supervisor');
  }

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

  loadLessons() {
    if (!this.selectedCourseId) return;

    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();

    this.lessonService.getLessonsByCourse(this.selectedCourseId).subscribe({
      next: (lessons) => {
        this.lessons = lessons.sort((a, b) => a.order - b.order);
        this.loading = false;
        this.loadAllLessonFiles();
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

  loadAllLessonFiles() {
    for (const lesson of this.lessons) {
      if (lesson.albumId) {
        this.lessonService.getLessonFiles(lesson.id).subscribe({
          next: (files) => {
            this.lessonFiles.set(lesson.id, files);
            this.cdr.detectChanges();
          },
          error: () => {},
        });
      }
    }
  }

  navigateToDetail(lesson: Lesson) {
    this.router.navigate(['/lessons', lesson.id]);
  }

  hasFiles(lesson: Lesson): boolean {
    const files = this.lessonFiles.get(lesson.id);
    return !!files && files.length > 0;
  }

  hasMarkdownFile(lesson: Lesson): boolean {
    const files = this.lessonFiles.get(lesson.id);
    return !!files && files.some(f => f.name.toLowerCase().endsWith('.md'));
  }

  hasWordOrTextFile(lesson: Lesson): boolean {
    const files = this.lessonFiles.get(lesson.id);
    return !!files && files.some(f => {
      const lower = f.name.toLowerCase();
      return lower.endsWith('.docx') || lower.endsWith('.txt');
    });
  }

  getMaterialLabels(lesson: Lesson): string[] {
    const labels: string[] = [];
    if (this.hasMarkdownFile(lesson)) labels.push('Markdown');
    if (this.hasWordOrTextFile(lesson)) labels.push('Word/Text');
    return labels;
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentLessonId = null;
    this.modalError = null;
    this.fileError = null;
    this.selectedRequirementIcons = [];
    this.pendingFiles = [];

    this.lessonForm.reset({
      title: '',
      description: '',
      durationMinutes: 90,
    });
    this.showModal = true;
  }

  openEditModal(lesson: Lesson) {
    this.isEditMode = true;
    this.currentLessonId = lesson.id;
    this.modalError = null;
    this.fileError = null;
    this.selectedRequirementIcons = (lesson.requirementIcons || []).map(r => r.id);
    this.pendingFiles = [];

    this.lessonService.getLessonById(lesson.id).subscribe({
      next: (detail) => {
        this.lessonForm.reset({
          title: detail.title,
          description: detail.description || '',
          durationMinutes: detail.durationMinutes,
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
    this.fileError = null;
    this.selectedRequirementIcons = [];
    this.pendingFiles = [];
  }

  toggleRequirementIcon(id: number) {
    const index = this.selectedRequirementIcons.indexOf(id);
    if (index >= 0) {
      this.selectedRequirementIcons.splice(index, 1);
    } else {
      this.selectedRequirementIcons.push(id);
    }
  }

  isRequirementSelected(id: number): boolean {
    return this.selectedRequirementIcons.includes(id);
  }

  onModalFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const files = input.files;
    if (!files) return;

    this.fileError = null;
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
      if (!ALLOWED_EXTENSIONS.includes(ext)) {
        this.fileError = `File "${file.name}" has an unsupported extension. Only .md, .docx, and .txt files are accepted.`;
        input.value = '';
        this.cdr.detectChanges();
        return;
      }
      if (this.pendingFiles.some(f => f.name === file.name)) {
        this.fileError = `File "${file.name}" is already added.`;
        input.value = '';
        this.cdr.detectChanges();
        return;
      }
      this.pendingFiles.push(file);
    }
    input.value = '';
    this.cdr.detectChanges();
  }

  removePendingFile(index: number) {
    this.pendingFiles.splice(index, 1);
    this.fileError = null;
    this.cdr.detectChanges();
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
        requirementIconIds: this.selectedRequirementIcons,
      };

      this.lessonService.updateLesson(this.currentLessonId, payload).subscribe({
        next: () => {
          this.uploadPendingFilesForLesson(this.currentLessonId!);
          this.loadLessons();
          this.closeModal();
        },
        error: (err) => {
          this.modalError = err?.detail || 'Failed to update lesson. Please check your input and try again.';
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
        requirementIconIds: this.selectedRequirementIcons,
      };

      this.lessonService.createLesson(payload).subscribe({
        next: (newId) => {
          this.uploadPendingFilesForLesson(newId);
          this.loadLessons();
          this.closeModal();
        },
        error: (err) => {
          this.modalError = err?.detail || 'Failed to create lesson. Please check your input and try again.';
          console.error('Error creating lesson:', err);
          this.cdr.detectChanges();
        },
      });
    }
  }

  private uploadPendingFilesForLesson(lessonId: number) {
    if (this.pendingFiles.length === 0) return;

    const uploads = this.pendingFiles.map(file =>
      this.lessonService.uploadMaterial(lessonId, file)
    );

    concat(...uploads).subscribe({
      error: (err) => {
        console.error('Error uploading file:', err);
        this.error = err?.detail || 'Failed to upload material file.';
        this.cdr.detectChanges();
      },
      complete: () => {
        this.loadLessons();
      },
    });
  }

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

  onFileSelected(event: Event, lesson: Lesson) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (!ALLOWED_EXTENSIONS.includes(ext)) {
      this.error = `File "${file.name}" has an unsupported extension. Only .md, .docx, and .txt files are accepted.`;
      input.value = '';
      this.cdr.detectChanges();
      return;
    }

    this.uploadingLessonId = lesson.id;
    this.uploadSuccess = null;
    this.cdr.detectChanges();

    this.lessonService.uploadMaterial(lesson.id, file).subscribe({
      next: () => {
        this.uploadingLessonId = null;
        this.uploadSuccess = `Material "${file.name}" uploaded successfully.`;
        this.loadLessons();
        setTimeout(() => {
          this.uploadSuccess = null;
          this.cdr.detectChanges();
        }, 4000);
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.uploadingLessonId = null;
        this.error = err?.detail || `Failed to upload material file "${file.name}".`;
        console.error('Error uploading material:', err);
        this.cdr.detectChanges();
      },
    });

    input.value = '';
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(_event: Event) {
    if (this.showModal) {
      this.closeModal();
    }
    if (this.showDeleteConfirm) {
      this.cancelDelete();
    }
  }

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
    if (controls['durationMinutes']?.errors?.['max']) {
      return 'Duration cannot exceed 3 hours (180 minutes)';
    }

    return 'Please fix the errors in the form.';
  }
}
