import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Teacher {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
  programmingLanguages: TeacherProgrammingLanguage[];
}

export interface TeacherProgrammingLanguage {
  id: number;
  name: string;
  label?: string;
  color?: string;
}

export interface CreateTeacherDto {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
}

export interface UpdateTeacherDto {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface TeacherQuery {
  search: string;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root',
})
export class TeacherService extends ApiService {
  private readonly apiUrl = '/Teachers';

  getAllTeachers(query: TeacherQuery): Observable<PagedResult<Teacher>> {
    const params = new URLSearchParams();
    if (query.search) {
      params.set('search', query.search);
    }
    params.set('page', String(query.page));
    params.set('pageSize', String(query.pageSize));

    return this.get<PagedResult<Teacher>>(`${this.apiUrl}?${params.toString()}`);
  }

  createTeacher(teacher: CreateTeacherDto): Observable<string> {
    const teacherData = {
      ...teacher,
      dateOfBirth: this.convertToUtcIsoString(teacher.dateOfBirth),
    };

    return this.post<string>(this.apiUrl, teacherData);
  }

  updateTeacher(id: string, teacher: UpdateTeacherDto): Observable<void> {
    const teacherData = {
      ...teacher,
      dateOfBirth: this.convertToUtcIsoString(teacher.dateOfBirth),
    };

    return this.put(`${this.apiUrl}/${id}`, teacherData);
  }

  deleteTeacher(id: string): Observable<void> {
    return this.delete(`${this.apiUrl}/${id}`);
  }

  seedTestTeachers(): Observable<number> {
    return this.post<number>(`${this.apiUrl}/seed-test`, {});
  }

  deleteTestTeachers(): Observable<number> {
    return this.post<number>(`${this.apiUrl}/delete-test`, {});
  }

  private convertToUtcIsoString(dateString: string): string {
    return `${dateString}T00:00:00Z`;
  }
}
