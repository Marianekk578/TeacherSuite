import { Observable, from } from 'rxjs';

export class ApiError extends Error {
  status: number;
  statusText: string;

  constructor(status: number, statusText: string, message?: string) {
    super(message ?? `Request failed: ${status} ${statusText}`);
    this.status = status;
    this.statusText = statusText;
  }
}

export class ApiService {
  protected get<T>(url: string): Observable<T> {
    return from(
      fetch(url).then((response) => {
        if (!response.ok) {
          throw new ApiError(response.status, response.statusText);
        }
        return response.json();
      })
    );
  }

  protected post<T>(url: string, body: unknown): Observable<T> {
    return from(
      fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      }).then((response) => {
        if (!response.ok) {
          throw new ApiError(response.status, response.statusText);
        }
        return response.json();
      })
    );
  }

  protected put(url: string, body: unknown): Observable<void> {
    return from(
      fetch(url, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      }).then((response) => {
        if (!response.ok) {
          throw new ApiError(response.status, response.statusText);
        }
      })
    );
  }

  protected delete(url: string): Observable<void> {
    return from(
      fetch(url, {
        method: 'DELETE',
      }).then((response) => {
        if (!response.ok) {
          throw new ApiError(response.status, response.statusText);
        }
      })
    );
  }
}
