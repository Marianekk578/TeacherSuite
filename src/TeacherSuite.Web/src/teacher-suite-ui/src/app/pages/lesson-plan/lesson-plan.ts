import { Component, OnInit, ChangeDetectorRef, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LessonPlanService, ScheduledLesson, StudentAttendance } from '../../services/lesson-plan.service';
import { LessonService, Lesson } from '../../services/lesson.service';
import { CourseService, Course } from '../../services/course.service';
import { GroupService, Group } from '../../services/group.service';
import { KeycloakService } from '../../auth/keycloak.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  heroCalendarDays,
  heroPlus,
  heroClock,
  heroUserGroup,
  heroBookOpen,
  heroEye,
  heroCheck,
  heroXMark,
  heroChevronRight,
} from '@ng-icons/heroicons/outline';

@Component({
  selector: 'app-lesson-plan',
  imports: [CommonModule, ReactiveFormsModule, NgIconComponent],
  providers: [
    provideIcons({
      heroCalendarDays,
      heroPlus,
      heroClock,
      heroUserGroup,
      heroBookOpen,
      heroEye,
      heroCheck,
      heroXMark,
      heroChevronRight,
    }),
  ],
  templateUrl: './lesson-plan.html',
  styleUrl: './lesson-plan.scss',
})
export class LessonPlanPage implements OnInit {
  private destroyRef = inject(DestroyRef);

  scheduledLessons: ScheduledLesson[] = [];
  loading = true;
  error = '';
  success = '';

  // Schedule modal
  showScheduleModal = false;
  scheduleForm: FormGroup;
  courses: Course[] = [];
  lessons: Lesson[] = [];
  groups: { id: string; name: string }[] = [];
  loadingCourses = false;
  loadingLessons = false;
  loadingGroups = false;

  // Attendance modal
  showAttendanceModal = false;
  selectedScheduledLesson: ScheduledLesson | null = null;
  studentAttendances: StudentAttendance[] = [];
  loadingAttendance = false;

  isAdminOrSupervisor = false;

  constructor(
    private lessonPlanService: LessonPlanService,
    private lessonService: LessonService,
    private courseService: CourseService,
    private groupService: GroupService,
    private keycloak: KeycloakService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    this.isAdminOrSupervisor = this.keycloak.hasRole('Admin') || this.keycloak.hasRole('Supervisor');

    this.scheduleForm = this.fb.group({
      courseId: ['', [Validators.required]],
      lessonId: ['', [Validators.required]],
      groupId: ['', [Validators.required]],
      scheduledStart: ['', [Validators.required]],
    });
  }

  ngOnInit(): void {
    this.loadLessonPlan();
  }

  loadLessonPlan(): void {
    this.loading = true;
    this.error = '';

    this.lessonPlanService.getLessonPlan()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.scheduledLessons = data;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.error = err.detail || 'Failed to load lesson plan';
          this.loading = false;
          this.cdr.detectChanges();
        },
      });
  }

  getLessonStatus(lesson: ScheduledLesson): 'upcoming' | 'active' | 'completed' {
    const now = new Date();
    const start = new Date(lesson.scheduledStart);
    const end = new Date(lesson.scheduledEnd);

    if (now < start) return 'upcoming';
    if (now >= start && now <= end) return 'active';
    return 'completed';
  }

  getStatusLabel(lesson: ScheduledLesson): string {
    const status = this.getLessonStatus(lesson);
    switch (status) {
      case 'upcoming': return 'Upcoming';
      case 'active': return 'In Progress';
      case 'completed': return 'Completed';
    }
  }

  navigateToLesson(lessonId: number): void {
    this.router.navigate(['/lessons', lessonId]);
  }

  // Schedule modal
  openScheduleModal(): void {
    this.showScheduleModal = true;
    this.scheduleForm.reset();
    this.lessons = [];
    this.groups = [];
    this.loadCourses();
    this.cdr.detectChanges();
  }

  closeScheduleModal(): void {
    this.showScheduleModal = false;
    this.cdr.detectChanges();
  }

  loadCourses(): void {
    this.loadingCourses = true;
    this.courseService.getAllCourses({ page: 1, pageSize: 100 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.courses = result.items;
          this.loadingCourses = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.loadingCourses = false;
          this.cdr.detectChanges();
        },
      });
  }

  onCourseChange(): void {
    const courseId = +this.scheduleForm.get('courseId')?.value;
    if (!courseId) {
      this.lessons = [];
      this.groups = [];
      return;
    }

    this.loadingLessons = true;
    this.loadingGroups = true;
    this.scheduleForm.patchValue({ lessonId: '', groupId: '' });

    this.lessonService.getLessonsByCourse(courseId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.lessons = data;
          this.loadingLessons = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.loadingLessons = false;
          this.cdr.detectChanges();
        },
      });

    this.lessonService.getCourseGroups(courseId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.groups = data;
          this.loadingGroups = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.loadingGroups = false;
          this.cdr.detectChanges();
        },
      });
  }

  scheduleLesson(): void {
    if (this.scheduleForm.invalid) return;

    const val = this.scheduleForm.value;
    this.lessonPlanService.createScheduledLesson({
      lessonId: +val.lessonId,
      groupId: val.groupId,
      scheduledStart: new Date(val.scheduledStart).toISOString(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.success = 'Lesson scheduled successfully';
          this.closeScheduleModal();
          this.loadLessonPlan();
          setTimeout(() => { this.success = ''; this.cdr.detectChanges(); }, 3000);
        },
        error: (err) => {
          this.error = err.detail || 'Failed to schedule lesson';
          this.cdr.detectChanges();
          setTimeout(() => { this.error = ''; this.cdr.detectChanges(); }, 5000);
        },
      });
  }

  // Attendance modal
  openAttendanceModal(lesson: ScheduledLesson): void {
    this.selectedScheduledLesson = lesson;
    this.showAttendanceModal = true;
    this.loadingAttendance = true;
    this.studentAttendances = [];

    this.lessonPlanService.getScheduledLessonStudents(lesson.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.studentAttendances = data;
          this.loadingAttendance = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.error = err.detail || 'Failed to load students';
          this.loadingAttendance = false;
          this.cdr.detectChanges();
        },
      });
  }

  closeAttendanceModal(): void {
    this.showAttendanceModal = false;
    this.selectedScheduledLesson = null;
    this.cdr.detectChanges();
  }

  toggleAttendance(student: StudentAttendance): void {
    if (!this.selectedScheduledLesson) return;

    const newValue = !student.isPresent;
    this.lessonPlanService.toggleStudentAttendance(this.selectedScheduledLesson.id, {
      studentId: student.studentId,
      isPresent: newValue,
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          student.isPresent = newValue;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.error = err.detail || 'Failed to update attendance';
          this.cdr.detectChanges();
          setTimeout(() => { this.error = ''; this.cdr.detectChanges(); }, 5000);
        },
      });
  }

  formatDateTime(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return 'Invalid Date';
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      timeZone: 'UTC',
    });
  }

  formatTime(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return 'Invalid';
    return date.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
      timeZone: 'UTC',
    });
  }
}
