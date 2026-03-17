import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { PagedResult } from '../models/paged-result.model';

export interface StudentGroup {
  groupId: string;
  groupName: string;
}

export interface Student {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  contactEmail: string;
  contactPhone: string;
  parentFirstName?: string;
  parentLastName?: string;
  groups: StudentGroup[];
}

export interface StudentCourseHistory {
  courseId: number;
  courseName: string;
  groupName: string;
  status: number;
  startDate: string;
  endDate?: string;
}

export interface StudentProgrammingLanguage {
  id: number;
  name: string;
  label?: string;
  color?: string;
}

export interface StudentDetailGroup {
  groupId: string;
  groupName: string;
  ageGroup?: {
    id: number;
    name: string;
    label?: string;
    minAge: number;
    maxAge: number;
  };
}

export interface StudentDetail {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  contactEmail: string;
  contactPhone: string;
  parentFirstName?: string;
  parentLastName?: string;
  groups: StudentDetailGroup[];
  courseHistory: StudentCourseHistory[];
  programmingLanguages: StudentProgrammingLanguage[];
}

export interface CreateStudentDto {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  contactEmail: string;
  contactPhone: string;
  parentFirstName?: string;
  parentLastName?: string;
  groupId?: string;
}

export interface UpdateStudentDto {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  contactEmail: string;
  contactPhone: string;
  parentFirstName?: string;
  parentLastName?: string;
}

export interface StudentQuery {
  search: string;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root',
})
export class StudentService extends ApiService {
  private readonly apiUrl = '/Students';
  private readonly groupUrl = '/Groups';

  getAllStudents(query: StudentQuery): Observable<PagedResult<Student>> {
    const params = new URLSearchParams();
    if (query.search) {
      params.set('search', query.search);
    }
    params.set('page', String(query.page));
    params.set('pageSize', String(query.pageSize));

    return this.get<PagedResult<Student>>(`${this.apiUrl}?${params.toString()}`);
  }

  getStudentById(id: string): Observable<StudentDetail> {
    return this.get<StudentDetail>(`${this.apiUrl}/${id}`);
  }

  createStudent(student: CreateStudentDto): Observable<string> {
    const studentData = {
      ...student,
      dateOfBirth: this.convertToUtcIsoString(student.dateOfBirth),
    };

    return this.post<string>(this.apiUrl, studentData);
  }

  updateStudent(id: string, student: UpdateStudentDto): Observable<void> {
    const studentData = {
      ...student,
      dateOfBirth: this.convertToUtcIsoString(student.dateOfBirth),
    };

    return this.put(`${this.apiUrl}/${id}`, studentData);
  }

  deleteStudent(id: string): Observable<void> {
    return this.delete(`${this.apiUrl}/${id}`);
  }

  assignToGroup(studentId: string, groupId: string): Observable<void> {
    return this.put(`${this.apiUrl}/${studentId}/groups/${groupId}`, {});
  }

  unassignFromGroup(studentId: string, groupId: string): Observable<void> {
    return this.delete(`${this.apiUrl}/${studentId}/groups/${groupId}`);
  }

  getAllGroups(): Observable<{ id: string; name: string; ageGroupID: number; ageGroup?: { id: number; name: string; label?: string; minAge: number; maxAge: number } }[]> {
    return this.get<{ id: string; name: string; ageGroupID: number; ageGroup?: { id: number; name: string; label?: string; minAge: number; maxAge: number } }[]>(this.groupUrl);
  }

  private convertToUtcIsoString(dateString: string): string {
    return `${dateString}T00:00:00Z`;
  }
}
