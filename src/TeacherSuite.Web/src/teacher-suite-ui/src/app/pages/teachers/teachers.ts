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
import { TeacherService, Teacher, CreateTeacherDto, UpdateTeacherDto } from '../../services/teacher.service';
import { ProgrammingLanguageService, ProgrammingLanguage } from '../../services/programming-language.service';

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

  seedingInProgress = false;
  deletingTestInProgress = false;
  showLanguageModal = false;
  languageTeacher: Teacher | null = null;
  allProgrammingLanguages: ProgrammingLanguage[] = [];

  constructor(
    private teacherService: TeacherService,
    private programmingLanguageService: ProgrammingLanguageService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder
  ) {
    this.teacherForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required]],
      dateOfBirth: ['', [Validators.required, this.dateOfBirthValidator.bind(this)]]
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
    if (this.showLanguageModal) {
      this.closeLanguageModal();
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


  seedTestTeachers() {
    this.seedingInProgress = true;
    this.error = null;
    this.cdr.detectChanges();

    this.teacherService.seedTestTeachers().subscribe({
      next: (count) => {
        this.seedingInProgress = false;
        this.loadTeachers();
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to seed test teachers. Please try again.';
        this.seedingInProgress = false;
        console.error('Error seeding test teachers:', error);
      }
    });
  }

  openLanguageModal(teacher: Teacher) {
    this.languageTeacher = teacher;
    this.programmingLanguageService.getAllProgrammingLanguages().subscribe({
      next: (languages) => {
        this.allProgrammingLanguages = languages;
        this.showLanguageModal = true;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to load programming languages. Please try again.';
        console.error('Error loading programming languages:', error);
        this.cdr.detectChanges();
      }
    });
  }


  deleteTestTeachers() {
    this.deletingTestInProgress = true;
    this.error = null;
    this.cdr.detectChanges();

    this.teacherService.deleteTestTeachers().subscribe({
      next: (count) => {
        this.deletingTestInProgress = false;
        this.loadTeachers();
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to delete test teachers. Please try again.';
        this.deletingTestInProgress = false;
        console.error('Error deleting test teachers:', error);
        this.cdr.detectChanges();
      }
    });
  }

  closeLanguageModal() {
    this.showLanguageModal = false;
    this.languageTeacher = null;
  }

  isLanguageAssigned(language: ProgrammingLanguage): boolean {
    if (!this.languageTeacher?.programmingLanguages) return false;
    return this.languageTeacher.programmingLanguages.some(lang => lang.id === language.id);
  }

  toggleLanguage(language: ProgrammingLanguage) {
    if (!this.languageTeacher) return;

    if (this.isLanguageAssigned(language)) {
      this.programmingLanguageService.unassignFromTeacher(this.languageTeacher.id, language.id).subscribe({
        next: () => {
          if (this.languageTeacher) {
            this.languageTeacher.programmingLanguages = this.languageTeacher.programmingLanguages.filter(lang => lang.id !== language.id);
          }
          this.loadTeachers();
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.error = 'Failed to unassign programming language. Please try again.';
          console.error('Error unassigning language:', error);
          this.cdr.detectChanges();
        }
      });
    } else {
      this.programmingLanguageService.assignToTeacher(this.languageTeacher.id, language.id).subscribe({
        next: () => {
          if (this.languageTeacher) {
            this.languageTeacher.programmingLanguages = [
              ...this.languageTeacher.programmingLanguages,
              { id: language.id, name: language.name }
            ];
          }
          this.loadTeachers();
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.error = 'Failed to assign programming language. Please try again.';
          console.error('Error assigning language:', error);
          this.cdr.detectChanges();
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
    if (!dateString) return 'N/A';
    
    const date = new Date(dateString);
    
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

  getCurrentDate(): string {
    const today = new Date();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${today.getFullYear()}-${month}-${day}`;
  }

  private dateOfBirthValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) {
      return null;
    }

    const date = new Date(control.value);
    if (isNaN(date.getTime())) {
      return { invalidDate: true };
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (date >= today) {
      return { futureDate: true };
    }

    const oldestAllowed = new Date();
    oldestAllowed.setFullYear(oldestAllowed.getFullYear() - 122);

    if (date <= oldestAllowed) {
      return { tooOld: true };
    }

    const youngestAllowed = new Date();
    youngestAllowed.setFullYear(youngestAllowed.getFullYear() - 18);

    if (date > youngestAllowed) {
      return { tooYoung: true };
    }

    return null;
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

    if (controls['dateOfBirth']?.errors?.['required']) {
      return 'Date of birth is required';
    }

    if (controls['dateOfBirth']?.errors?.['futureDate']) {
      return 'How can you predict when someone will be born?';
    }

    if (controls['dateOfBirth']?.errors?.['tooOld']) {
      return 'I dont think you can beat Jeanne Calment, she lived 122 years.';
    }

    if (controls['dateOfBirth']?.errors?.['tooYoung']) {
      return 'I know students who are older.';
    }

    if (controls['dateOfBirth']?.errors?.['invalidDate']) {
      return 'Date of birth is invalid.';
    }

    return 'Please fix the errors in the form.';
  }
}
