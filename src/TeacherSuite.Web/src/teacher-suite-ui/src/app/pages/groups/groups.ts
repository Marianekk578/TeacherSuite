import { Component, OnInit, OnDestroy, ChangeDetectorRef, HostListener, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, tap } from 'rxjs/operators';
import { GroupService, Group, CreateGroupDto, UpdateGroupDto, GroupCourseAssignment } from '../../services/group.service';
import { Teacher } from '../../services/teacher.service';
import { Course, AgeGroup } from '../../services/course.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  heroUserGroup,
  heroUser,
  heroUsers,
  heroPlus,
  heroPencil,
  heroTrash,
} from '@ng-icons/heroicons/outline';

@Component({
  selector: 'app-groups',
  imports: [CommonModule, ReactiveFormsModule, NgIconComponent],
  providers: [provideIcons({ heroUserGroup, heroUser, heroUsers, heroPlus, heroPencil, heroTrash })],
  templateUrl: './groups.html',
  styleUrl: './groups.scss',
})
export class Groups implements OnInit, OnDestroy {
  private destroyRef = inject(DestroyRef);

  groups: Group[] = [];
  teachers: Teacher[] = [];
  courses: Course[] = [];
  ageGroups: AgeGroup[] = [];
  loading = false;
  error: string | null = null;
  courseNameFilter: string | null = null;

  // Teacher search autocomplete
  teacherSearchText = '';
  teacherSuggestions: Teacher[] = [];
  showTeacherSuggestions = false;
  selectedTeacher: Teacher | null = null;
  teacherSearchLoading = false;
  private teacherSearchSubject = new Subject<string>();
  private subscriptions: Subscription[] = [];

  // Teacher tooltip on group cards
  hoveredTeacher: Teacher | null = null;
  tooltipStyle: { top: string; left: string } = { top: '0px', left: '0px' };

  showModal = false;
  isEditMode = false;
  currentGroupId: string | null = null;
  modalError: string | null = null;

  groupForm: FormGroup;

  showDeleteConfirm = false;
  groupToDelete: Group | null = null;

  showCourseModal = false;
  courseModalGroupId: string | null = null;
  courseModalGroupName: string | null = null;
  courseModalError: string | null = null;
  courseModalFilteredCourses: Course[] = [];
  courseForm: FormGroup;

  showStatusModal = false;
  statusModalGroupId: string | null = null;
  statusModalCourse: GroupCourseAssignment | null = null;
  statusModalError: string | null = null;
  statusModalTransitions: { value: number; label: string }[] = [];
  statusForm: FormGroup;

  readonly statusLabels: Record<number, string> = {
    0: 'Planned',
    1: 'Active',
    2: 'Completed',
    3: 'Cancelled'
  };

  constructor(
    private groupService: GroupService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.groupForm = this.fb.group({
      name: ['', [Validators.required]],
      teacherId: [null, [Validators.required]],
      ageGroupID: [null, [Validators.required]]
    });
    this.courseForm = this.fb.group({
      courseId: [null, [Validators.required]],
      status: [0, [Validators.required]]
    });
    this.statusForm = this.fb.group({
      status: [null, [Validators.required]]
    });
  }

