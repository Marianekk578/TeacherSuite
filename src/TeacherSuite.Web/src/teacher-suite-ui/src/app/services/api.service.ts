import { inject } from '@angular/core';
import { Observable, from } from 'rxjs';
import { KeycloakService } from '../auth/keycloak.service';

export class ApiError extends Error {
  status: number;
  statusText: string;
  detail?: string;
  errors?: Record<string, string[]>;

  constructor(status: number, statusText: string, message?: string, detail?: string, errors?: Record<string, string[]>) {
    super(message ?? `Request failed: ${status} ${statusText}`);
    this.status = status;
    this.statusText = statusText;
    this.detail = detail;
    this.errors = errors;
  }
}

export class ApiService {
  private keycloakService = inject(KeycloakService);

  private async getAuthHeaders(extra: Record<string, string> = {}): Promise<Record<string, string>> {
    try {
      const token = await this.keycloakService.updateToken(30);
      if (token && token.trim()) {
        return { Authorization: `Bearer ${token}`, ...extra };
      }
      return { ...extra };
    } catch {
      return { ...extra };
    }
  }

  private async parseJsonIfPresent<T>(response: Response): Promise<T> {
    if (response.status === 204 || response.status === 205) {
      return undefined as T;
    }

    const contentLength = response.headers.get('content-length');
    if (contentLength === '0') {
      return undefined as T;
    }

    const text = await response.text();
    if (!text) {
      return undefined as T;
    }

    try {
      return JSON.parse(text) as T;
    } catch {
      throw new ApiError(response.status, response.statusText, 'Invalid JSON response');
    }
  }

  private async throwApiError(response: Response): Promise<never> {
    let detail: string | undefined;
    let errors: Record<string, string[]> | undefined;
    try {
      const body = await response.json();
      detail = body?.detail;
      errors = body?.errors;
    } catch {
      // response body is not JSON — leave detail/errors undefined
    }
    throw new ApiError(response.status, response.statusText, undefined, detail, errors);
  }

  protected get<T>(url: string): Observable<T> {
    return from(
      this.getAuthHeaders().then((headers) =>
        fetch(url, { headers }).then((response) => {
          if (!response.ok) {
            return this.throwApiError(response);
          }
          return this.parseJsonIfPresent<T>(response);
        })
      )
    );
  }

  protected post<T>(url: string, body: unknown): Observable<T> {
    return from(
      this.getAuthHeaders({ 'Content-Type': 'application/json' }).then((headers) =>
        fetch(url, {
          method: 'POST',
          headers,
          body: JSON.stringify(body),
        }).then((response) => {
          if (!response.ok) {
            return this.throwApiError(response);
          }
          return this.parseJsonIfPresent<T>(response);
        })
      )
    );
  }

  protected put(url: string, body: unknown): Observable<void> {
    return from(
      this.getAuthHeaders({ 'Content-Type': 'application/json' }).then((headers) =>
        fetch(url, {
          method: 'PUT',
          headers,
          body: JSON.stringify(body),
        }).then(async (response) => {
          if (!response.ok) {
            await this.throwApiError(response);
          }
        })
      )
    );
  }

  protected delete(url: string): Observable<void> {
    return from(
      this.getAuthHeaders().then((headers) =>
        fetch(url, {
          method: 'DELETE',
          headers,
        }).then(async (response) => {
          if (!response.ok) {
            await this.throwApiError(response);
          }
        })
      )
    );
  }

  protected patch(url: string, body: unknown): Observable<void> {
    return from(
      this.getAuthHeaders({ 'Content-Type': 'application/json' }).then((headers) =>
        fetch(url, {
          method: 'PATCH',
          headers,
          body: JSON.stringify(body),
        }).then(async (response) => {
          if (!response.ok) {
            await this.throwApiError(response);
          }
        })
      )
    );
  }
}
