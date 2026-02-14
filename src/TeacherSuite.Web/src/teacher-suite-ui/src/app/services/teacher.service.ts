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

@Injectable({
  providedIn: 'root',
})
export class TeacherService extends ApiService {
  private readonly apiUrl = '/Teachers';

  getAllTeachers(): Observable<Teacher[]> {
    return this.get<Teacher[]>(this.apiUrl);
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

  private convertToUtcIsoString(dateString: string): string {
    const date = new Date(dateString + 'T00:00:00.000Z');
    return date.toISOString();
  }
}
