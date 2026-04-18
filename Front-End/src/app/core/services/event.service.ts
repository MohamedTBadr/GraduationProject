import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  EventResponseDto,
  EventSummaryDto,
  PagedResult,
  PaginationParams,
  CreateEventDto,
  UpdateEventDto,
  ApproveItemRequest,
  CancelEventRequest
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class EventService {
  private readonly apiUrl = `${environment.apiUrl}/Event`;

  constructor(private http: HttpClient) {}

  /** GET /Event - Get all events (paginated) */
  getAll(params?: PaginationParams): Observable<PagedResult<EventSummaryDto>> {
    const queryParams: any = {};
    if (params?.pageNumber) queryParams.pageNumber = params.pageNumber;
    if (params?.pageSize) queryParams.pageSize = params.pageSize;
    
    return this.http.get<PagedResult<EventSummaryDto>>(this.apiUrl, { params: queryParams });
  }

  /** POST /Event - Create a new event */
  create(payload: CreateEventDto): Observable<EventResponseDto> {
    const headers = new HttpHeaders({
      'IdempotencyKey': crypto.randomUUID()
    });
    return this.http.post<EventResponseDto>(this.apiUrl, payload, { headers });
  }

  /** PUT /Event/{id} - Update an existing event */
  update(id: string, payload: UpdateEventDto): Observable<EventResponseDto> {
    return this.http.put<EventResponseDto>(`${this.apiUrl}/${id}`, payload);
  }

  /** GET /Event/{id} - Get event details */
  getById(id: string): Observable<EventResponseDto> {
    return this.http.get<EventResponseDto>(`${this.apiUrl}/${id}`);
  }

  /** GET /Event/user/{userId} - Get events for a specific user/vendor */
  getByUser(userId: string): Observable<EventResponseDto[]> {
    return this.http.get<any>(`${this.apiUrl}/user/${userId}`).pipe(
      map(res => res.value || res)
    );
  }

  /** GET /Event/status/{status} - Get events by status */
  getByStatus(status: string): Observable<EventResponseDto[]> {
    return this.http.get<any>(`${this.apiUrl}/status/${status}`).pipe(
      map(res => res.value || res)
    );
  }

  /** PATCH /Event/{eventId}/items/{itemId}/approve - Vendor approve/reject an item */
  approveItem(eventId: string, itemId: string, payload: ApproveItemRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${eventId}/items/${itemId}/approve`, payload);
  }

  /** PATCH /Event/{id}/cancel - Cancel an event */
  cancelEvent(id: string, payload: CancelEventRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/cancel`, payload);
  }
}
