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

  protected get<T>(url: string): Observable<T> {
    return from(
      fetch(url).then((response) => {
        if (!response.ok) {
          throw new ApiError(response.status, response.statusText);
        }
        return this.parseJsonIfPresent<T>(response);
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
        return this.parseJsonIfPresent<T>(response);
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

  protected patch(url: string, body: unknown): Observable<void> {
    return from(
      fetch(url, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      }).then((response) => {
        if (!response.ok) {
          throw new ApiError(response.status, response.statusText);
        }
      })
    );
  }
}
