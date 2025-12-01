import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TeacherService, Teacher, CreateTeacherCommand, UpdateTeacherCommand } from '../../services/teacher.service';

@Component({
  selector: 'app-teachers',
  imports: [CommonModule, FormsModule],
  templateUrl: './teachers.html',
  styleUrl: './teachers.scss',
})
export class Teachers implements OnInit {
  teachers = signal<Teacher[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  
  showModal = signal(false);
  editMode = signal(false);
  currentTeacherId = signal<string | null>(null);
  
  formData = signal({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    dateOfBirth: ''
  });

  constructor(private teacherService: TeacherService) {}

  ngOnInit() {
    this.loadTeachers();
  }

  loadTeachers() {
    this.loading.set(true);
    this.error.set(null);
    this.teacherService.getAllTeachers().subscribe({
      next: (data) => {
        this.teachers.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load teachers');
        this.loading.set(false);
        console.error('Error loading teachers:', err);
      }
    });
  }

  openCreateModal() {
    this.editMode.set(false);
    this.currentTeacherId.set(null);
    this.formData.set({
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      dateOfBirth: ''
    });
    this.showModal.set(true);
  }

  openEditModal(teacher: Teacher) {
    // Note: Edit is limited because the GET /Teachers endpoint only returns
    // firstName and lastName. To enable full edit functionality, we would need
    // a GET /Teachers/{id} endpoint that returns email, phoneNumber, and dateOfBirth.
    // For now, we only populate the fields we have from the list.
    this.editMode.set(true);
    this.currentTeacherId.set(teacher.id);
    this.formData.set({
      firstName: teacher.firstName,
      lastName: teacher.lastName,
      email: '', // Would need to fetch from a detail endpoint
      phoneNumber: '', // Would need to fetch from a detail endpoint
      dateOfBirth: '' // Would need to fetch from a detail endpoint
    });
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  submitForm() {
    const data = this.formData();
    
    if (this.editMode() && this.currentTeacherId()) {
      const command: UpdateTeacherCommand = {
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        phoneNumber: data.phoneNumber,
        dateOfBirth: data.dateOfBirth
      };
      
      this.teacherService.updateTeacher(this.currentTeacherId()!, command).subscribe({
        next: () => {
          this.closeModal();
          this.loadTeachers();
        },
        error: (err) => {
          this.error.set('Failed to update teacher');
          console.error('Error updating teacher:', err);
        }
      });
    } else {
      const command: CreateTeacherCommand = {
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        phoneNumber: data.phoneNumber,
        dateOfBirth: data.dateOfBirth
      };
      
      this.teacherService.createTeacher(command).subscribe({
        next: () => {
          this.closeModal();
          this.loadTeachers();
        },
        error: (err) => {
          this.error.set('Failed to create teacher');
          console.error('Error creating teacher:', err);
        }
      });
    }
  }
}

