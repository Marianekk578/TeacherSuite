import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CourseService, Course, CreateCourseCommand, UpdateCourseCommand } from '../../services/course.service';
import { AgeGroupService, AgeGroup } from '../../services/age-group.service';

@Component({
  selector: 'app-courses',
  imports: [CommonModule, FormsModule],
  templateUrl: './courses.html',
  styleUrl: './courses.scss',
})
export class Courses implements OnInit {
  courses = signal<Course[]>([]);
  ageGroups = signal<AgeGroup[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  
  showModal = signal(false);
  editMode = signal(false);
  currentCourseId = signal<number | null>(null);
  
  formData = signal({
    name: '',
    description: '',
    ageGroupID: 0
  });

  constructor(
    private courseService: CourseService,
    private ageGroupService: AgeGroupService
  ) {}

  ngOnInit() {
    this.loadCourses();
    this.loadAgeGroups();
  }

  loadCourses() {
    this.loading.set(true);
    this.error.set(null);
    this.courseService.getAllCourses().subscribe({
      next: (data) => {
        this.courses.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load courses');
        this.loading.set(false);
        console.error('Error loading courses:', err);
      }
    });
  }

  loadAgeGroups() {
    this.ageGroupService.getAllAgeGroups().subscribe({
      next: (data) => {
        this.ageGroups.set(data);
      },
      error: (err) => {
        console.error('Error loading age groups:', err);
      }
    });
  }

  openCreateModal() {
    this.editMode.set(false);
    this.currentCourseId.set(null);
    this.formData.set({
      name: '',
      description: '',
      ageGroupID: this.ageGroups()[0]?.id || 0
    });
    this.showModal.set(true);
  }

  openEditModal(course: Course) {
    this.editMode.set(true);
    this.currentCourseId.set(course.id);
    this.formData.set({
      name: course.name,
      description: course.description,
      ageGroupID: course.ageGroupID
    });
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  submitForm() {
    const data = this.formData();
    
    if (this.editMode() && this.currentCourseId()) {
      const command: UpdateCourseCommand = {
        name: data.name,
        description: data.description,
        ageGroupID: data.ageGroupID
      };
      
      this.courseService.updateCourse(this.currentCourseId()!, command).subscribe({
        next: () => {
          this.closeModal();
          this.loadCourses();
        },
        error: (err) => {
          this.error.set('Failed to update course');
          console.error('Error updating course:', err);
        }
      });
    } else {
      const command: CreateCourseCommand = {
        name: data.name,
        description: data.description,
        ageGroupID: data.ageGroupID
      };
      
      this.courseService.createCourse(command).subscribe({
        next: () => {
          this.closeModal();
          this.loadCourses();
        },
        error: (err) => {
          this.error.set('Failed to create course');
          console.error('Error creating course:', err);
        }
      });
    }
  }

  deleteCourse(id: number) {
    if (confirm('Are you sure you want to delete this course?')) {
      this.courseService.deleteCourse(id).subscribe({
        next: () => {
          this.loadCourses();
        },
        error: (err) => {
          this.error.set('Failed to delete course');
          console.error('Error deleting course:', err);
        }
      });
    }
  }

  getAgeGroupName(ageGroupID: number): string {
    return this.ageGroups().find(ag => ag.id === ageGroupID)?.name || 'Unknown';
  }
}

