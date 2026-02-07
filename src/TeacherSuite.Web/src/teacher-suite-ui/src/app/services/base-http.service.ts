import { Observable, from } from 'rxjs';

export abstract class BaseHttpService {
  protected abstract readonly baseUrl: string;

  protected get<T>(url: string): Observable<T> {
    return from(
      fetch(url)
        .then((response) => {
          if (!response.ok) {
            throw new Error(`HTTP error: ${response.status} ${response.statusText}`);
          }
          return response.json();
        })
        .catch((error) => {
          throw error;
        })
    );
  }

  protected post<TRequest, TResponse>(url: string, data: TRequest): Observable<TResponse> {
    return from(
      fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      })
        .then((response) => {
          if (!response.ok) {
            throw new Error(`HTTP error: ${response.status} ${response.statusText}`);
          }
          return response.json();
        })
        .catch((error) => {
          throw error;
        })
    );
  }

  protected put<TRequest>(url: string, data: TRequest): Observable<void> {
    return from(
      fetch(url, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      })
        .then((response) => {
          if (!response.ok) {
            throw new Error(`HTTP error: ${response.status} ${response.statusText}`);
          }
        })
        .catch((error) => {
          throw error;
        })
    );
  }

  protected delete(url: string): Observable<void> {
    return from(
      fetch(url, {
        method: 'DELETE',
      })
        .then((response) => {
          if (!response.ok) {
            throw new Error(`HTTP error: ${response.status} ${response.statusText}`);
          }
        })
        .catch((error) => {
          throw error;
        })
    );
  }

  protected convertToUtcIsoString(dateString: string): string {
    // dateString is in YYYY-MM-DD format from the date input
    // Create a date at midnight UTC to avoid timezone issues
    const date = new Date(dateString + 'T00:00:00.000Z');
    return date.toISOString();
  }
}
