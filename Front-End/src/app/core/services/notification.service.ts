import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AppNotification } from '../../shared/types/api.interfaces';

const NOTIFICATION_TYPE_BY_NAME: Record<string, number> = {
  ACCOUNT_ACCEPTED: 0,
  ACCOUNT_SUSPENDED: 1,
  ORDER_PLACED: 2,
  ORDER_REJECTED: 3,
  PAYMENT_REJECTED: 4,
  PAYMENT_ACCEPTED: 5,
  ORDER_CANCELLED: 6,
  ORDER_COMPLETED: 7,
  EVENT_STATUS_UPDATED: 8,
  EVENT_STATUS_DELETED: 9,
  EVENT_ITEM_APPROVED: 10,
  EVENT_ITEM_REJECTED: 11,
  EVENT_COMPLETED: 12
};

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** GET /notifications */
  getNotifications(): Observable<AppNotification[]> {
    return this.http.get<any>(`${this.apiUrl}/notifications`).pipe(
      map(res => this.extractNotificationList(res)),
      catchError((error) => {
        if (error?.status === 404) {
          console.warn('Notifications API not available yet (404). Returning empty list.');
          return of([]);
        }
        return throwError(() => error);
      })
    );
  }

  /** Normalizes camelCase/PascalCase API payloads into AppNotification. */
  normalizeNotification(raw: unknown): AppNotification | null {
    if (!raw || typeof raw !== 'object') return null;

    const item = raw as Record<string, unknown>;
    const id = item['id'] ?? item['Id'];
    if (id == null || id === '') return null;

    const typeRaw = item['type'] ?? item['Type'];
    let type: number | undefined;
    if (typeof typeRaw === 'number') {
      type = typeRaw;
    } else if (typeof typeRaw === 'string') {
      type = NOTIFICATION_TYPE_BY_NAME[typeRaw.trim().toUpperCase()];
    }

    return {
      id: String(id),
      userId: String(item['userId'] ?? item['UserId'] ?? ''),
      title: String(item['title'] ?? item['Title'] ?? ''),
      message: String(item['message'] ?? item['Message'] ?? ''),
      type,
      isRead: Boolean(item['isRead'] ?? item['IsRead'] ?? false),
      createdAt: String(item['createdAt'] ?? item['CreatedAt'] ?? ''),
      isLive: Boolean(item['isLive'] ?? item['IsLive'] ?? false)
    };
  }

  private extractNotificationList(res: unknown): AppNotification[] {
    if (!res) return [];

    let items: unknown[] = [];
    if (Array.isArray(res)) {
      items = res;
    } else if (typeof res === 'object') {
      const body = res as Record<string, unknown>;
      const value = body['value'] ?? body['Value'];
      const directItems = body['items'] ?? body['Items'];

      if (Array.isArray(value)) {
        items = value;
      } else if (Array.isArray(directItems)) {
        items = directItems;
      } else if (value && typeof value === 'object') {
        const nested = value as Record<string, unknown>;
        const nestedItems = nested['items'] ?? nested['Items'];
        if (Array.isArray(nestedItems)) {
          items = nestedItems;
        }
      }
    }

    return items
      .map(item => this.normalizeNotification(item))
      .filter((item): item is AppNotification => item !== null);
  }

  /** PATCH /notifications/{id}/read */
  markAsRead(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/notifications/${id}/read`, {});
  }
  
  // Note: /notifications/stream could be implemented via SSE (Server-Sent Events) or SignalR
  // Depending on the backend implementation. For now we stick to REST.
}
