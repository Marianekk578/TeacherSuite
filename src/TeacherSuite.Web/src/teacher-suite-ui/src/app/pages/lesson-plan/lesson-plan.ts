import { Component, OnInit, ChangeDetectorRef, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LessonPlanService, ScheduledLesson, StudentAttendance } from '../../services/lesson-plan.service';
import { LessonService, Lesson } from '../../services/lesson.service';
import { CourseService, Course } from '../../services/course.service';
import { GroupService } from '../../services/group.service';
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
  heroChevronLeft,
} from '@ng-icons/heroicons/outline';

export interface CalendarDay {
  date: Date;
  day: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  hasLesson: boolean;
}

export interface LessonGroup {
  label: string;
  lessons: ScheduledLesson[];
}

@Component({
  selector: 'app-lesson-plan',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NgIconComponent],
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
      heroChevronLeft,
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

  // View mode & period
  viewMode: 'weekly' | 'monthly' = 'weekly';
  currentDate = new Date();

  // Mini calendar state
  calendarYear = new Date().getFullYear();
  calendarMonth = new Date().getMonth();
  calendarDays: CalendarDay[] = [];
  lessonDateStrings = new Set<string>();
  readonly weekDayLabels = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su'];

  // Filters
  groupByMode: 'none' | 'course' | 'group' = 'none';
  filterCourse = '';
  filterGroup = '';
  availableCourses: string[] = [];
  availableGroups: string[] = [];

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
    this.buildCalendar();
    this.loadPeriodLessons();
  }

  // --- Period calculation ---

  get periodStart(): Date {
    if (this.viewMode === 'weekly') {
      return this.getMonday(this.currentDate);
    }
    const d = new Date(this.currentDate);
    return new Date(d.getFullYear(), d.getMonth(), 1);
  }

  get periodEnd(): Date {
    if (this.viewMode === 'weekly') {
      const monday = this.getMonday(this.currentDate);
      const sunday = new Date(monday);
      sunday.setDate(monday.getDate() + 6);
      sunday.setHours(23, 59, 59, 999);
      return sunday;
    }
    const d = new Date(this.currentDate);
    return new Date(d.getFullYear(), d.getMonth() + 1, 0, 23, 59, 59, 999);
  }

  get periodLabel(): string {
    if (this.viewMode === 'weekly') {
      const start = this.periodStart;
      const end = this.periodEnd;
      const startStr = start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
      const endStr = end.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
      return startStr + ' \u2013 ' + endStr;
    }
    return this.currentDate.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  }

  private getMonday(d: Date): Date {
    const date = new Date(d);
    const day = date.getDay();
    const diff = day === 0 ? -6 : 1 - day;
    date.setDate(date.getDate() + diff);
    date.setHours(0, 0, 0, 0);
    return date;
  }

  // --- Period navigation ---

  prevPeriod(): void {
    if (this.viewMode === 'weekly') {
      const d = new Date(this.currentDate);
      d.setDate(d.getDate() - 7);
      this.currentDate = d;
    } else {
      this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() - 1, 1);
    }
    this.loadPeriodLessons();
  }

  nextPeriod(): void {
    if (this.viewMode === 'weekly') {
      const d = new Date(this.currentDate);
      d.setDate(d.getDate() + 7);
      this.currentDate = d;
    } else {
      this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, 1);
    }
    this.loadPeriodLessons();
  }

  onViewModeChange(): void {
    this.loadPeriodLessons();
  }

  goToToday(): void {
    this.currentDate = new Date();
    this.calendarYear = this.currentDate.getFullYear();
    this.calendarMonth = this.currentDate.getMonth();
    this.buildCalendar();
    this.loadPeriodLessons();
  }

  // --- Data loading ---

  loadPeriodLessons(): void {
    this.loading = true;
    this.error = '';

    const from = this.periodStart.toISOString();
    const to = this.periodEnd.toISOString();

    this.lessonPlanService.getLessonPlan(from, to)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.scheduledLessons = data;
          this.updateDerivedData();
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

  private updateDerivedData(): void {
    const courseSet = new Set<string>();
    const groupSet = new Set<string>();
    this.lessonDateStrings.clear();

    for (const lesson of this.scheduledLessons) {
      courseSet.add(lesson.courseName);
      groupSet.add(lesson.groupName);
      const d = new Date(lesson.scheduledStart);
      this.lessonDateStrings.add(this.toDateKey(d));
    }

    this.availableCourses = Array.from(courseSet).sort();
    this.availableGroups = Array.from(groupSet).sort();
    this.buildCalendar();
  }

  // --- Filtering & Grouping ---

  get filteredLessons(): ScheduledLesson[] {
    let result = this.scheduledLessons;
    if (this.filterCourse) {
      result = result.filter(l => l.courseName === this.filterCourse);
    }
    if (this.filterGroup) {
      result = result.filter(l => l.groupName === this.filterGroup);
    }
    return result.sort((a, b) =>
      new Date(a.scheduledStart).getTime() - new Date(b.scheduledStart).getTime()
    );
  }

  get groupedLessons(): LessonGroup[] {
    const lessons = this.filteredLessons;
    if (this.groupByMode === 'none') {
      return [{ label: '', lessons }];
    }

    const map = new Map<string, ScheduledLesson[]>();
    for (const l of lessons) {
      const key = this.groupByMode === 'course' ? l.courseName : l.groupName;
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(l);
    }

    return Array.from(map.entries())
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([label, items]) => ({ label, lessons: items }));
  }

  // --- Mini calendar ---

  buildCalendar(): void {
    const year = this.calendarYear;
    const month = this.calendarMonth;
    const firstOfMonth = new Date(year, month, 1);
    const dow = firstOfMonth.getDay();
    const offset = dow === 0 ? -6 : 1 - dow;
    const startDay = new Date(year, month, 1 + offset);

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    this.calendarDays = [];
    const current = new Date(startDay);

    for (let i = 0; i < 42; i++) {
      const d = new Date(current);
      d.setHours(0, 0, 0, 0);
      this.calendarDays.push({
        date: new Date(d),
        day: d.getDate(),
        isCurrentMonth: d.getMonth() === month,
        isToday: d.getTime() === today.getTime(),
        hasLesson: this.lessonDateStrings.has(this.toDateKey(d)),
      });
      current.setDate(current.getDate() + 1);
    }
  }

  prevCalendarMonth(): void {
    if (this.calendarMonth === 0) {
      this.calendarMonth = 11;
      this.calendarYear--;
    } else {
      this.calendarMonth--;
    }
    this.buildCalendar();
  }

  nextCalendarMonth(): void {
    if (this.calendarMonth === 11) {
      this.calendarMonth = 0;
      this.calendarYear++;
    } else {
      this.calendarMonth++;
    }
    this.buildCalendar();
  }

  get calendarMonthLabel(): string {
    return new Date(this.calendarYear, this.calendarMonth, 1)
      .toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
  }

  onCalendarDayClick(day: CalendarDay): void {
    this.currentDate = new Date(day.date);
    this.calendarYear = day.date.getFullYear();
    this.calendarMonth = day.date.getMonth();
    this.loadPeriodLessons();
  }

  // --- Status helpers ---

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

  // --- Format ---

  private toDateKey(d: Date): string {
    return d.getFullYear() + '-' +
      String(d.getMonth() + 1).padStart(2, '0') + '-' +
      String(d.getDate()).padStart(2, '0');
  }

  formatScheduledDateTime(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return 'Invalid Date';
    const datePart = date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return datePart + ' ' + hours + ':' + minutes;
  }

  // --- Navigation ---

  navigateToLesson(lessonId: number): void {
    this.router.navigate(['/lessons', lessonId], { queryParams: { from: 'lesson-plan' } });
  }

  // --- Schedule modal ---

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
          this.loadPeriodLessons();
          setTimeout(() => { this.success = ''; this.cdr.detectChanges(); }, 3000);
        },
        error: (err) => {
          this.error = err.detail || 'Failed to schedule lesson';
          this.cdr.detectChanges();
          setTimeout(() => { this.error = ''; this.cdr.detectChanges(); }, 5000);
        },
      });
  }

  // --- Attendance modal ---

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
}
