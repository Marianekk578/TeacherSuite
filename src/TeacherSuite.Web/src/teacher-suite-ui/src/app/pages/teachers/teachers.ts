import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TeacherService, Teacher, CreateTeacherDto, UpdateTeacherDto } from '../../services/teacher.service';

@Component({
  selector: 'app-teachers',
  imports: [CommonModule, FormsModule],
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
  
  teacherForm: CreateTeacherDto | UpdateTeacherDto = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    dateOfBirth: ''
  };

  showDeleteConfirm = false;
  teacherToDelete: Teacher | null = null;

  constructor(
    private teacherService: TeacherService,
    private cdr: ChangeDetectorRef
  ) {}

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
    this.teacherForm = {
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      dateOfBirth: ''
    };
    this.showModal = true;
  }

  openEditModal(teacher: Teacher) {
    this.isEditMode = true;
    this.currentTeacherId = teacher.id;
    this.modalError = null;
    this.teacherForm = {
      firstName: teacher.firstName,
      lastName: teacher.lastName,
      email: teacher.email,
      phoneNumber: teacher.phoneNumber,
      dateOfBirth: teacher.dateOfBirth?.split('T')[0] || ''
    };
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.isEditMode = false;
    this.currentTeacherId = null;
    this.modalError = null;
  }

  saveTeacher() {
    // Client-side validation
    this.modalError = null;
    
    if (!this.teacherForm.firstName?.trim()) {
      this.modalError = 'First name is required';
      this.cdr.detectChanges();
      return;
    }
    
    if (!this.teacherForm.lastName?.trim()) {
      this.modalError = 'Last name is required';
      this.cdr.detectChanges();
      return;
    }
    
    if (!this.teacherForm.email?.trim()) {
      this.modalError = 'Email is required';
      this.cdr.detectChanges();
      return;
    }
    
    // Email format validation
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(this.teacherForm.email)) {
      this.modalError = 'Please enter a valid email address';
      this.cdr.detectChanges();
      return;
    }
    
    if (!this.teacherForm.phoneNumber?.trim()) {
      this.modalError = 'Phone number is required';
      this.cdr.detectChanges();
      return;
    }
    
    if (!this.teacherForm.dateOfBirth) {
      this.modalError = 'Date of birth is required';
      this.cdr.detectChanges();
      return;
    }
    
    if (this.isEditMode && this.currentTeacherId) {
      this.teacherService.updateTeacher(this.currentTeacherId, this.teacherForm).subscribe({
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
      this.teacherService.createTeacher(this.teacherForm).subscribe({
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
    return `${teacher.firstName} ${teacher.lastName}`.trim();
  }

  formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    
    const date = new Date(dateString);
    
    // Check if date is valid
    if (isNaN(date.getTime())) {
      return 'Invalid Date';
    }
    
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric',
      timeZone: 'UTC'
    });
  }
}
