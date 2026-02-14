import { Component, OnInit, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { GroupService, Group, CreateGroupDto, UpdateGroupDto } from '../../services/group.service';
import { Teacher } from '../../services/teacher.service';
import { AgeGroup } from '../../services/course.service';

@Component({
  selector: 'app-groups',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './groups.html',
  styleUrl: './groups.scss',
})
export class Groups implements OnInit {
  groups: Group[] = [];
  teachers: Teacher[] = [];
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
  }

  ngOnInit() {
    this.loadGroups();
    this.loadTeachers();
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

  private getFormErrorMessage(): string | null {
    const controls = this.groupForm.controls;

    if (controls['name']?.errors?.['required']) {
      return 'Group name is required';
    }

    if (controls['teacherId']?.errors?.['required']) {
      return 'A teacher must be assigned to the group';
    }

    if (controls['ageGroupID']?.errors?.['required']) {
      return 'Age group is required';
    }

    return 'Please fix the errors in the form.';
  }
}
