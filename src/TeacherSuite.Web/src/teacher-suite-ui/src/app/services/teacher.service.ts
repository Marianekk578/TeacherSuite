import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Teacher {
  id: string;
  firstName: string;
  lastName: string;
}

export interface TeacherDetails extends Teacher {
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
}

export interface CreateTeacherCommand {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
}

export interface UpdateTeacherCommand {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
}

@Injectable({
  providedIn: 'root'
})
export class TeacherService {
  private apiUrl = '/Teachers';

  constructor(private http: HttpClient) {}

  getAllTeachers(): Observable<Teacher[]> {
    return this.http.get<Teacher[]>(this.apiUrl);
  }

  createTeacher(command: CreateTeacherCommand): Observable<string> {
    return this.http.post<string>(this.apiUrl, command);
  }

  updateTeacher(id: string, command: UpdateTeacherCommand): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, command);
  }
}
