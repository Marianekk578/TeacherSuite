import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';

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
export class TeacherService {
  private readonly apiUrl = '/Teachers';

  getAllTeachers(): Observable<Teacher[]> {
    return from(
      fetch(this.apiUrl)
        .then((response) => {
          if (!response.ok) {
            throw new Error(`Failed to fetch teachers: ${response.status} ${response.statusText}`);
          }
          return response.json();
        })
        .catch((error) => {
          throw error;
        })
    );
  }

  createTeacher(teacher: CreateTeacherDto): Observable<string> {
    const teacherData = {
      ...teacher,
      dateOfBirth: this.convertToUtcIsoString(teacher.dateOfBirth)
    };
    
    return from(
      fetch(this.apiUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(teacherData),
      })
        .then((response) => {
          if (!response.ok) {
            throw new Error(`Failed to create teacher: ${response.status} ${response.statusText}`);
          }
          return response.json();
        })
        .catch((error) => {
          throw error;
        })
    );
  }

  updateTeacher(id: string, teacher: UpdateTeacherDto): Observable<void> {
    const teacherData = {
      ...teacher,
      dateOfBirth: this.convertToUtcIsoString(teacher.dateOfBirth)
    };
    
    return from(
      fetch(`${this.apiUrl}/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(teacherData),
      })
        .then((response) => {
          if (!response.ok) {
            throw new Error(`Failed to update teacher: ${response.status} ${response.statusText}`);
          }
        })
        .catch((error) => {
          throw error;
        })
    );
  }

  deleteTeacher(id: string): Observable<void> {
    return from(
      fetch(`${this.apiUrl}/${id}`, {
        method: 'DELETE',
      })
        .then((response) => {
          if (!response.ok) {
            throw new Error(`Failed to delete teachers: ${response.status} ${response.statusText}`);
          }
        })
        .catch((error) => {
          throw error;
        })
    );
  }

  private convertToUtcIsoString(dateString: string): string {
    // dateString is in YYYY-MM-DD format from the date input
    // Create a date at midnight UTC to avoid timezone issues
    const date = new Date(dateString + 'T00:00:00.000Z');
    return date.toISOString();
  }
}
