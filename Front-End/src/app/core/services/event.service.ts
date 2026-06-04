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
        const bookings = res?.value || res?.Value || (Array.isArray(res) ? res : []);
        if (!Array.isArray(bookings)) return [];

        const eventsMap = new Map<string, EventResponseDto>();

        bookings.forEach(b => {
          const eventId = b.eventId || b.EventId;
          const eventItemId = b.eventItemId || b.EventItemId;
          const serviceName = b.serviceName || b.ServiceName;
          const price = b.price || b.Price;
          const bookingStatus = b.bookingStatus || b.BookingStatus;
          const notes = b.notes || b.Notes;
          const eventTitle = b.eventTitle || b.EventTitle;
          const eventType = b.eventType || b.EventType;
          const eventDate = b.eventDate || b.EventDate;
          const eventStatus = b.eventStatus || b.EventStatus;
          const guestCount = b.guestCount || b.GuestCount;
          const location = b.location || b.Location;

          if (!eventsMap.has(eventId)) {
            eventsMap.set(eventId, {
              id: eventId,
              userId: '',
              userName: 'Client',
              title: eventTitle,
              eventTypeName: eventType,
              eventDate: eventDate,
              totalBudget: 0,
              guestCount: guestCount,
              notes: notes,
              eventStatus: eventStatus,
              eventItems: []
            });
          }

          const ev = eventsMap.get(eventId)!;
          ev.eventItems.push({
            id: eventItemId,
            eventId: eventId,
            vendorId: vendorUserId,
            vendorName: '',
            serviceName: serviceName,
            price: price,
            quantity: 1,
            itemStatus: bookingStatus
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
        const arr = res.value ?? res.Value;
        if (arr && Array.isArray(arr)) return arr.map(this.normalizeEvent);
        const items = res.value?.items ?? res.Value?.items ?? res.items ?? res.Items;
        if (items && Array.isArray(items)) return items.map(this.normalizeEvent);
        return [];
      })
    );
  }

  private normalizeEvent = (e: any): EventResponseDto => ({
    id: e.id ?? e.Id,
    userId: e.userId ?? e.UserId,
    userName: e.userName ?? e.UserName,
    title: e.title ?? e.Title,
    eventTypeName: e.eventTypeName ?? e.EventTypeName,
    eventDate: e.eventDate ?? e.EventDate,
    totalBudget: e.totalBudget ?? e.TotalBudget ?? 0,
    guestCount: e.guestCount ?? e.GuestCount ?? 0,
    notes: e.notes ?? e.Notes,
    eventStatus: e.eventStatus ?? e.EventStatus,
    cancellationReason: e.cancellationReason ?? e.CancellationReason,
    additionalNotes: e.additionalNotes ?? e.AdditionalNotes,
    cancelledAt: e.cancelledAt ?? e.CancelledAt,
    location: e.location ?? e.Location,
    eventItems: (e.eventItems ?? e.EventItems ?? []).map((item: any) => ({
      id: item.id ?? item.Id,
      eventId: item.eventId ?? item.EventId,
      serviceId: item.serviceId ?? item.ServiceId,
      serviceImage: item.serviceImage ?? item.ServiceImage,
      serviceName: item.serviceName ?? item.ServiceName,
      price: item.price ?? item.Price ?? 0,
      vendorId: item.vendorId ?? item.VendorId,
      vendorName: item.vendorName ?? item.VendorName,
      quantity: item.quantity ?? item.Quantity ?? 1,
      itemStatus: item.itemStatus ?? item.ItemStatus,
      rejectionReason: item.rejectionReason ?? item.RejectionReason
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

  /** PATCH /Event/{eventId}/items/{itemId}/status - Update item status (e.g., Done, Completed) */
  updateItemStatus(eventId: string, itemId: string, status: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${eventId}/items/${itemId}/status`, { status });
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
