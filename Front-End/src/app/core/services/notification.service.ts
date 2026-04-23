import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AppNotification } from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** GET /notifications */
  getNotifications(): Observable<AppNotification[]> {
    return this.http.get<any>(`${this.apiUrl}/notifications`).pipe(
      map(res => {
        if (!res) return [];
        if (Array.isArray(res)) return res;
        if (res.value && Array.isArray(res.value)) return res.value;
        if (res.items && Array.isArray(res.items)) return res.items;
        if (res.value && res.value.items && Array.isArray(res.value.items)) return res.value.items;
        return [];
      }),
      catchError((error) => {
        console.warn('Notifications API not available yet (404). Returning empty list.');
        return of([]);
      })
    );
  }

  /** PATCH /notifications/{id}/read */
  markAsRead(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/notifications/${id}/read`, {});
  }
  
  // Note: /notifications/stream could be implemented via SSE (Server-Sent Events) or SignalR
  // Depending on the backend implementation. For now we stick to REST.
}
