import { inject, Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';
import { ApiService, ApiError } from './api.service';
import { KeycloakService } from '../auth/keycloak.service';

export interface RequirementIconDto {
  id: number;
  emoji: string;
  label: string;
}

export interface Lesson {
  id: number;
  courseId: number;
  order: number;
  title: string;
  description?: string;
  durationMinutes: number;
  albumId?: string;
  requirementIcons: RequirementIconDto[];
}

export interface LessonDetail extends Lesson {
  courseName: string;
  suggestions: LessonSuggestion[];
  attendances: LessonAttendance[];
}

export interface LessonSuggestion {
  id: string;
  lessonId: number;
  teacherId: string;
  teacherName: string;
  teacherEmail?: string;
  content: string;
  selectedText?: string;
  selectionStart?: number;
  selectionEnd?: number;
  created: string;
  upvoteCount: number;
  downvoteCount: number;
}

export interface LessonAttendance {
  id: string;
  lessonId: number;
  groupId: string;
  groupName: string;
  attendedAt: string;
}

export interface LessonFile {
  uuid: string;
  name: string;
  size: number;
}

export interface CreateLessonDto {
  courseId: number;
  title: string;
  description?: string;
  durationMinutes: number;
  requirementIconIds?: number[];
}

export interface UpdateLessonDto {
  title: string;
  description?: string;
  durationMinutes: number;
  requirementIconIds?: number[];
}

export interface CreateSuggestionDto {
  content: string;
  selectedText?: string;
  selectionStart?: number;
  selectionEnd?: number;
}

export interface RecordAttendanceDto {
  groupId: string;
  attendedAt: string;
}

export interface VoteDto {
  vote: number;
}

export interface CourseGroup {
  id: string;
  name: string;
}

@Injectable({
  providedIn: 'root',
})
export class LessonService extends ApiService {
  private readonly apiUrl = '/Lessons';
  private readonly keycloak = inject(KeycloakService);

  getLessonsByCourse(courseId: number): Observable<Lesson[]> {
    return this.get<Lesson[]>(`${this.apiUrl}?courseId=${courseId}`);
  }

  getLessonById(id: number): Observable<LessonDetail> {
    return this.get<LessonDetail>(`${this.apiUrl}/${id}`);
  }

  createLesson(lesson: CreateLessonDto): Observable<number> {
    return this.post<number>(this.apiUrl, lesson);
  }

  updateLesson(id: number, lesson: UpdateLessonDto): Observable<void> {
    return this.put(`${this.apiUrl}/${id}`, lesson);
  }

  deleteLesson(id: number): Observable<void> {
    return this.delete(`${this.apiUrl}/${id}`);
  }

  reorderLesson(id: number, direction: string): Observable<void> {
    return this.post<void>(`${this.apiUrl}/${id}/reorder`, { direction });
  }

  uploadMaterial(lessonId: number, file: File): Observable<void> {
    return from(this.uploadFile(`${this.apiUrl}/${lessonId}/material`, file));
  }

  downloadMaterial(lessonId: number, fileUuid: string): Observable<void> {
    return from(this.downloadFileByUrl(`${this.apiUrl}/${lessonId}/material/download?fileUuid=${fileUuid}`));
  }

  async downloadMaterialAsText(lessonId: number, fileUuid: string): Promise<string> {
    const headers: Record<string, string> = {};
    const token = await this.getAuthToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(`${this.apiUrl}/${lessonId}/material/download?fileUuid=${fileUuid}`, { headers });

    if (!response.ok) {
      throw new ApiError(response.status, response.statusText);
    }

    return await response.text();
  }

  getLessonFiles(lessonId: number): Observable<LessonFile[]> {
    return this.get<LessonFile[]>(`${this.apiUrl}/${lessonId}/files`);
  }

  getCourseGroups(courseId: number): Observable<CourseGroup[]> {
    return this.get<CourseGroup[]>(`${this.apiUrl}/course/${courseId}/groups`);
  }

  getSuggestions(lessonId: number): Observable<LessonSuggestion[]> {
    return this.get<LessonSuggestion[]>(`${this.apiUrl}/${lessonId}/suggestions`);
  }

  createSuggestion(lessonId: number, suggestion: CreateSuggestionDto): Observable<string> {
    return this.post<string>(`${this.apiUrl}/${lessonId}/suggestions`, suggestion);
  }

  deleteSuggestion(suggestionId: string): Observable<void> {
    return this.delete(`${this.apiUrl}/suggestions/${suggestionId}`);
  }

  voteSuggestion(suggestionId: string, vote: VoteDto): Observable<void> {
    return this.post<void>(`${this.apiUrl}/suggestions/${suggestionId}/vote`, vote);
  }

  getAttendances(lessonId: number): Observable<LessonAttendance[]> {
    return this.get<LessonAttendance[]>(`${this.apiUrl}/${lessonId}/attendances`);
  }

  recordAttendance(lessonId: number, attendance: RecordAttendanceDto): Observable<string> {
    return this.post<string>(`${this.apiUrl}/${lessonId}/attendances`, attendance);
  }

  private async getAuthToken(): Promise<string | null> {
    try {
      const token = await this.keycloak.updateToken(30);
      return token && token.trim() ? token : null;
    } catch {
      return null;
    }
  }

  private async uploadFile(url: string, file: File): Promise<void> {
    const formData = new FormData();
    formData.append('file', file);

    const headers: Record<string, string> = {};
    const token = await this.getAuthToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      method: 'POST',
      headers,
      body: formData,
    });

    if (!response.ok) {
      let detail: string | undefined;
      let errors: Record<string, string[]> | undefined;
      try {
        const body = await response.json();
        detail = body?.detail;
        errors = body?.errors;
      } catch {
        // response body is not JSON
      }
      throw new ApiError(response.status, response.statusText, undefined, detail, errors);
    }
  }

  private async downloadFileByUrl(url: string): Promise<void> {
    const headers: Record<string, string> = {};
    const token = await this.getAuthToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(url, { headers });

    if (!response.ok) {
      let detail: string | undefined;
      let errors: Record<string, string[]> | undefined;
      try {
        const body = await response.json();
        detail = body?.detail;
        errors = body?.errors;
      } catch {
        // response body is not JSON
      }
      throw new ApiError(response.status, response.statusText, undefined, detail, errors);
    }

    const blob = await response.blob();
    const contentDisposition = response.headers.get('content-disposition');
    const fileName = this.extractFileName(contentDisposition) ?? 'download';

    const objectUrl = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = objectUrl;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(objectUrl);
  }

  private extractFileName(contentDisposition: string | null): string | null {
    if (!contentDisposition) {
      return null;
    }
    const utf8Match = contentDisposition.match(/filename\*=UTF-8''(.+?)(?:;|$)/i);
    if (utf8Match) {
      return decodeURIComponent(utf8Match[1]);
    }
    const match = contentDisposition.match(/filename="?(.+?)"?(?:;|$)/);
    return match ? match[1] : null;
  }
}
