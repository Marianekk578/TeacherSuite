import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Teacher } from './teacher.service';
import { AgeGroup } from './course.service';

export interface Group {
  id: string;
  name: string;
  teacherId: string;
  ageGroupID: number;
  teacher?: Teacher;
  ageGroup?: AgeGroup;
}

export interface CreateGroupDto {
  name: string;
  teacherId: string;
  ageGroupID: number;
}

export interface UpdateGroupDto {
  name: string;
  teacherId: string;
  ageGroupID: number;
}

@Injectable({
  providedIn: 'root',
})
export class GroupService extends ApiService {
  private readonly apiUrl = '/Groups';
  private readonly teacherUrl = '/Teachers';
  private readonly ageGroupUrl = '/AgeGroups';

  getAllGroups(): Observable<Group[]> {
    return this.get<Group[]>(this.apiUrl);
  }

  getGroupById(id: string): Observable<Group> {
    return this.get<Group>(`${this.apiUrl}/${id}`);
  }

  createGroup(group: CreateGroupDto): Observable<string> {
    return this.post<string>(this.apiUrl, group);
  }

  updateGroup(id: string, group: UpdateGroupDto): Observable<void> {
    return this.put(`${this.apiUrl}/${id}`, group);
  }

  deleteGroup(id: string): Observable<void> {
    return this.delete(`${this.apiUrl}/${id}`);
  }

  getAllTeachers(): Observable<Teacher[]> {
    return this.get<Teacher[]>(this.teacherUrl);
  }

  getAllAgeGroups(): Observable<AgeGroup[]> {
    return this.get<AgeGroup[]>(this.ageGroupUrl);
  }
}
