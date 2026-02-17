import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface ProgrammingLanguage {
  id: number;
  name: string;
}

export interface CreateProgrammingLanguageDto {
  name: string;
}

export interface UpdateProgrammingLanguageDto {
  name: string;
}

@Injectable({
  providedIn: 'root',
})
export class ProgrammingLanguageService extends ApiService {
  private readonly apiUrl = '/ProgrammingLanguages';

  getAllProgrammingLanguages(): Observable<ProgrammingLanguage[]> {
    return this.get<ProgrammingLanguage[]>(this.apiUrl);
  }

  getProgrammingLanguageById(id: number): Observable<ProgrammingLanguage> {
    return this.get<ProgrammingLanguage>(`${this.apiUrl}/${id}`);
  }

  createProgrammingLanguage(programmingLanguage: CreateProgrammingLanguageDto): Observable<number> {
    return this.post<number>(this.apiUrl, programmingLanguage);
  }

  updateProgrammingLanguage(id: number, programmingLanguage: UpdateProgrammingLanguageDto): Observable<void> {
    return this.put(`${this.apiUrl}/${id}`, programmingLanguage);
  }

  deleteProgrammingLanguage(id: number): Observable<void> {
    return this.delete(`${this.apiUrl}/${id}`);
  }

  assignToTeacher(teacherId: string, programmingLanguageId: number): Observable<void> {
    return this.put(`/Teachers/${teacherId}/programming-languages/${programmingLanguageId}`, {});
  }

  unassignFromTeacher(teacherId: string, programmingLanguageId: number): Observable<void> {
    return this.delete(`/Teachers/${teacherId}/programming-languages/${programmingLanguageId}`);
  }
}
