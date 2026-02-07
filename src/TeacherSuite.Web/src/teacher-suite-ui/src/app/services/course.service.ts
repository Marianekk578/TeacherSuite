import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from './base-http.service';

export interface Course {
  id: number;
  name: string;
  description: string;
  ageGroupID: number;
}

export interface CreateCourseDto {
  name: string;
  description: string;
  ageGroupID: number;
}

export interface UpdateCourseDto {
  name: string;
  description: string;
  ageGroupID: number;
}

@Injectable({
  providedIn: 'root',
})
export class CourseService extends BaseHttpService {
  protected readonly baseUrl = '/Courses';

  getAllCourses(): Observable<Course[]> {
    return this.get<Course[]>(this.baseUrl);
  }

  getCourseById(id: number): Observable<Course> {
    return this.get<Course>(`${this.baseUrl}/${id}`);
  }

  createCourse(course: CreateCourseDto): Observable<number> {
    return this.post<CreateCourseDto, number>(this.baseUrl, course);
  }

  updateCourse(id: number, course: UpdateCourseDto): Observable<void> {
    return this.put<UpdateCourseDto>(`${this.baseUrl}/${id}`, course);
  }

  deleteCourse(id: number): Observable<void> {
    return this.delete(`${this.baseUrl}/${id}`);
  }
}
