import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  CreateTicketRequest,
  SupportTicket,
  TicketStats,
  TicketFilters,
  ReplyTicketRequest,
  AssignTicketRequest,
  ResolveTicketRequest,
  EscalateTicketRequest,
  TicketReply
} from '../../shared/types/api.interfaces';
import {
  TicketSubmitterType,
  normalizeTicketResponse,
  extractCategoryFromDescription,
  pickField,
} from '../../shared/utils/support-ticket.utils';

export interface SubmittedTicketRecord {
  ticket_id: string;
  title: string;
  category: string;
  priority: string;
  status: string;
  opened_at: string;
  booking_ref?: string | null;
  submitter_type: TicketSubmitterType;
}

@Injectable({ providedIn: 'root' })
export class SupportService {
  private readonly adminBaseUrl = `${environment.apiUrl}/admin/support/tickets`;
  private readonly userTicketsUrl = `${environment.apiUrl}/support/tickets`;

  constructor(private http: HttpClient) {}

  /** GET /admin/support/tickets/stats - Get ticket stats */
  getStats(): Observable<TicketStats> {
    return this.http.get<TicketStats>(`${this.adminBaseUrl}/stats`);
  }

  /** GET /admin/support/tickets - List all tickets with filters */
  listTickets(filters: TicketFilters): Observable<{ total: number; page: number; limit: number; data: SupportTicket[] }> {
    return this.http.get<unknown>(this.adminBaseUrl, { params: this.buildTicketParams(filters) }).pipe(
      map((raw) => this.mapPagedTickets(raw)),
    );
  }

  /** GET /support/tickets - List tickets for the signed-in user or vendor */
  listMyTickets(
    filters: TicketFilters,
    submitterType: TicketSubmitterType,
  ): Observable<SubmittedTicketRecord[]> {
    const params = this.buildTicketParams({ ...filters, type: submitterType });
    return this.http.get<unknown>(this.userTicketsUrl, { params }).pipe(
      map((raw) => this.mapPagedTickets(raw).data.map((ticket) => this.toSubmittedRecord(ticket, submitterType))),
    );
  }

  /** GET /admin/support/tickets/{ticket_id} - Get single ticket */
  getTicket(ticketId: string): Observable<SupportTicket> {
    return this.http.get<unknown>(`${this.adminBaseUrl}/${ticketId}`).pipe(
      map((raw) => this.toSupportTicket(raw)),
    );
  }

  /** GET /support/tickets/{ticket_id} - Get own ticket */
  getMyTicket(ticketId: string): Observable<SupportTicket> {
    return this.http.get<unknown>(`${this.userTicketsUrl}/${ticketId}`).pipe(
      map((raw) => this.toSupportTicket(raw)),
    );
  }

  /** POST /admin/support/tickets/{ticket_id}/reply - Reply to ticket */
  reply(ticketId: string, payload: ReplyTicketRequest): Observable<TicketReply> {
    return this.http.post<TicketReply>(`${this.adminBaseUrl}/${ticketId}/reply`, payload);
  }

  /** POST /admin/support/tickets/{ticket_id}/assign - Assign ticket to agent */
  assign(ticketId: string, payload: AssignTicketRequest): Observable<any> {
    return this.http.post<any>(`${this.adminBaseUrl}/${ticketId}/assign`, payload);
  }

  /** PATCH /admin/support/tickets/{ticket_id}/resolve - Mark ticket as resolved */
  resolve(ticketId: string, payload: ResolveTicketRequest): Observable<any> {
    return this.http.patch<any>(`${this.adminBaseUrl}/${ticketId}/resolve`, payload);
  }

  /** POST /support/tickets/{ticketId}/escalate — user/vendor route (not under /admin) */
  escalate(ticketId: string, payload: EscalateTicketRequest): Observable<any> {
    return this.http.post<any>(`${this.userTicketsUrl}/${ticketId}/escalate`, payload);
  }

  /** POST /support/tickets - Open a support ticket (Vendor, Customer) */
  openTicket(payload: CreateTicketRequest): Observable<SupportTicket> {
    return this.http.post<unknown>(this.userTicketsUrl, payload).pipe(
      map((raw) => this.toSupportTicket(raw)),
    );
  }

  private buildTicketParams(filters: TicketFilters): HttpParams {
    let params = new HttpParams();
    if (filters.status) params = params.set('status', filters.status);
    if (filters.priority) params = params.set('priority', filters.priority);
    if (filters.type) params = params.set('type', filters.type);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.limit) params = params.set('limit', filters.limit.toString());
    return params;
  }

  private mapPagedTickets(raw: unknown): { total: number; page: number; limit: number; data: SupportTicket[] } {
    const body = (raw && typeof raw === 'object' ? raw : {}) as Record<string, unknown>;
    const items = pickField<unknown[]>(body, 'data', 'Data') ?? [];
    return {
      total: Number(pickField<number>(body, 'total', 'Total') ?? items.length),
      page: Number(pickField<number>(body, 'page', 'Page') ?? 1),
      limit: Number(pickField<number>(body, 'limit', 'Limit') ?? items.length),
      data: items.map((item) => this.toSupportTicket(item)),
    };
  }

  private toSupportTicket(raw: unknown): SupportTicket {
    const normalized = normalizeTicketResponse(
      raw && typeof raw === 'object' ? (raw as Record<string, unknown>) : {},
    );
    return {
      ticket_id: String(normalized['ticket_id'] ?? ''),
      title: String(normalized['title'] ?? ''),
      from: String(normalized['from'] ?? ''),
      type: (normalized['type'] as SupportTicket['type']) || 'Client',
      priority: (normalized['priority'] as SupportTicket['priority']) || 'medium',
      status: (normalized['status'] as SupportTicket['status']) || 'open',
      opened_at: String(normalized['opened_at'] ?? new Date().toISOString()),
      description: String(normalized['description'] ?? ''),
      booking_ref: (normalized['booking_ref'] as string | null) ?? null,
    };
  }

  private toSubmittedRecord(ticket: SupportTicket, submitterType: TicketSubmitterType): SubmittedTicketRecord {
    return {
      ticket_id: ticket.ticket_id,
      title: ticket.title,
      category: extractCategoryFromDescription(ticket.description),
      priority: ticket.priority,
      status: ticket.status,
      opened_at: ticket.opened_at,
      booking_ref: ticket.booking_ref,
      submitter_type: submitterType,
    };
  }
}
