import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { EventType } from '../models/taxonomy.models';

@Injectable({ providedIn: 'root' })
export class EventTypeService {
  private readonly apiUrl = environment.apiUrl;
  private cachedEventTypes: EventType[] | null = null;

  constructor(private http: HttpClient) {}

  /** GET /api/EventTypes */
  getAll(): Observable<EventType[]> {
    if (this.cachedEventTypes?.some(t => !t.id || t.name == null)) {
      this.cachedEventTypes = null;
    }
    if (this.cachedEventTypes) {
      return of(this.cachedEventTypes);
    }
    
    return this.http.get<any>(`${this.apiUrl}/EventTypes`).pipe(
      map(res => {
        if (!res) return [];
        const data = res.value ?? res;
        const arr = Array.isArray(data) ? data : (data.items ?? []);
        return arr.map((item: any) => ({
          id: String(item.Id ?? item.id ?? ''),
          name: String(item.Name ?? item.name ?? '')
        }));
      }),
      tap(data => this.cachedEventTypes = data)
    );
  }

  /** GET /api/EventTypes/{id} */
  getById(id: string): Observable<EventType> {
    return this.http.get<any>(`${this.apiUrl}/EventTypes/${id}`).pipe(
      map(res => {
        if (!res) return { id: '', name: '' };
        const item = res.value ?? res;
        return { id: item.id, name: item.name };
      })
    );
  }

  /** POST /api/EventTypes */
  create(payload: { name: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/EventTypes`, payload, { responseType: 'text' }).pipe(
      tap(() => this.cachedEventTypes = null)
    );
  }

  /** PUT /api/EventTypes/{id} */
  update(id: string, payload: { id: string, name: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/EventTypes/${id}`, payload, { responseType: 'text' }).pipe(
      tap(() => this.cachedEventTypes = null)
    );
  }

  /** DELETE /api/EventTypes/{id} */
  delete(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/EventTypes/${id}`, { responseType: 'text' }).pipe(
      tap(() => this.cachedEventTypes = null)
    );
  }
}

