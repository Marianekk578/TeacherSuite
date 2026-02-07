import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from './base-http.service';

export interface AgeGroup {
  id: number;
  name: string;
  minAge: number;
  maxAge: number;
}

export interface CreateAgeGroupDto {
  name: string;
  minAge: number;
  maxAge: number;
}

@Injectable({
  providedIn: 'root',
})
export class AgeGroupService extends BaseHttpService {
  protected readonly baseUrl = '/AgeGroups';

  getAllAgeGroups(): Observable<AgeGroup[]> {
    return this.get<AgeGroup[]>(this.baseUrl);
  }

  createAgeGroup(ageGroup: CreateAgeGroupDto): Observable<number> {
    return this.post<CreateAgeGroupDto, number>(this.baseUrl, ageGroup);
  }
}
