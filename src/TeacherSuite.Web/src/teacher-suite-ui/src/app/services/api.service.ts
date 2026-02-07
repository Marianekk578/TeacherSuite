import { Observable, from } from 'rxjs';

export class ApiService {
  protected get<T>(url: string): Observable<T> {
    return from(
      fetch(url).then((response) => {
        if (!response.ok) {
          throw new Error(`Request failed: ${response.status} ${response.statusText}`);
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
          throw new Error(`Request failed: ${response.status} ${response.statusText}`);
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
          throw new Error(`Request failed: ${response.status} ${response.statusText}`);
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
          throw new Error(`Request failed: ${response.status} ${response.statusText}`);
        }
      })
    );
  }
}
