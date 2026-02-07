import { Component, OnInit, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { TeacherService, Teacher, CreateTeacherDto, UpdateTeacherDto } from '../../services/teacher.service';
import { formatDate, getCurrentDate } from '../../utils/date-utils';
import { dateOfBirthValidator, getDateOfBirthErrorMessage } from '../../utils/form-validators';

@Component({
  selector: 'app-teachers',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './teachers.html',
  styleUrl: './teachers.scss',
})
export class Teachers implements OnInit {
  teachers: Teacher[] = [];
  loading = false;
  error: string | null = null;
  
  showModal = false;
  isEditMode = false;
  currentTeacherId: string | null = null;
  modalError: string | null = null;

  teacherForm: FormGroup;

  showDeleteConfirm = false;
  teacherToDelete: Teacher | null = null;

  constructor(
    private teacherService: TeacherService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder
  ) {
    this.teacherForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required]],
      dateOfBirth: ['', [Validators.required, dateOfBirthValidator]]
    });
  }

  ngOnInit() {
    this.loadTeachers();
  }

  loadTeachers() {
    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();
    
    this.teacherService.getAllTeachers().subscribe({
      next: (teachers) => {
        this.teachers = teachers;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to load teachers. Please try again.';
        this.loading = false;
        console.error('Error loading teachers:', error);
        this.cdr.detectChanges();
      }
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentTeacherId = null;
    this.modalError = null;
    this.teacherForm.reset({
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      dateOfBirth: ''
    });
    this.showModal = true;
  }

  openEditModal(teacher: Teacher) {
    this.isEditMode = true;
    this.currentTeacherId = teacher.id;
    this.modalError = null;
    this.teacherForm.reset({
      firstName: teacher.firstName,
      lastName: teacher.lastName,
      email: teacher.email,
      phoneNumber: teacher.phoneNumber,
      dateOfBirth: teacher.dateOfBirth?.split('T')[0] || ''
    });
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.isEditMode = false;
    this.currentTeacherId = null;
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

  saveTeacher() {
    this.modalError = null;

    if (this.teacherForm.invalid) {
      this.teacherForm.markAllAsTouched();
      this.modalError = this.getFormErrorMessage();
      this.cdr.detectChanges();
      return;
    }

    const teacherPayload = this.teacherForm.getRawValue() as CreateTeacherDto | UpdateTeacherDto;

    if (this.isEditMode && this.currentTeacherId) {
      this.teacherService.updateTeacher(this.currentTeacherId, teacherPayload).subscribe({
        next: () => {
          this.loadTeachers();
          this.closeModal();
        },
        error: (error) => {
          this.modalError = 'Failed to update teacher. Please check your input and try again.';
          console.error('Error updating teacher:', error);
          this.cdr.detectChanges();
        }
      });
    } else {
      this.teacherService.createTeacher(teacherPayload).subscribe({
        next: () => {
          this.loadTeachers();
          this.closeModal();
        },
        error: (error) => {
          this.modalError = 'Failed to create teacher. Please check your input and try again.';
          console.error('Error creating teacher:', error);
          this.cdr.detectChanges();
        }
      });
    }
  }

  confirmDelete(teacher: Teacher) {
    this.teacherToDelete = teacher;
    this.showDeleteConfirm = true;
  }

  cancelDelete() {
    this.teacherToDelete = null;
    this.showDeleteConfirm = false;
  }

  deleteTeacher() {
    if (this.teacherToDelete) {
      this.teacherService.deleteTeacher(this.teacherToDelete.id).subscribe({
        next: () => {
          this.loadTeachers();
          this.cancelDelete();
        },
        error: (error) => {
          this.error = 'Failed to delete teacher. Please try again.';
          console.error('Error deleting teacher:', error);
          this.cancelDelete();
        }
      });
    }
  }

  getFullName(teacher: Teacher): string {
    const firstName = teacher.firstName ?? '';
    const lastName = teacher.lastName ?? '';
    return `${firstName} ${lastName}`.trim();
  }

  formatDate(dateString: string): string {
    return formatDate(dateString);
  }

  getCurrentDate(): string {
    return getCurrentDate();
  }

  private getFormErrorMessage(): string | null {
    const controls = this.teacherForm.controls;

    if (controls['firstName']?.errors?.['required']) {
      return 'First name is required';
    }

    if (controls['lastName']?.errors?.['required']) {
      return 'Last name is required';
    }

    if (controls['email']?.errors?.['required']) {
      return 'Email is required';
    }

    if (controls['email']?.errors?.['email']) {
      return 'Please enter a valid email address';
    }

    if (controls['phoneNumber']?.errors?.['required']) {
      return 'Phone number is required';
    }

    const dateError = getDateOfBirthErrorMessage(controls['dateOfBirth']?.errors);
    if (dateError) {
      return dateError;
    }

    return 'Please fix the errors in the form.';
  }
}
