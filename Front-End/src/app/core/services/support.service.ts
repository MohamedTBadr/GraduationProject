import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  SupportTicket,
  TicketStats,
  TicketFilters,
  ReplyTicketRequest,
  AssignTicketRequest,
  ResolveTicketRequest,
  EscalateTicketRequest,
  TicketReply
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class SupportService {
  private readonly baseUrl = `${environment.apiUrl}/admin/support/tickets`;

  constructor(private http: HttpClient) {}

  /** GET /admin/support/tickets/stats - Get ticket stats */
  getStats(): Observable<TicketStats> {
    return this.http.get<TicketStats>(`${this.baseUrl}/stats`);
  }

  /** GET /admin/support/tickets - List all tickets with filters */
  listTickets(filters: TicketFilters): Observable<{ total: number; page: number; limit: number; data: SupportTicket[] }> {
    let params = new HttpParams();
    if (filters.status) params = params.set('status', filters.status);
    if (filters.priority) params = params.set('priority', filters.priority);
    if (filters.type) params = params.set('type', filters.type);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.limit) params = params.set('limit', filters.limit.toString());

    return this.http.get<any>(this.baseUrl, { params });
  }

  /** GET /admin/support/tickets/{ticket_id} - Get single ticket */
  getTicket(ticketId: string): Observable<SupportTicket> {
    return this.http.get<SupportTicket>(`${this.baseUrl}/${ticketId}`);
  }

  /** POST /admin/support/tickets/{ticket_id}/reply - Reply to ticket */
  reply(ticketId: string, payload: ReplyTicketRequest): Observable<TicketReply> {
    return this.http.post<TicketReply>(`${this.baseUrl}/${ticketId}/reply`, payload);
  }

  /** POST /admin/support/tickets/{ticket_id}/assign - Assign ticket to agent */
  assign(ticketId: string, payload: AssignTicketRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${ticketId}/assign`, payload);
  }

  /** PATCH /admin/support/tickets/{ticket_id}/resolve - Mark ticket as resolved */
  resolve(ticketId: string, payload: ResolveTicketRequest): Observable<any> {
    return this.http.patch<any>(`${this.baseUrl}/${ticketId}/resolve`, payload);
  }

  /** POST /admin/support/tickets/{ticket_id}/escalate - Escalate ticket */
  escalate(ticketId: string, payload: EscalateTicketRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${ticketId}/escalate`, payload);
  }
}
