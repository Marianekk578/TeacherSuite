import { Component, OnInit, OnDestroy, ChangeDetectorRef, HostListener, signal, computed, inject } from '@angular/core';
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
import { StudentService, Student, StudentDetail, CreateStudentDto, UpdateStudentDto, GroupWithAgeGroup } from '../../services/student.service';
import { PagedResult } from '../../models/paged-result.model';
import { PaginationBarComponent } from '../../components/pagination-bar/pagination-bar';
import { KeycloakService } from '../../auth/keycloak.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  heroAcademicCap,
  heroMagnifyingGlass,
  heroPlus,
  heroInformationCircle,
  heroUserGroup,
  heroPencil,
  heroTrash,
  heroEnvelope,
  heroCalendarDays,
} from '@ng-icons/heroicons/outline';

@Component({
  selector: 'app-students',
  imports: [CommonModule, ReactiveFormsModule, PaginationBarComponent, NgIconComponent],
  providers: [provideIcons({ heroAcademicCap, heroMagnifyingGlass, heroPlus, heroInformationCircle, heroUserGroup, heroPencil, heroTrash, heroEnvelope, heroCalendarDays })],
  templateUrl: './students.html',
  styleUrl: './students.scss',
})
export class Students implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly search = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(12);
  readonly pageSizeOptions = [12, 20, 30, 50];

  readonly totalCount = signal(0);
  readonly totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()) || 1);

  students: Student[] = [];
  loading = false;
  error: string | null = null;

  showModal = false;
  isEditMode = false;
  currentStudentId: string | null = null;
  modalError: string | null = null;

  studentForm: FormGroup;

  showDeleteConfirm = false;
  studentToDelete: Student | null = null;

  showDetailsModal = false;
  detailsLoading = false;
  selectedStudentDetail: StudentDetail | null = null;

  showGroupModal = false;
  groupStudent: Student | null = null;
  allGroups: GroupWithAgeGroup[] = [];
  filteredGroupsForAssign: GroupWithAgeGroup[] = [];

  get filteredGroupsForCreate(): GroupWithAgeGroup[] {
    const dob = this.studentForm.get('dateOfBirth')?.value;
    if (!dob) return this.allGroups;
    const age = this.calculateAge(dob);
    return this.allGroups.filter(g => {
      if (!g.ageGroup) return true;
      return age >= g.ageGroup.minAge && age <= g.ageGroup.maxAge;
    });
  }

  isAdminOrSupervisor = false;

  private readonly searchSubject = new Subject<string>();
  private subscriptions: Subscription[] = [];

  readonly courseStatusLabels: Record<number, string> = {
    0: 'Planned',
    1: 'Active',
    2: 'Completed',
    3: 'Cancelled'
  };

  constructor(
    private studentService: StudentService,
    private keycloakService: KeycloakService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder
  ) {
    this.studentForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      dateOfBirth: ['', [Validators.required, this.dateOfBirthValidator.bind(this)]],
      contactEmail: ['', [Validators.required, Validators.email]],
      contactPhone: ['', [Validators.required]],
      parentFirstName: [''],
      parentLastName: [''],
      groupId: ['']
    });

    this.isAdminOrSupervisor = this.keycloakService.hasRole('Admin') || this.keycloakService.hasRole('Supervisor');

    const dobSub = this.studentForm.get('dateOfBirth')?.valueChanges.subscribe(() => {
      if (this.isAdult()) {
        this.studentForm.patchValue({
          parentFirstName: '',
          parentLastName: ''
        }, { emitEvent: false });
      }
    });
    if (dobSub) {
      this.subscriptions.push(dobSub);
    }
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
        this.loadStudents();
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

  loadStudents() {
    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();

    this.studentService.getAllStudents({
      search: this.search(),
      page: this.page(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (result: PagedResult<Student>) => {
        this.students = result.items;
        this.totalCount.set(result.totalCount);
        this.loading = false;

        if (this.page() > this.totalPages() && result.totalCount > 0) {
          this.navigateWithParams({ page: this.totalPages() });
          return;
        }

        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to load students. Please try again.';
        this.loading = false;
        console.error('Error loading students:', error);
        this.cdr.detectChanges();
      }
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentStudentId = null;
    this.modalError = null;
    this.studentForm.reset({
      firstName: '',
      lastName: '',
      dateOfBirth: '',
      contactEmail: '',
      contactPhone: '',
      parentFirstName: '',
      parentLastName: '',
      groupId: ''
    });
    this.loadGroupsForModal();
    this.showModal = true;
  }

  openEditModal(student: Student) {
    this.isEditMode = true;
    this.currentStudentId = student.id;
    this.modalError = null;
    this.studentForm.reset({
      firstName: student.firstName,
      lastName: student.lastName,
      dateOfBirth: student.dateOfBirth?.split('T')[0] || '',
      contactEmail: student.contactEmail,
      contactPhone: student.contactPhone,
      parentFirstName: student.parentFirstName || '',
      parentLastName: student.parentLastName || '',
      groupId: ''
    });
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.isEditMode = false;
    this.currentStudentId = null;
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
    if (this.showDetailsModal) {
      this.closeDetailsModal();
    }
    if (this.showGroupModal) {
      this.closeGroupModal();
    }
  }

  saveStudent() {
    this.modalError = null;

    if (this.studentForm.invalid) {
      this.studentForm.markAllAsTouched();
      this.modalError = this.getFormErrorMessage();
      this.cdr.detectChanges();
      return;
    }

    const formValue = this.studentForm.getRawValue();

    if (this.isEditMode && this.currentStudentId) {
      const payload: UpdateStudentDto = {
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        dateOfBirth: formValue.dateOfBirth,
        contactEmail: formValue.contactEmail,
        contactPhone: formValue.contactPhone,
        parentFirstName: formValue.parentFirstName || undefined,
        parentLastName: formValue.parentLastName || undefined,
      };

      this.studentService.updateStudent(this.currentStudentId, payload).subscribe({
        next: () => {
          this.loadStudents();
          this.closeModal();
        },
        error: (error) => {
          this.modalError = 'Failed to update student. Please check your input and try again.';
          console.error('Error updating student:', error);
          this.cdr.detectChanges();
        }
      });
    } else {
      const payload: CreateStudentDto = {
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        dateOfBirth: formValue.dateOfBirth,
        contactEmail: formValue.contactEmail,
        contactPhone: formValue.contactPhone,
        parentFirstName: formValue.parentFirstName || undefined,
        parentLastName: formValue.parentLastName || undefined,
        groupId: formValue.groupId || undefined,
      };

      this.studentService.createStudent(payload).subscribe({
        next: () => {
          this.loadStudents();
          this.closeModal();
        },
        error: (error) => {
          this.modalError = 'Failed to create student. Please check your input and try again.';
          console.error('Error creating student:', error);
          this.cdr.detectChanges();
        }
      });
    }
  }

  confirmDelete(student: Student) {
    this.studentToDelete = student;
    this.showDeleteConfirm = true;
  }

  cancelDelete() {
    this.studentToDelete = null;
    this.showDeleteConfirm = false;
  }

  deleteStudent() {
    if (this.studentToDelete) {
      this.studentService.deleteStudent(this.studentToDelete.id).subscribe({
        next: () => {
          this.loadStudents();
          this.cancelDelete();
        },
        error: (error) => {
          this.error = 'Failed to delete student. Please try again.';
          console.error('Error deleting student:', error);
          this.cancelDelete();
        }
      });
    }
  }

  openDetailsModal(student: Student) {
    this.detailsLoading = true;
    this.selectedStudentDetail = null;
    this.showDetailsModal = true;
    this.cdr.detectChanges();

    this.studentService.getStudentById(student.id).subscribe({
      next: (detail) => {
        this.selectedStudentDetail = detail;
        this.detailsLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.detailsLoading = false;
        this.error = 'Failed to load student details.';
        this.showDetailsModal = false;
        console.error('Error loading student details:', error);
        this.cdr.detectChanges();
      }
    });
  }

  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedStudentDetail = null;
  }

  openGroupModal(student: Student) {
    this.groupStudent = student;
    const studentAge = this.calculateAge(student.dateOfBirth);
    this.studentService.getAllGroups().subscribe({
      next: (groups) => {
        this.allGroups = groups;
        this.filteredGroupsForAssign = groups.filter(g => {
          if (!g.ageGroup) return true;
          return studentAge >= g.ageGroup.minAge && studentAge <= g.ageGroup.maxAge;
        });
        this.showGroupModal = true;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to load groups. Please try again.';
        console.error('Error loading groups:', error);
        this.cdr.detectChanges();
      }
    });
  }

  closeGroupModal() {
    this.showGroupModal = false;
    this.groupStudent = null;
  }

  isGroupAssigned(group: { id: string }): boolean {
    if (!this.groupStudent?.groups) return false;
    return this.groupStudent.groups.some(g => g.groupId === group.id);
  }

  toggleGroup(group: { id: string; name: string }) {
    if (!this.groupStudent) return;

    if (this.isGroupAssigned(group)) {
      this.studentService.unassignFromGroup(this.groupStudent.id, group.id).subscribe({
        next: () => {
          if (this.groupStudent) {
            this.groupStudent.groups = this.groupStudent.groups.filter(g => g.groupId !== group.id);
          }
          this.loadStudents();
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.error = 'Failed to unassign student from group. Please try again.';
          console.error('Error unassigning group:', error);
          this.cdr.detectChanges();
        }
      });
    } else {
      this.studentService.assignToGroup(this.groupStudent.id, group.id).subscribe({
        next: () => {
          if (this.groupStudent) {
            this.groupStudent.groups = [
              ...this.groupStudent.groups,
              { groupId: group.id, groupName: group.name }
            ];
          }
          this.loadStudents();
          this.cdr.detectChanges();
        },
        error: (error) => {
          const status = error?.status as number | undefined;
          if (status === 409) {
            const detail = (error?.detail ?? error?.message ?? '') as string;
            if (detail.toLowerCase().includes('age')) {
              this.error = 'Student age does not match the group\'s age range.';
            } else if (detail.toLowerCase().includes('already')) {
              this.error = 'Student is already assigned to this group.';
            } else {
              this.error = detail || 'Conflict: unable to assign student to this group.';
            }
          } else {
            this.error = 'Failed to assign student to group. Please try again.';
          }
          console.error('Error assigning group:', error);
          this.cdr.detectChanges();
        }
      });
    }
  }

  getFullName(student: { firstName?: string; lastName?: string }): string {
    const firstName = student.firstName ?? '';
    const lastName = student.lastName ?? '';
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

  getContactLabel(): string {
    const dob = this.studentForm.get('dateOfBirth')?.value;
    if (!dob) return 'Contact';
    const age = this.calculateAge(dob);
    return age >= 18 ? 'Contact' : "Parent's Contact";
  }

  hasDateOfBirth(): boolean {
    const dob = this.studentForm.get('dateOfBirth')?.value;
    if (!dob) return false;
    const date = new Date(dob);
    if (isNaN(date.getTime()) || date >= new Date()) return false;
    return this.calculateAge(dob) >= 7;
  }

  isAdult(): boolean {
    const dob = this.studentForm.get('dateOfBirth')?.value;
    if (!dob) return false;
    return this.calculateAge(dob) >= 18;
  }

  isStudentAdult(student: { dateOfBirth: string }): boolean {
    return this.calculateAge(student.dateOfBirth) >= 18;
  }

  getCurrentDate(): string {
    const today = new Date();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${today.getFullYear()}-${month}-${day}`;
  }

  getMaxBirthYear(): number {
    return new Date().getFullYear() - 7;
  }

  private calculateAge(dateString: string): number {
    const date = new Date(dateString);
    const today = new Date();
    let age = today.getFullYear() - date.getFullYear();
    const m = today.getMonth() - date.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < date.getDate())) {
      age--;
    }
    return age;
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

    const age = this.calculateAge(control.value);
    if (age < 7) {
      return { tooYoung: true };
    }

    return null;
  }

  private getFormErrorMessage(): string | null {
    const controls = this.studentForm.controls;

    if (controls['firstName']?.errors?.['required']) {
      return 'First name is required';
    }

    if (controls['lastName']?.errors?.['required']) {
      return 'Last name is required';
    }

    if (controls['dateOfBirth']?.errors?.['required']) {
      return 'Date of birth is required';
    }

    if (controls['dateOfBirth']?.errors?.['futureDate']) {
      return 'Date of birth cannot be in the future';
    }

    if (controls['dateOfBirth']?.errors?.['tooYoung']) {
      return `Student must be at least 7 years old. Maximum birth year: ${new Date().getFullYear() - 7}`;
    }

    if (controls['contactEmail']?.errors?.['required']) {
      return 'Contact email is required';
    }

    if (controls['contactEmail']?.errors?.['email']) {
      return 'Please enter a valid email address';
    }

    if (controls['contactPhone']?.errors?.['required']) {
      return 'Contact phone is required';
    }

    return 'Please check the form for errors';
  }

  private loadGroupsForModal() {
    this.studentService.getAllGroups().subscribe({
      next: (groups) => {
        this.allGroups = groups;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading groups:', error);
      }
    });
  }
}
