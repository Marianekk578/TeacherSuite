import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface AgeGroup {
  id: number;
  name: string;
  minAge: number;
  maxAge: number;
}

export interface ProgrammingLanguage {
  id: number;
  name: string;
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
}

export interface UpdateCourseDto {
  name: string;
  description: string;
  ageGroupID: number;
}

@Injectable({
  providedIn: 'root',
})
export class CourseService extends ApiService {
  private readonly apiUrl = '/Courses';
  private readonly ageGroupUrl = '/AgeGroups';

  getAllCourses(): Observable<Course[]> {
    return this.get<Course[]>(this.apiUrl);
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
}
