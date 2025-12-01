import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Course {
  id: number;
  name: string;
  description: string;
  ageGroupID: number;
}

export interface CreateCourseCommand {
  name: string;
  description: string;
  ageGroupID: number;
}

export interface UpdateCourseCommand {
  name: string;
  description: string;
  ageGroupID: number;
}

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private apiUrl = '/Courses';

  constructor(private http: HttpClient) {}

  getAllCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(this.apiUrl);
  }

  getCourseById(id: number): Observable<Course> {
    return this.http.get<Course>(`${this.apiUrl}/${id}`);
  }

  createCourse(command: CreateCourseCommand): Observable<number> {
    return this.http.post<number>(this.apiUrl, command);
  }

  updateCourse(id: number, command: UpdateCourseCommand): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, command);
  }

  deleteCourse(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
