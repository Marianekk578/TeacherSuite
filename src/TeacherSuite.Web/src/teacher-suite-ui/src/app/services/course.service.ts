import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { PagedResult } from './teacher.service';

export interface AgeGroup {
  id: number;
  name: string;
  minAge: number;
  maxAge: number;
}

export interface Course {
  id: number;
  name: string;
  description: string;
  ageGroupID: number;
  ageGroup?: AgeGroup;
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

export interface CourseQuery {
  search: string;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root',
})
export class CourseService extends ApiService {
  private readonly apiUrl = '/Courses';
  private readonly ageGroupUrl = '/AgeGroups';

  getAllCourses(query: CourseQuery): Observable<PagedResult<Course>> {
    const params = new URLSearchParams();
    if (query.search) {
      params.set('search', query.search);
    }
    params.set('page', String(query.page));
    params.set('pageSize', String(query.pageSize));

    return this.get<PagedResult<Course>>(`${this.apiUrl}?${params.toString()}`);
  }

  getCourseById(id: number): Observable<Course> {
    return this.get<Course>(`${this.apiUrl}/${id}`);
  }

  createCourse(course: CreateCourseDto): Observable<number> {
    return this.post<number>(this.apiUrl, course);
  }

  updateCourse(id: number, course: UpdateCourseDto): Observable<void> {
    return this.put(`${this.apiUrl}/${id}`, course);
  }

  deleteCourse(id: number): Observable<void> {
    return this.delete(`${this.apiUrl}/${id}`);
  }

  seedTestCourses(): Observable<number> {
    return this.post<number>(`${this.apiUrl}/seed-test`, {});
  }

  deleteTestCourses(): Observable<number> {
    return this.post<number>(`${this.apiUrl}/delete-test`, {});
  }

  getAllAgeGroups(): Observable<AgeGroup[]> {
    return this.get<AgeGroup[]>(this.ageGroupUrl);
  }
}
