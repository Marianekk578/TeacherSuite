import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface ScheduledLesson {
  id: string;
  lessonId: number;
  groupId: string;
  groupName: string;
  lessonTitle: string;
  courseName: string;
  courseId: number;
  lessonOrder: number;
  scheduledStart: string;
  scheduledEnd: string;
  hasAttendance: boolean;
}

export interface StudentAttendance {
  id: string;
  studentId: string;
  studentFirstName: string;
  studentLastName: string;
  isPresent: boolean;
}

export interface CreateScheduledLessonDto {
  lessonId: number;
  groupId: string;
  scheduledStart: string;
}

export interface ToggleAttendanceDto {
  studentId: string;
  isPresent: boolean;
}

export interface SaveAttendanceDto {
  attendances: { studentId: string; isPresent: boolean }[];
}

@Injectable({
  providedIn: 'root',
})
export class LessonPlanService extends ApiService {
  private readonly apiUrl = '/LessonPlan';

  getLessonPlan(from?: string, to?: string): Observable<ScheduledLesson[]> {
    const params = new URLSearchParams();
    if (from) params.append('from', from);
    if (to) params.append('to', to);
    const query = params.toString();
    return this.get<ScheduledLesson[]>(`${this.apiUrl}${query ? '?' + query : ''}`);
  }

  createScheduledLesson(dto: CreateScheduledLessonDto): Observable<string> {
    return this.post<string>(this.apiUrl, dto);
  }

  getScheduledLessonStudents(scheduledLessonId: string): Observable<StudentAttendance[]> {
    return this.get<StudentAttendance[]>(`${this.apiUrl}/${scheduledLessonId}/students`);
  }

  toggleStudentAttendance(scheduledLessonId: string, dto: ToggleAttendanceDto): Observable<string> {
    return this.post<string>(`${this.apiUrl}/${scheduledLessonId}/attendance`, dto);
  }

  saveAttendance(scheduledLessonId: string, dto: SaveAttendanceDto): Observable<void> {
    return this.post<void>(`${this.apiUrl}/${scheduledLessonId}/attendance/save`, dto);
  }
}
