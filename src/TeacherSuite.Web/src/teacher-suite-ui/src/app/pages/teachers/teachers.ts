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
  }

  saveTeacher() {
    if (this.isEditMode && this.currentTeacherId) {
      this.teacherService.updateTeacher(this.currentTeacherId, this.teacherForm).subscribe({
        next: () => {
          this.loadTeachers();
          this.closeModal();
        },
        error: (error) => {
          this.error = 'Failed to update teacher. Please try again.';
          console.error('Error updating teacher:', error);
        }
      });
    } else {
      this.teacherService.createTeacher(this.teacherForm).subscribe({
        next: () => {
          this.loadTeachers();
          this.closeModal();
        },
        error: (error) => {
          this.error = 'Failed to create teacher. Please try again.';
          console.error('Error creating teacher:', error);
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
