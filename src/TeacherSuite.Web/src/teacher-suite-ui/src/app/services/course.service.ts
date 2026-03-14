import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { PagedResult } from './teacher.service';

export interface AgeGroup {
  id: number;
  name: string;
  label?: string;
  minAge: number;
  maxAge: number;
}

export interface ProgrammingLanguage {
  id: number;
  name: string;
  label?: string;
  color?: string;
}

export interface Course {
  id: number;
  name: string;
  description: string;
  ageGroupID: number;
  ageGroup?: AgeGroup;
  programmingLanguages: ProgrammingLanguage[];
}

export interface CreateCourseDto {
  name: string;
  description: string;
  ageGroupID: number;
  programmingLanguageIds?: number[];
}

export interface UpdateCourseDto {
  name: string;
  description: string;
  ageGroupID: number;
  programmingLanguageIds?: number[];
}

export interface CourseQuery {
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root',
})
export class CourseService extends ApiService {
  private readonly apiUrl = '/Courses';
  private readonly ageGroupUrl = '/AgeGroups';
  private readonly programmingLanguageUrl = '/ProgrammingLanguages';

  getAllCourses(query: CourseQuery): Observable<PagedResult<Course>> {
    const params = new URLSearchParams();
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

  getAllAgeGroups(): Observable<AgeGroup[]> {
    return this.get<AgeGroup[]>(this.ageGroupUrl);
  }

  getAllProgrammingLanguages(): Observable<ProgrammingLanguage[]> {
    return this.get<ProgrammingLanguage[]>(this.programmingLanguageUrl);
  }
}
