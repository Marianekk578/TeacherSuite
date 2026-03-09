import { Component, OnInit, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CourseService, Course, AgeGroup, ProgrammingLanguage, CreateCourseDto, UpdateCourseDto } from '../../services/course.service';
import { KeycloakService } from '../../auth/keycloak.service';

@Component({
  selector: 'app-courses',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './courses.html',
  styleUrl: './courses.scss',
})
export class Courses implements OnInit {
  courses: Course[] = [];
  ageGroups: AgeGroup[] = [];
  allProgrammingLanguages: ProgrammingLanguage[] = [];
  loading = false;
  error: string | null = null;
  
  showModal = false;
  isEditMode = false;
  currentCourseId: number | null = null;
  modalError: string | null = null;
  selectedLanguageIds: number[] = [];

  courseForm: FormGroup;

  showDeleteConfirm = false;
  courseToDelete: Course | null = null;

  showDetailsModal = false;
  selectedCourse: Course | null = null;
  detailsLoading = false;

  constructor(
    private courseService: CourseService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private keycloakService: KeycloakService
  ) {
    this.courseForm = this.fb.group({
      name: ['', [Validators.required]],
      description: ['', [Validators.required]],
      ageGroupID: [null, [Validators.required]]
    });
  }

  ngOnInit() {
    this.loadCourses();
    this.loadAgeGroups();
    this.loadProgrammingLanguages();

    this.route.queryParams.subscribe(params => {
      const courseId = params['courseId'];
      if (courseId) {
        this.openDetailsById(+courseId);
      }
    });
  }

  loadCourses() {
    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();
    
    this.courseService.getAllCourses().subscribe({
      next: (courses) => {
        this.courses = courses;
        this.loading = false;
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

  loadProgrammingLanguages() {
    this.courseService.getAllProgrammingLanguages().subscribe({
      next: (languages) => {
        this.allProgrammingLanguages = languages;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading programming languages:', error);
      }
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentCourseId = null;
    this.modalError = null;
    this.selectedLanguageIds = [];
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
    this.selectedLanguageIds = course.programmingLanguages?.map(pl => pl.id) || [];
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
    this.selectedLanguageIds = [];
  }

  toggleLanguage(languageId: number) {
    const index = this.selectedLanguageIds.indexOf(languageId);
    if (index >= 0) {
      this.selectedLanguageIds.splice(index, 1);
    } else {
      this.selectedLanguageIds.push(languageId);
    }
  }

  isLanguageSelected(languageId: number): boolean {
    return this.selectedLanguageIds.includes(languageId);
  }

  getLanguageColor(pl: ProgrammingLanguage): string {
    return pl.color || '#667eea';
  }

  getLanguageLabel(pl: ProgrammingLanguage): string {
    return pl.label || pl.name || '';
  }

  getAgeGroupLabel(ag: AgeGroup): string {
    return ag.label || ag.name || '';
  }

  openDetailsModal(course: Course) {
    this.detailsLoading = true;
    this.showDetailsModal = true;
    this.selectedCourse = course;
    this.cdr.detectChanges();

    this.courseService.getCourseById(course.id).subscribe({
      next: (fullCourse) => {
        this.selectedCourse = fullCourse;
        this.detailsLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading course details:', error);
        this.detailsLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openDetailsById(courseId: number) {
    this.detailsLoading = true;
    this.showDetailsModal = true;
    this.selectedCourse = null;
    this.cdr.detectChanges();

    this.courseService.getCourseById(courseId).subscribe({
      next: (course) => {
        this.selectedCourse = course;
        this.detailsLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading course details:', error);
        this.detailsLoading = false;
        this.showDetailsModal = false;
        this.error = 'Failed to load course details.';
        this.cdr.detectChanges();
      }
    });
  }

  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedCourse = null;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {},
      replaceUrl: true
    });
  }

  showAssignedGroups(course: Course) {
    this.closeDetailsModal();
    this.router.navigate(['/groups'], {
      queryParams: { courseName: course.name }
    });
  }

  canSeeAssignedGroups(): boolean {
    return this.keycloakService.hasRole('Admin') || this.keycloakService.hasRole('Supervisor');
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: Event) {
    if (this.showModal) {
      this.closeModal();
    }
    if (this.showDeleteConfirm) {
      this.cancelDelete();
    }
    if (this.showDetailsModal) {
      this.closeDetailsModal();
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

    const formValue = this.courseForm.getRawValue();
    const coursePayload: CreateCourseDto | UpdateCourseDto = {
      name: formValue.name,
      description: formValue.description,
      ageGroupID: formValue.ageGroupID,
      programmingLanguageIds: this.selectedLanguageIds
    };

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
