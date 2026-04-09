import { Component, OnInit, OnDestroy, HostListener, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { TeacherService, Teacher, CreateTeacherDto, UpdateTeacherDto } from '../../services/teacher.service';
import { ProgrammingLanguageService, ProgrammingLanguage } from '../../services/programming-language.service';
import { PagedResult } from '../../models/paged-result.model';
import { PaginationBarComponent } from '../../components/pagination-bar/pagination-bar';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  heroAcademicCap,
  heroMagnifyingGlass,
  heroPlus,
  heroEnvelope,
  heroPhone,
  heroCalendarDays,
  heroCodeBracket,
  heroPencil,
  heroTrash,
} from '@ng-icons/heroicons/outline';

@Component({
  selector: 'app-teachers',
  imports: [CommonModule, ReactiveFormsModule, PaginationBarComponent, NgIconComponent],
  providers: [provideIcons({ heroAcademicCap, heroMagnifyingGlass, heroPlus, heroEnvelope, heroPhone, heroCalendarDays, heroCodeBracket, heroPencil, heroTrash })],
  templateUrl: './teachers.html',
  styleUrl: './teachers.scss',
})
export class Teachers implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly search = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(12);
  readonly pageSizeOptions = [12, 20, 30, 50];

  readonly totalCount = signal(0);
  readonly totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()) || 1);

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

  private readonly searchSubject = new Subject<string>();
  private subscriptions: Subscription[] = [];

  constructor(
    private teacherService: TeacherService,
    private programmingLanguageService: ProgrammingLanguageService,
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
        this.loadTeachers();
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

  onPageSizeChange(newSize: number) {
    this.navigateWithParams({ pageSize: newSize, page: 1 });
  }

  goToPage(p: number) {
    if (p < 1 || p > this.totalPages()) return;
    this.navigateWithParams({ page: p });
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

  loadTeachers() {
    this.loading = true;
    this.error = null;

    this.teacherService.getAllTeachers({
      search: this.search(),
      page: this.page(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (result: PagedResult<Teacher>) => {
        this.teachers = result.items;
        this.totalCount.set(result.totalCount);
        this.loading = false;

        if (this.page() > this.totalPages() && result.totalCount > 0) {
          this.navigateWithParams({ page: this.totalPages() });
          return;
        }
      },
      error: (error) => {
        this.error = 'Failed to load teachers. Please try again.';
        this.loading = false;
        console.error('Error loading teachers:', error);
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
          const status = error?.status as number | undefined;
          if (status === 409) {
            this.error = 'This teacher is assigned to a group and cannot be deleted.';
          } else {
            this.error = 'Failed to delete teacher. Please try again.';
          }
          console.error('Error deleting teacher:', error);
          this.cancelDelete();
        }
      });
    }
  }


  seedTestTeachers() {
    this.seedingInProgress = true;
    this.error = null;

    this.teacherService.seedTestTeachers().subscribe({
      next: (count) => {
        this.seedingInProgress = false;
        this.loadTeachers();
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
      },
      error: (error) => {
        this.error = 'Failed to load programming languages. Please try again.';
        console.error('Error loading programming languages:', error);
      }
    });
  }


  deleteTestTeachers() {
    this.deletingTestInProgress = true;
    this.error = null;

    this.teacherService.deleteTestTeachers().subscribe({
      next: (count) => {
        this.deletingTestInProgress = false;
        this.loadTeachers();
      },
      error: (error) => {
        this.error = 'Failed to delete test teachers. Please try again.';
        this.deletingTestInProgress = false;
        console.error('Error deleting test teachers:', error);
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
        },
        error: (error) => {
          this.error = 'Failed to unassign programming language. Please try again.';
          console.error('Error unassigning language:', error);
        }
      });
    } else {
      this.programmingLanguageService.assignToTeacher(this.languageTeacher.id, language.id).subscribe({
        next: () => {
          if (this.languageTeacher) {
            this.languageTeacher.programmingLanguages = [
              ...this.languageTeacher.programmingLanguages,
              { id: language.id, name: language.name, label: language.label, color: language.color }
            ];
          }
          this.loadTeachers();
        },
        error: (error) => {
          this.error = 'Failed to assign programming language. Please try again.';
          console.error('Error assigning language:', error);
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
