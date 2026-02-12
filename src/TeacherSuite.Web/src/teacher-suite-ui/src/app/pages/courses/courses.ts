import { Component, OnInit, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { CourseService, Course, AgeGroup, CreateCourseDto, UpdateCourseDto } from '../../services/course.service';

@Component({
  selector: 'app-courses',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './courses.html',
  styleUrl: './courses.scss',
})
export class Courses implements OnInit {
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
    this.loadCourses();
    this.loadAgeGroups();
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
          this.error = 'Failed to delete course. Please try again.';
          console.error('Error deleting course:', error);
          this.cancelDelete();
        }
      });
    }
  }

  getAgeGroupName(ageGroupId: number): string {
    const ageGroup = this.ageGroups.find(ag => ag.id === ageGroupId);
    return ageGroup ? ageGroup.name || 'Unknown' : 'Unknown';
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
