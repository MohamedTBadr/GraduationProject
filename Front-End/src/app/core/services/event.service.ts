import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  EventResponseDto,
  EventSummaryDto,
  PagedResult,
  PaginatedRequest,
  CreateEventDto,
  UpdateEventDto,
  ApproveItemRequest,
  CancelEventRequest,
  CreateEventItemDto,
  AiEventPlanResponse
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class EventService {
  private readonly apiUrl = `${environment.apiUrl}/Event`;

  constructor(private http: HttpClient) {}

  /** GET /Event - Get all events (paginated) */
  getAll(params?: PaginatedRequest): Observable<PagedResult<EventSummaryDto>> {
    let queryParams = new HttpParams();
    if (params) {
      if (params.pageIndex) queryParams = queryParams.set('pageIndex', params.pageIndex.toString());
      if (params.pageSize) queryParams = queryParams.set('pageSize', params.pageSize.toString());
      if (params.searchTerm) queryParams = queryParams.set('searchTerm', params.searchTerm);
    }
    
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
  getForVendor(vendorUserId: string): Observable<EventResponseDto[]> {
    return this.http.get<any>(`${environment.apiUrl}/Vendor/bookings`).pipe(
      map(res => {
        const bookings = res?.value ?? (Array.isArray(res) ? res : []);
        if (!Array.isArray(bookings)) return [];

        const eventsMap = new Map<string, EventResponseDto>();

        bookings.forEach((b: any) => {
          if (!eventsMap.has(b.eventId)) {
            eventsMap.set(b.eventId, {
              id: b.eventId,
              userId: '',
              userName: 'Client',
              title: b.eventTitle,
              eventTypeName: b.eventType,
              eventDate: b.eventDate,
              totalBudget: 0,
              guestCount: b.guestCount,
              notes: b.notes,
              eventStatus: b.eventStatus,
              eventItems: []
            });
          }

          eventsMap.get(b.eventId)!.eventItems.push({
            id: b.eventItemId,
            eventId: b.eventId,
            vendorId: vendorUserId,
            vendorName: '',
            serviceName: b.serviceName,
            price: b.price,
            quantity: 1,
            itemStatus: b.bookingStatus
          });
        });

        return Array.from(eventsMap.values());
      })
    );
  }

  /** GET /Event/my-events - Get events for the authenticated user */
  getByUser(): Observable<EventResponseDto[]> {
    return this.http.get<any>(`${this.apiUrl}/my-events`).pipe(
      map(res => {
        if (!res) return [];
        if (Array.isArray(res)) return res.map(this.normalizeEvent);
        const arr = res.value;
        if (arr && Array.isArray(arr)) return arr.map(this.normalizeEvent);
        const items = res.value?.items ?? res.items;
        if (items && Array.isArray(items)) return items.map(this.normalizeEvent);
        return [];
      })
    );
  }

  private normalizeEvent = (e: any): EventResponseDto => ({
    id: e.id,
    userId: e.userId,
    userName: e.userName,
    title: e.title,
    eventTypeName: e.eventTypeName,
    eventDate: e.eventDate,
    totalBudget: e.totalBudget ?? 0,
    guestCount: e.guestCount ?? 0,
    notes: e.notes,
    eventStatus: e.eventStatus,
    cancellationReason: e.cancellationReason,
    additionalNotes: e.additionalNotes,
    cancelledAt: e.cancelledAt,
    location: e.location,
    eventItems: (e.eventItems ?? []).map((item: any) => ({
      id: item.id,
      eventId: item.eventId,
      serviceId: item.serviceId,
      serviceImage: item.serviceImage,
      serviceName: item.serviceName,
      price: item.price ?? 0,
      vendorId: item.vendorId,
      vendorName: item.vendorName,
      quantity: item.quantity ?? 1,
      itemStatus: item.itemStatus,
      rejectionReason: item.rejectionReason
    }))
  });

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

  /** POST /Event/createEventByAI/{eventId} - Generate an event plan using AI (Llama 3) */
  generateEventByAI(eventId: string): Observable<AiEventPlanResponse> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    return this.http.post<any>(`${this.apiUrl}/createEventByAI/${eventId}`, {}, { headers }).pipe(
      map(res => res?.value ?? res?.Value ?? res)
    );
  }
}
