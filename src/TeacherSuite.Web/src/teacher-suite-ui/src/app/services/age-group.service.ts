import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AgeGroup {
  id: number;
  name: string;
  minAge: number;
  maxAge: number;
}

export interface CreateAgeGroupCommand {
  name: string;
  minAge: number;
  maxAge: number;
}

@Injectable({
  providedIn: 'root'
})
export class AgeGroupService {
  private apiUrl = '/AgeGroups';

  constructor(private http: HttpClient) {}

  getAllAgeGroups(): Observable<AgeGroup[]> {
    return this.http.get<AgeGroup[]>(this.apiUrl);
  }

  createAgeGroup(command: CreateAgeGroupCommand): Observable<number> {
    return this.http.post<number>(this.apiUrl, command);
  }
}
