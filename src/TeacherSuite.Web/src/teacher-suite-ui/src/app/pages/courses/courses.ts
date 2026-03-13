import { Component, OnInit, OnDestroy, ChangeDetectorRef, HostListener, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CourseService, Course, AgeGroup, CreateCourseDto, UpdateCourseDto } from '../../services/course.service';
import { PagedResult } from '../../services/teacher.service';

@Component({
  selector: 'app-courses',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './courses.html',
  styleUrl: './courses.scss',
})
export class Courses implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly search = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(12);
  readonly pageSizeOptions = [12, 20, 30, 50];

  readonly totalCount = signal(0);
  readonly totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()) || 1);

  courses: Course[] = [];
  ageGroups: AgeGroup[] = [];
  loading = false;
  error: string | null = null;
  
  showModal = false;
  isEditMode = false;
  currentCourseId: number | null = null;
  modalError: string | null = null;

  courseForm: FormGroup;

  showDeleteConfirm = false;
  courseToDelete: Course | null = null;

  seedingInProgress = false;
  deletingTestInProgress = false;

  private readonly searchSubject = new Subject<string>();
  private subscriptions: Subscription[] = [];

  constructor(
    private courseService: CourseService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder
  ) {
    this.courseForm = this.fb.group({
      name: ['', [Validators.required]],
      description: ['', [Validators.required]],
      ageGroupID: [null, [Validators.required]]
    });
  }

  ngOnInit() {
    this.loadAgeGroups();

    this.subscriptions.push(
      this.searchSubject.pipe(
        debounceTime(300),
        distinctUntilChanged()
      ).subscribe(value => {
        this.navigateWithParams({ search: value, page: 1 });
      })
    );

    this.subscriptions.push(
      this.route.queryParams.subscribe(params => {
        const s = params['search'] ?? '';
        const p = parseInt(params['page'], 10) || 1;
        const ps = parseInt(params['pageSize'], 10) || 12;

        this.search.set(s);
        this.page.set(p);
        this.pageSize.set(ps);
        this.loadCourses();
      })
    );
  }

  ngOnDestroy() {
    this.subscriptions.forEach(s => s.unsubscribe());
    this.searchSubject.complete();
  }

  onSearchInput(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.searchSubject.next(value);
  }

  onPageSizeChange(event: Event) {
    const value = parseInt((event.target as HTMLSelectElement).value, 10);
    this.navigateWithParams({ pageSize: value, page: 1 });
  }

  goToPage(p: number) {
    if (p < 1 || p > this.totalPages()) return;
    this.navigateWithParams({ page: p });
  }

  get visiblePages(): number[] {
    const total = this.totalPages();
    const current = this.page();
    const pages: number[] = [];
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  private navigateWithParams(overrides: { search?: string; page?: number; pageSize?: number }) {
    const search = overrides.search ?? this.search();
    const page = overrides.page ?? this.page();
    const pageSize = overrides.pageSize ?? this.pageSize();

    const queryParams: Record<string, string | undefined> = {};
    if (search) queryParams['search'] = search;
    if (page > 1) queryParams['page'] = String(page);
    if (pageSize !== 12) queryParams['pageSize'] = String(pageSize);

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
    });
  }

  loadCourses() {
    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();
    
    this.courseService.getAllCourses({
      search: this.search(),
      page: this.page(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (result: PagedResult<Course>) => {
        this.courses = result.items;
        this.totalCount.set(result.totalCount);
        this.loading = false;

        if (this.page() > this.totalPages() && result.totalCount > 0) {
          this.navigateWithParams({ page: this.totalPages() });
          return;
        }

        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to load courses. Please try again.';
        this.loading = false;
        console.error('Error loading courses:', error);
        this.cdr.detectChanges();
      }
    });
  }

  loadAgeGroups() {
    this.courseService.getAllAgeGroups().subscribe({
      next: (ageGroups) => {
        this.ageGroups = ageGroups;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading age groups:', error);
      }
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentCourseId = null;
    this.modalError = null;
    this.courseForm.reset({
      name: '',
      description: '',
      ageGroupID: null
    });
    this.showModal = true;
  }

  openEditModal(course: Course) {
    this.isEditMode = true;
    this.currentCourseId = course.id;
    this.modalError = null;
    this.courseForm.reset({
      name: course.name,
      description: course.description,
      ageGroupID: course.ageGroupID
    });
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.isEditMode = false;
    this.currentCourseId = null;
    this.modalError = null;
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: Event) {
    if (this.showModal) {
      this.closeModal();
    }
    if (this.showDeleteConfirm) {
      this.cancelDelete();
    }
  }

  saveCourse() {
    this.modalError = null;

    if (this.courseForm.invalid) {
      this.courseForm.markAllAsTouched();
      this.modalError = this.getFormErrorMessage();
      this.cdr.detectChanges();
      return;
    }

    const coursePayload = this.courseForm.getRawValue() as CreateCourseDto | UpdateCourseDto;

    if (this.isEditMode && this.currentCourseId !== null) {
      this.courseService.updateCourse(this.currentCourseId, coursePayload).subscribe({
        next: () => {
          this.loadCourses();
          this.closeModal();
        },
        error: (error) => {
          this.modalError = 'Failed to update course. Please check your input and try again.';
          console.error('Error updating course:', error);
          this.cdr.detectChanges();
        }
      });
    } else {
      this.courseService.createCourse(coursePayload).subscribe({
        next: () => {
          this.loadCourses();
          this.closeModal();
        },
        error: (error) => {
          this.modalError = 'Failed to create course. Please check your input and try again.';
          console.error('Error creating course:', error);
          this.cdr.detectChanges();
        }
      });
    }
  }

  confirmDelete(course: Course) {
    this.courseToDelete = course;
    this.showDeleteConfirm = true;
  }

  cancelDelete() {
    this.courseToDelete = null;
    this.showDeleteConfirm = false;
  }

  deleteCourse() {
    if (this.courseToDelete) {
      this.courseService.deleteCourse(this.courseToDelete.id).subscribe({
        next: () => {
          this.loadCourses();
          this.cancelDelete();
        },
        error: (error) => {
          const status = error?.status as number | undefined;
          if (status === 409) {
            this.error = 'This course is assigned to a group and cannot be deleted.';
          } else {
            this.error = 'Failed to delete course. Please try again.';
          }
          console.error('Error deleting course:', error);
          this.cancelDelete();
        }
      });
    }
  }

  seedTestCourses() {
    this.seedingInProgress = true;
    this.error = null;
    this.cdr.detectChanges();

    this.courseService.seedTestCourses().subscribe({
      next: () => {
        this.seedingInProgress = false;
        this.loadCourses();
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to seed test courses. Please try again.';
        this.seedingInProgress = false;
        console.error('Error seeding test courses:', error);
        this.cdr.detectChanges();
      }
    });
  }

  deleteTestCourses() {
    this.deletingTestInProgress = true;
    this.error = null;
    this.cdr.detectChanges();

    this.courseService.deleteTestCourses().subscribe({
      next: () => {
        this.deletingTestInProgress = false;
        this.loadCourses();
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to delete test courses. Please try again.';
        this.deletingTestInProgress = false;
        console.error('Error deleting test courses:', error);
        this.cdr.detectChanges();
      }
    });
  }

  private getFormErrorMessage(): string | null {
    const controls = this.courseForm.controls;

    if (controls['name']?.errors?.['required']) {
      return 'Course name is required';
    }

    if (controls['description']?.errors?.['required']) {
      return 'Description is required';
    }

    if (controls['ageGroupID']?.errors?.['required']) {
      return 'Age group is required';
    }

    return 'Please fix the errors in the form.';
  }
}
