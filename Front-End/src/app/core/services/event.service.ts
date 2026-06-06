import { Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { SKIP_ERROR_TOAST } from './auth.service';
import {
  EventResponseDto,
  EventItemResponseDto,
  EventSummaryDto,
  PagedResult,
  PaginatedRequest,
  CreateEventDto,
  UpdateEventDto,
  ApproveItemRequest,
  CancelEventRequest,
  CreateEventItemDto,
  AiEventPlanResponse,
  EventCollaboratorDto
} from '../../shared/types/api.interfaces';
import { normalizeAddressDto } from '../../shared/utils/location.utils';

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
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(res => this.normalizeEvent(this.unwrapEventBody(res)))
    );
  }
  /** GET /Vendor/bookings — scoped to the signed-in vendor via auth token */
  getForVendor(vendorUserId: string): Observable<EventResponseDto[]> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);
    return this.http.get<any>(`${environment.apiUrl}/Vendor/bookings`, { context }).pipe(
      map(res => this.mapVendorBookingsToEvents(res, vendorUserId)),
      catchError(err => {
        // Backend returns 404 when the vendor has no bookings yet.
        if (err?.status === 404 || err?.status === 204) {
          return of([]);
        }
        return throwError(() => err);
      })
    );
  }

  private extractBookingList(res: any): any[] {
    if (res == null) return [];
    if (Array.isArray(res)) return res;

    const inner = res.value ?? res.Value ?? res.data ?? res.Data;
    if (Array.isArray(inner)) return inner;
    if (inner != null && typeof inner === 'object') {
      const nested = inner.items ?? inner.Items ?? inner.$values ?? inner.$Values;
      if (Array.isArray(nested)) return nested;
    }

    const top = res.items ?? res.Items ?? res.$values ?? res.$Values;
    if (Array.isArray(top)) return top;

    return [];
  }

  private mapVendorBookingsToEvents(res: any, vendorUserId: string): EventResponseDto[] {
    const bookings = this.extractBookingList(res);
    if (!bookings.length) return [];

    const eventsMap = new Map<string, EventResponseDto>();
    const normalizedVendorId = this.normalizeEventId(vendorUserId);

    for (const b of bookings) {
      const eventId = this.normalizeEventId(this.pickField(b, 'eventId', 'EventId'));
      if (!eventId) continue;

      if (!eventsMap.has(eventId)) {
        eventsMap.set(eventId, {
          id: eventId,
          userId: this.normalizeEventId(this.pickField(b, 'userId', 'UserId')),
          userName: String(
            this.pickField(
              b,
              'clientName',
              'ClientName',
              'userName',
              'UserName',
              'customerName',
              'CustomerName'
            ) ?? 'Client'
          ),
          title: String(this.pickField(b, 'eventTitle', 'EventTitle') ?? ''),
          eventTypeName: String(this.pickField(b, 'eventType', 'EventType') ?? ''),
          eventDate: String(this.pickField(b, 'eventDate', 'EventDate') ?? ''),
          totalBudget: Number(this.pickField(b, 'totalBudget', 'TotalBudget') ?? 0),
          guestCount: Number(this.pickField(b, 'guestCount', 'GuestCount') ?? 0),
          notes: this.pickField(b, 'notes', 'Notes'),
          eventStatus: String(this.pickField(b, 'eventStatus', 'EventStatus') ?? 'Pending'),
          location: normalizeAddressDto(
            b.location ?? b.Location ?? {
              street: this.pickField(b, 'eventStreet', 'EventStreet', 'street', 'Street'),
              city: this.pickField(b, 'eventCity', 'EventCity', 'city', 'City'),
              state: this.pickField(b, 'eventState', 'EventState', 'state', 'State')
            }
          ),
          eventItems: []
        });
      }

      const itemId = this.normalizeEventId(
        this.pickField(b, 'eventItemId', 'EventItemId', 'id', 'Id')
      );
      if (!itemId) continue;

      eventsMap.get(eventId)!.eventItems.push({
        id: itemId,
        eventId,
        vendorId: normalizedVendorId,
        vendorName: String(this.pickField(b, 'vendorName', 'VendorName') ?? ''),
        serviceName: String(this.pickField(b, 'serviceName', 'ServiceName') ?? ''),
        price: Number(this.pickField(b, 'price', 'Price') ?? 0),
        quantity: Number(this.pickField(b, 'quantity', 'Quantity') ?? 1),
        itemStatus: this.mapItemStatus(
          this.pickField(b, 'bookingStatus', 'BookingStatus', 'itemStatus', 'ItemStatus')
        ),
        rejectionReason: this.pickField(b, 'rejectionReason', 'RejectionReason')
      });
    }

    return Array.from(eventsMap.values());
  }

  /**
   * Load events for the signed-in user.
   * Prefer GET /Event — it scopes by token user id without the strict Customer-role gate on /my-events.
   */
  getByUser(): Observable<EventResponseDto[]> {
    return this.http.get<any>(this.apiUrl).pipe(
      map(res => this.mapEventList(res)),
      catchError(err => {
        if (err?.status === 403 || err?.status === 404) {
          return this.http.get<any>(`${this.apiUrl}/my-events`).pipe(
            map(res => this.mapEventList(res))
          );
        }
        return throwError(() => err);
      })
    );
  }

  private mapEventList(res: any): EventResponseDto[] {
    return this.extractEventList(res)
      .map(e => this.normalizeEvent(e))
      .filter(e => !!e.id);
  }

  private extractEventList(res: any): any[] {
    if (!res) return [];
    if (Array.isArray(res)) return res;

    const inner = res.value ?? res.Value ?? res.data ?? res.Data;
    if (inner != null) {
      if (Array.isArray(inner)) return inner;
      const nested = inner.items ?? inner.Items ?? inner.$values ?? inner.$Values;
      if (Array.isArray(nested)) return nested;
    }

    const top = res.items ?? res.Items ?? res.$values ?? res.$Values;
    if (Array.isArray(top)) return top;

    return [];
  }

  private normalizeEventId(id: unknown): string {
    return id == null ? '' : String(id).trim().toLowerCase();
  }

  private unwrapEventBody(o: any): any {
    if (!o || typeof o !== 'object') return o;
    const nested = o.value ?? o.Value;
    if (nested && typeof nested === 'object' && (nested.id ?? nested.Id)) {
      return nested;
    }
    return o;
  }

  private pickField(obj: any, ...keys: string[]): any {
    if (!obj) return undefined;
    for (const key of keys) {
      const val = obj[key];
      if (val !== undefined && val !== null && val !== '') return val;
    }
    return undefined;
  }

  private mapItemStatus(value: unknown): EventItemResponseDto['itemStatus'] {
    switch (String(value ?? 'Pending').toLowerCase()) {
      case 'approved': return 'Approved';
      case 'paid': return 'Paid';
      case 'rejected': return 'Rejected';
      case 'done': return 'Done';
      case 'completed': return 'Completed';
      default: return 'Pending';
    }
  }

  private normalizeEvent = (e: any): EventResponseDto => {
    const raw = this.unwrapEventBody(e);
    if (!raw || typeof raw !== 'object') {
      return {
        id: '',
        userId: '',
        userName: '',
        title: '',
        eventTypeName: '',
        eventDate: '',
        totalBudget: 0,
        guestCount: 0,
        notes: undefined,
        eventStatus: 'Pending',
        eventItems: []
      };
    }

    const eventItems = raw.eventItems ?? raw.EventItems ?? [];

    return {
      id: this.normalizeEventId(this.pickField(raw, 'id', 'Id')),
      userId: this.normalizeEventId(this.pickField(raw, 'userId', 'UserId')),
      userName: String(this.pickField(raw, 'userName', 'UserName') ?? ''),
      title: String(this.pickField(raw, 'title', 'Title') ?? ''),
      eventTypeName: String(this.pickField(raw, 'eventTypeName', 'EventTypeName') ?? ''),
      eventDate: String(this.pickField(raw, 'eventDate', 'EventDate') ?? ''),
      totalBudget: Number(this.pickField(raw, 'totalBudget', 'TotalBudget') ?? 0),
      guestCount: Number(this.pickField(raw, 'guestCount', 'GuestCount') ?? 0),
      notes: this.pickField(raw, 'notes', 'Notes'),
      eventStatus: String(this.pickField(raw, 'eventStatus', 'EventStatus') ?? 'Pending'),
      cancellationReason: this.pickField(raw, 'cancellationReason', 'CancellationReason'),
      additionalNotes: this.pickField(raw, 'additionalNotes', 'AdditionalNotes'),
      cancelledAt: this.pickField(raw, 'cancelledAt', 'CancelledAt'),
      location: normalizeAddressDto(raw.location ?? raw.Location),
      eventItems: (Array.isArray(eventItems) ? eventItems : []).map((item: any) => ({
        id: this.normalizeEventId(this.pickField(item, 'id', 'Id')),
        eventId: this.normalizeEventId(this.pickField(item, 'eventId', 'EventId')),
        serviceId: this.pickField(item, 'serviceId', 'ServiceId'),
        serviceImage: this.pickField(item, 'serviceImage', 'ServiceImage'),
        serviceName: String(this.pickField(item, 'serviceName', 'ServiceName') ?? ''),
        price: Number(this.pickField(item, 'price', 'Price') ?? 0),
        vendorId: String(this.pickField(item, 'vendorId', 'VendorId') ?? ''),
        vendorName: String(this.pickField(item, 'vendorName', 'VendorName') ?? ''),
        quantity: Number(this.pickField(item, 'quantity', 'Quantity') ?? 1),
        itemStatus: this.mapItemStatus(this.pickField(item, 'itemStatus', 'ItemStatus')),
        rejectionReason: this.pickField(item, 'rejectionReason', 'RejectionReason')
      }))
    };
  };

  /** GET /Event/status/{status} - Get events by status (e.g. Completed) */
  getByStatus(status: string): Observable<EventResponseDto[]> {
    return this.http.get<any>(`${this.apiUrl}/status/${status}`).pipe(
      map(res => this.extractEventList(res).map(e => this.normalizeEvent(e)))
    );
  }

  /** PATCH /Event/{eventId}/items/{itemId}/status — Done (vendor) or Completed (customer) */
  updateItemStatus(
    eventId: string,
    itemId: string,
    status: 'Done' | 'Completed'
  ): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${eventId}/items/${itemId}/status`, { status });
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

  // ── Collaborators ─────────────────────────────────────────────────────────

  /** GET /Event/{id}/collaborators */
  getCollaborators(eventId: string): Observable<EventCollaboratorDto[]> {
    return this.http.get<any>(`${this.apiUrl}/${eventId}/collaborators`).pipe(
      map(res => {
        const data = res?.value ?? res;
        return Array.isArray(data) ? data : [];
      })
    );
  }

  /** POST /Event/{id}/collaborators */
  addCollaborator(eventId: string, userEmailOrName: string, role: 0 | 1): Observable<any> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    return this.http.post<any>(
      `${this.apiUrl}/${eventId}/collaborators`,
      { userEmailOrName, role },
      { headers }
    );
  }

  /** DELETE /Event/{id}/collaborators/{userId} */
  removeCollaborator(eventId: string, userId: string): Observable<void> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    return this.http.delete<void>(`${this.apiUrl}/${eventId}/collaborators/${userId}`, { headers });
  }
}
