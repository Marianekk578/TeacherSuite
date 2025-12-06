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
            throw new Error('Failed to fetch teachers');
          }
          return response.json();
        })
        .catch((error) => {
          console.error('Error fetching teachers:', error);
          throw error;
        })
    );
  }

  createTeacher(teacher: CreateTeacherDto): Observable<string> {
    return from(
      fetch(this.apiUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(teacher),
      })
        .then((response) => {
          if (!response.ok) {
            throw new Error('Failed to create teacher');
          }
          return response.json();
        })
        .catch((error) => {
          console.error('Error creating teacher:', error);
          throw error;
        })
    );
  }

  updateTeacher(id: string, teacher: UpdateTeacherDto): Observable<void> {
    return from(
      fetch(`${this.apiUrl}/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(teacher),
      })
        .then((response) => {
          if (!response.ok) {
            throw new Error('Failed to update teacher');
          }
        })
        .catch((error) => {
          console.error('Error updating teacher:', error);
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
            throw new Error('Failed to delete teacher');
          }
        })
        .catch((error) => {
          console.error('Error deleting teacher:', error);
          throw error;
        })
    );
  }
}
