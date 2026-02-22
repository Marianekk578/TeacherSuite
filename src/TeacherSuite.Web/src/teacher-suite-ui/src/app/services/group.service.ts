import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Teacher } from './teacher.service';
import { Course } from './course.service';

export interface GroupCourseAssignment {
  courseId: number;
  courseName: string;
  status: number;
  startDate: string;
  endDate?: string;
}

export interface Group {
  id: string;
  name: string;
  teacherId: string;
  ageGroupID: number;
  teacher?: Teacher;
  courses: GroupCourseAssignment[];
}

export interface CreateGroupDto {
  name: string;
  teacherId: string;
}

export interface UpdateGroupDto {
  name: string;
  teacherId: string;
}

export interface AssignCourseDto {
  status: number;
}

export interface UpdateCourseStatusDto {
  status: number;
}

@Injectable({
  providedIn: 'root',
})
export class GroupService extends ApiService {
  private readonly apiUrl = '/Groups';
  private readonly teacherUrl = '/Teachers';
  private readonly courseUrl = '/Courses';

  getAllGroups(): Observable<Group[]> {
    return this.get<Group[]>(this.apiUrl);
  }

  getGroupById(id: string): Observable<Group> {
    return this.get<Group>(`${this.apiUrl}/${id}`);
  }

  createGroup(group: CreateGroupDto): Observable<string> {
    return this.post<string>(this.apiUrl, group);
  }

  updateGroup(id: string, group: UpdateGroupDto): Observable<void> {
    return this.put(`${this.apiUrl}/${id}`, group);
  }

  deleteGroup(id: string): Observable<void> {
    return this.delete(`${this.apiUrl}/${id}`);
  }

  assignCourse(groupId: string, courseId: number, data: AssignCourseDto): Observable<void> {
    return this.put(`${this.apiUrl}/${groupId}/courses/${courseId}`, data);
  }

  unassignCourse(groupId: string, courseId: number): Observable<void> {
    return this.delete(`${this.apiUrl}/${groupId}/courses/${courseId}`);
  }

  updateCourseStatus(groupId: string, courseId: number, data: UpdateCourseStatusDto): Observable<void> {
    return this.patch(`${this.apiUrl}/${groupId}/courses/${courseId}/status`, data);
  }

  getAllTeachers(): Observable<Teacher[]> {
    return this.get<Teacher[]>(this.teacherUrl);
  }

  getAllCourses(): Observable<Course[]> {
    return this.get<Course[]>(this.courseUrl);
  }
}
