import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from './base-http.service';

export interface Teacher {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
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
export class TeacherService extends BaseHttpService {
  protected readonly baseUrl = '/Teachers';

  getAllTeachers(): Observable<Teacher[]> {
    return this.get<Teacher[]>(this.baseUrl);
  }

  createTeacher(teacher: CreateTeacherDto): Observable<string> {
    const teacherData = {
      ...teacher,
      dateOfBirth: this.convertToUtcIsoString(teacher.dateOfBirth)
    };
    
    return this.post<CreateTeacherDto, string>(this.baseUrl, teacherData);
  }

  updateTeacher(id: string, teacher: UpdateTeacherDto): Observable<void> {
    const teacherData = {
      ...teacher,
      dateOfBirth: this.convertToUtcIsoString(teacher.dateOfBirth)
    };
    
    return this.put<UpdateTeacherDto>(`${this.baseUrl}/${id}`, teacherData);
  }

  deleteTeacher(id: string): Observable<void> {
    return this.delete(`${this.baseUrl}/${id}`);
  }
}
