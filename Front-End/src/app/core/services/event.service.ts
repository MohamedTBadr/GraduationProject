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
  CancelEventRequest,
  CreateEventItemDto
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

  /** POST /Event/{eventId}/items - Add an item to an event */
  addItem(eventId: string, payload: CreateEventItemDto): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${eventId}/items`, payload);
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
      map(res => {
        if (!res) return [];
        if (Array.isArray(res)) return res;
        if (res.value && Array.isArray(res.value)) return res.value;
        if (res.value?.items && Array.isArray(res.value.items)) return res.value.items;
        if (res.items && Array.isArray(res.items)) return res.items;
        return res.value || res;
      })
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

  /** POST /Event/createEventByAI/{eventId} - Generate an event plan using Gemini */
  generateEventByAI(eventId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/createEventByAI/${eventId}`, {});
  }
}