  ngOnInit() {
    this.loadCourses();
    this.loadAgeGroups();

    this.subscriptions.push(
      this.teacherSearchSubject.pipe(
        debounceTime(300),
        distinctUntilChanged(),
        tap(() => {
          this.teacherSearchLoading = true;
          this.cdr.detectChanges();
        }),
        switchMap(search => this.groupService.searchTeachers(search))
      ).subscribe({
        next: (teachers) => {
          this.teacherSuggestions = teachers;
          this.showTeacherSuggestions = true;
          this.teacherSearchLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.teacherSuggestions = [];
          this.showTeacherSuggestions = false;
          this.teacherSearchLoading = false;
          this.cdr.detectChanges();
        }
      })
    );

    this.route.queryParams
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const courseName = params['courseName'] ?? null;
        this.courseNameFilter = courseName;
        this.loadGroups();
    });
  }

  ngOnDestroy() {
    this.subscriptions.forEach(s => s.unsubscribe());
    this.teacherSearchSubject.complete();
  }

  loadGroups() {
    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();

    const source$ = this.courseNameFilter
      ? this.groupService.getGroupsByCourseName(this.courseNameFilter)
      : this.groupService.getAllGroups();

    source$.subscribe({
      next: (groups) => {
        this.groups = groups;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to load groups. Please try again.';
        this.loading = false;
        console.error('Error loading groups:', error);
        this.cdr.detectChanges();
      }
    });
  }

  onTeacherSearchInput(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.teacherSearchText = value;
    if (this.selectedTeacher) {
      this.selectedTeacher = null;
      this.groupForm.patchValue({ teacherId: null });
    }
    if (value.trim().length >= 2) {
      this.teacherSearchSubject.next(value.trim());
    } else {
      this.teacherSuggestions = [];
      this.showTeacherSuggestions = false;
      this.teacherSearchLoading = false;
      this.cdr.detectChanges();
    }
  }

  selectTeacher(teacher: Teacher) {
    this.selectedTeacher = teacher;
    this.teacherSearchText = `${teacher.firstName} ${teacher.lastName}`;
    this.groupForm.patchValue({ teacherId: teacher.id });
    this.showTeacherSuggestions = false;
    this.teacherSuggestions = [];
    this.cdr.detectChanges();
  }

  clearTeacherSelection() {
    this.selectedTeacher = null;
    this.teacherSearchText = '';
    this.groupForm.patchValue({ teacherId: null });
    this.teacherSuggestions = [];
    this.showTeacherSuggestions = false;
    this.cdr.detectChanges();
  }

  onTeacherSearchBlur() {
    setTimeout(() => {
      this.showTeacherSuggestions = false;
      this.cdr.detectChanges();
    }, 200);
  }

  onTeacherSearchFocus() {
    if (this.teacherSuggestions.length > 0 && !this.selectedTeacher) {
      this.showTeacherSuggestions = true;
      this.cdr.detectChanges();
    }
  }

  // Teacher tooltip on group cards
  showTeacherTooltip(event: MouseEvent, group: Group) {
    if (group.teacher) {
      this.hoveredTeacher = group.teacher;
      const target = event.target as HTMLElement;
      const rect = target.getBoundingClientRect();
      this.tooltipStyle = {
        top: `${rect.bottom + window.scrollY + 8}px`,
        left: `${rect.left + window.scrollX}px`
      };
      this.cdr.detectChanges();
    }
  }

  hideTeacherTooltip() {
    this.hoveredTeacher = null;
    this.cdr.detectChanges();
  }

  loadCourses() {
    this.groupService.getAllCourses().subscribe({
      next: (courses) => {
        this.courses = courses;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading courses:', error);
      }
    });
  }

  loadAgeGroups() {
    this.groupService.getAllAgeGroups().subscribe({
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
    this.currentGroupId = null;
    this.modalError = null;
    this.teacherSearchText = '';
    this.selectedTeacher = null;
    this.teacherSuggestions = [];
    this.showTeacherSuggestions = false;
    this.groupForm.reset({
      name: '',
      teacherId: null,
      ageGroupID: null
    });
    this.showModal = true;
  }

  openEditModal(group: Group) {
    this.isEditMode = true;
    this.currentGroupId = group.id;
    this.modalError = null;
    this.selectedTeacher = group.teacher ?? null;
    this.teacherSearchText = group.teacher
      ? `${group.teacher.firstName} ${group.teacher.lastName}`
      : '';
    this.teacherSuggestions = [];
    this.showTeacherSuggestions = false;
    this.groupForm.reset({
      name: group.name,
      teacherId: group.teacherId,
      ageGroupID: group.ageGroupID
    });
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.isEditMode = false;
    this.currentGroupId = null;
    this.modalError = null;
    this.teacherSearchText = '';
    this.selectedTeacher = null;
    this.teacherSuggestions = [];
    this.showTeacherSuggestions = false;
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: Event) {
    if (this.showModal) {
      this.closeModal();
    }
    if (this.showDeleteConfirm) {
      this.cancelDelete();
    }
    if (this.showCourseModal) {
      this.closeCourseModal();
    }
    if (this.showStatusModal) {
      this.closeStatusModal();
    }
  }

  saveGroup() {
    this.modalError = null;

    if (this.groupForm.invalid) {
      this.groupForm.markAllAsTouched();
      this.modalError = this.getFormErrorMessage();
      this.cdr.detectChanges();
      return;
    }

    const groupPayload = this.groupForm.getRawValue() as CreateGroupDto | UpdateGroupDto;

    if (this.isEditMode && this.currentGroupId) {
      this.groupService.updateGroup(this.currentGroupId, groupPayload).subscribe({
        next: () => {
          this.loadGroups();
          this.closeModal();
        },
        error: (error) => {
          this.modalError = 'Failed to update group. Please check your input and try again.';
          console.error('Error updating group:', error);
          this.cdr.detectChanges();
        }
      });
    } else {
      this.groupService.createGroup(groupPayload).subscribe({
        next: () => {
          this.loadGroups();
          this.closeModal();
        },
        error: (error) => {
          this.modalError = 'Failed to create group. Please check your input and try again.';
          console.error('Error creating group:', error);
          this.cdr.detectChanges();
        }
      });
    }
  }

  confirmDelete(group: Group) {
    this.groupToDelete = group;
    this.showDeleteConfirm = true;
  }

  cancelDelete() {
    this.groupToDelete = null;
    this.showDeleteConfirm = false;
  }

  deleteGroup() {
    if (this.groupToDelete) {
      this.groupService.deleteGroup(this.groupToDelete.id).subscribe({
        next: () => {
          this.loadGroups();
          this.cancelDelete();
        },
        error: (error) => {
          this.error = 'Failed to delete group. Please try again.';
          console.error('Error deleting group:', error);
          this.cancelDelete();
        }
      });
    }
  }

  getTeacherName(group: Group): string {
    if (group.teacher) {
      const firstName = group.teacher.firstName ?? '';
      const lastName = group.teacher.lastName ?? '';
      return `${firstName} ${lastName}`.trim();
    }
    return 'Unassigned';
  }

  getAgeGroupLabel(group: Group): string {
    const ag = group.ageGroup || this.ageGroups.find(a => a.id === group.ageGroupID);
    if (ag) {
      const label = ag.label || ag.name;
      return `${label} (${ag.minAge}-${ag.maxAge})`;
    }
    return 'Unknown';
  }

  getStatusLabel(status: number): string {
    return this.statusLabels[status] ?? 'Unknown';
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 0: return 'status-planned';
      case 1: return 'status-active';
      case 2: return 'status-completed';
      case 3: return 'status-cancelled';
      default: return '';
    }
  }

  getAvailableTransitions(currentStatus: number): { value: number; label: string }[] {
    switch (currentStatus) {
      case 0: return [{ value: 1, label: 'Active' }, { value: 3, label: 'Cancelled' }];
      case 1: return [{ value: 2, label: 'Completed' }, { value: 3, label: 'Cancelled' }];
      default: return [];
    }
  }

  // Course assignment modal
  openCourseModal(group: Group) {
    this.courseModalGroupId = group.id;
    this.courseModalGroupName = group.name;
    this.courseModalError = null;
    this.courseModalFilteredCourses = this.courses.filter(c => c.ageGroupID === group.ageGroupID);
    this.courseForm.reset({
      courseId: null,
      status: 0
    });
    this.showCourseModal = true;
  }

  closeCourseModal() {
    this.showCourseModal = false;
    this.courseModalGroupId = null;
    this.courseModalGroupName = null;
    this.courseModalError = null;
  }

  assignCourse() {
    this.courseModalError = null;

    if (this.courseForm.invalid) {
      this.courseForm.markAllAsTouched();
      this.courseModalError = 'Please select a course and status.';
      this.cdr.detectChanges();
      return;
    }

    const { courseId, status } = this.courseForm.getRawValue();

    this.groupService.assignCourse(this.courseModalGroupId!, courseId, { status }).subscribe({
      next: () => {
        this.loadGroups();
        this.closeCourseModal();
      },
      error: (error) => {
        const errorStatus = error?.status as number | undefined;
        if (errorStatus === 409) {
          this.courseModalError = 'This course is already assigned or its age group does not match.';
        } else {
          this.courseModalError = 'Failed to assign course. Please try again.';
        }
        console.error('Error assigning course:', error);
        this.cdr.detectChanges();
      }
    });
  }

  unassignCourse(group: Group, assignment: GroupCourseAssignment) {
    this.groupService.unassignCourse(group.id, assignment.courseId).subscribe({
      next: () => {
        this.loadGroups();
      },
      error: (error) => {
        this.error = 'Failed to unassign course. Please try again.';
        console.error('Error unassigning course:', error);
        this.cdr.detectChanges();
      }
    });
  }

  // Status update modal
  openStatusModal(group: Group, assignment: GroupCourseAssignment) {
    this.statusModalGroupId = group.id;
    this.statusModalCourse = assignment;
    this.statusModalError = null;
    this.statusModalTransitions = this.getAvailableTransitions(assignment.status);
    this.statusForm.reset({
      status: null
    });
    this.showStatusModal = true;
  }

  closeStatusModal() {
    this.showStatusModal = false;
    this.statusModalGroupId = null;
    this.statusModalCourse = null;
    this.statusModalError = null;
  }

  updateCourseStatus() {
    this.statusModalError = null;

    if (this.statusForm.invalid) {
      this.statusForm.markAllAsTouched();
      this.statusModalError = 'Please select a status.';
      this.cdr.detectChanges();
      return;
    }

    const { status } = this.statusForm.getRawValue();

    this.groupService.updateCourseStatus(this.statusModalGroupId!, this.statusModalCourse!.courseId, { status }).subscribe({
      next: () => {
        this.loadGroups();
        this.closeStatusModal();
      },
      error: (error) => {
        this.statusModalError = 'Failed to update course status. Please try again.';
        console.error('Error updating course status:', error);
        this.cdr.detectChanges();
      }
    });
  }

  private getFormErrorMessage(): string | null {
    const controls = this.groupForm.controls;

    if (controls['name']?.errors?.['required']) {
      return 'Group name is required';
    }

    if (controls['teacherId']?.errors?.['required']) {
      return 'A teacher must be assigned to the group';
    }

    if (controls['ageGroupID']?.errors?.['required']) {
      return 'An age group must be assigned to the group';
    }

    return 'Please fix the errors in the form.';
  }

  navigateToCourseDetails(courseId: number) {
    this.router.navigate(['/courses'], {
      queryParams: { courseId: courseId }
    });
  }

  clearCourseFilter() {
    this.courseNameFilter = null;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {},
      replaceUrl: true
    });
  }
}
