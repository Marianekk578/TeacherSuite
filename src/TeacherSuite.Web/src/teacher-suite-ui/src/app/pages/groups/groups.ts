import { Component, OnInit, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { GroupService, Group, CreateGroupDto, UpdateGroupDto, GroupCourseAssignment } from '../../services/group.service';
import { Teacher } from '../../services/teacher.service';
import { Course, AgeGroup } from '../../services/course.service';

@Component({
  selector: 'app-groups',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './groups.html',
  styleUrl: './groups.scss',
})
export class Groups implements OnInit {
  groups: Group[] = [];
  teachers: Teacher[] = [];
  courses: Course[] = [];
  ageGroups: AgeGroup[] = [];
  loading = false;
  error: string | null = null;

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
    private fb: FormBuilder
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
    this.loadGroups();
    this.loadTeachers();
    this.loadCourses();
    this.loadAgeGroups();
  }

  loadGroups() {
    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();

    this.groupService.getAllGroups().subscribe({
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

  loadTeachers() {
    this.groupService.getAllTeachers().subscribe({
      next: (teachers) => {
        this.teachers = teachers;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading teachers:', error);
      }
    });
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

  getAgeGroupName(group: Group): string {
    if (group.ageGroup) {
      return group.ageGroup.name;
    }
    const ag = this.ageGroups.find(a => a.id === group.ageGroupID);
    return ag ? ag.name : 'Unknown';
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
}
