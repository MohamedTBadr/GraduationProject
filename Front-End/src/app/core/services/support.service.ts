import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
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

const SUBMITTED_TICKETS_KEY = 'epichub_submitted_tickets';

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
    let params = new HttpParams();
    if (filters.status) params = params.set('status', filters.status);
    if (filters.priority) params = params.set('priority', filters.priority);
    if (filters.type) params = params.set('type', filters.type);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.limit) params = params.set('limit', filters.limit.toString());

    return this.http.get<any>(this.adminBaseUrl, { params });
  }

  /** GET /admin/support/tickets/{ticket_id} - Get single ticket */
  getTicket(ticketId: string): Observable<SupportTicket> {
    return this.http.get<SupportTicket>(`${this.adminBaseUrl}/${ticketId}`);
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
  openTicket(payload: CreateTicketRequest, category = 'General'): Observable<SupportTicket> {
    return this.http.post<unknown>(this.userTicketsUrl, payload).pipe(
      map((raw) => this.toSupportTicket(raw)),
      tap((ticket) => this.cacheSubmittedTicket(ticket, category, payload.type as TicketSubmitterType)),
    );
  }

  getSubmittedTickets(submitterType?: TicketSubmitterType): SubmittedTicketRecord[] {
    const all = this.readSubmittedTickets();
    if (!submitterType) return all;
    return all.filter((t) => t.submitter_type === submitterType);
  }

  private toSupportTicket(raw: unknown): SupportTicket {
    const normalized = normalizeTicketResponse(raw);
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

  private cacheSubmittedTicket(
    ticket: SupportTicket,
    category: string,
    submitterType: TicketSubmitterType,
  ): void {
    if (!ticket.ticket_id) return;

    const record: SubmittedTicketRecord = {
      ticket_id: ticket.ticket_id,
      title: ticket.title,
      category,
      priority: ticket.priority,
      status: ticket.status,
      opened_at: ticket.opened_at,
      booking_ref: ticket.booking_ref,
      submitter_type: submitterType,
    };

    const existing = this.readSubmittedTickets().filter((t) => t.ticket_id !== record.ticket_id);
    existing.unshift(record);
    localStorage.setItem(SUBMITTED_TICKETS_KEY, JSON.stringify(existing.slice(0, 50)));
  }

  private readSubmittedTickets(): SubmittedTicketRecord[] {
    try {
      const raw = localStorage.getItem(SUBMITTED_TICKETS_KEY);
      if (!raw) return [];
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }
}
