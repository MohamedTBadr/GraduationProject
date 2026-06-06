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
  extractCategoryFromDescription,
  pickField,
  mapSupportTicket,
  mapTicketReply,
  normalizeTicketStatus,
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
    return this.http.get<unknown>(`${this.adminBaseUrl}/stats`).pipe(
      map((raw) => this.mapTicketStats(raw)),
    );
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
      map((raw) => mapSupportTicket(raw)),
    );
  }

  /** GET /support/tickets/{ticket_id} - Get own ticket */
  getMyTicket(ticketId: string): Observable<SupportTicket> {
    return this.http.get<unknown>(`${this.userTicketsUrl}/${ticketId}`).pipe(
      map((raw) => mapSupportTicket(raw)),
    );
  }

  /** POST /admin/support/tickets/{ticket_id}/reply - Reply to ticket */
  reply(ticketId: string, payload: ReplyTicketRequest): Observable<TicketReply> {
    return this.http.post<unknown>(`${this.adminBaseUrl}/${ticketId}/reply`, {
      message: payload.message,
      sendEmail: payload.sendEmail ?? true,
      sendSms: payload.sendSms ?? false,
    }).pipe(map((raw) => mapTicketReply(raw, ticketId)));
  }

  /** POST /admin/support/tickets/{ticket_id}/assign - Assign ticket to agent */
  assign(ticketId: string, payload: AssignTicketRequest): Observable<{
    status: SupportTicket['status'];
    assigned_to: SupportTicket['assigned_to'];
  }> {
    return this.http.post<unknown>(`${this.adminBaseUrl}/${ticketId}/assign`, {
      agentId: payload.agentId,
      note: payload.note ?? null,
    }).pipe(map((raw) => this.mapAssignResponse(raw)));
  }

  /** PATCH /admin/support/tickets/{ticket_id}/resolve - Mark ticket as resolved */
  resolve(ticketId: string, payload: ResolveTicketRequest): Observable<{ resolved_at: string }> {
    return this.http.patch<unknown>(`${this.adminBaseUrl}/${ticketId}/resolve`, {
      resolutionNote: payload.resolutionNote,
    }).pipe(map((raw) => ({
      resolved_at: String(pickField<string>(raw as Record<string, unknown>, 'resolvedAt', 'ResolvedAt') ?? new Date().toISOString()),
    })));
  }

  /** POST /admin/support/tickets/{ticketId}/escalate — admin escalation */
  adminEscalate(ticketId: string, payload: EscalateTicketRequest): Observable<unknown> {
    return this.http.post<unknown>(`${this.adminBaseUrl}/${ticketId}/escalate`, {
      reason: payload.reason,
      escalateTo: payload.escalateTo,
      notifyFinance: payload.notifyFinance ?? false,
    });
  }

  /** POST /support/tickets/{ticketId}/escalate — user/vendor escalation */
  escalate(ticketId: string, payload: EscalateTicketRequest): Observable<unknown> {
    return this.http.post<unknown>(`${this.userTicketsUrl}/${ticketId}/escalate`, {
      reason: payload.reason,
      escalateTo: payload.escalateTo,
      notifyFinance: payload.notifyFinance ?? false,
    });
  }

  /** POST /support/tickets - Open a support ticket (Vendor, Customer) */
  openTicket(payload: CreateTicketRequest): Observable<SupportTicket> {
    return this.http.post<unknown>(this.userTicketsUrl, payload).pipe(
      map((raw) => mapSupportTicket(raw)),
    );
  }

  private buildTicketParams(filters: TicketFilters): HttpParams {
    let params = new HttpParams();
    if (filters.status) params = params.set('status', this.toApiStatus(filters.status));
    if (filters.priority) params = params.set('priority', filters.priority);
    if (filters.type) params = params.set('type', filters.type);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.limit) params = params.set('limit', filters.limit.toString());
    return params;
  }

  /** Backend enum serializes as InProgress → "inprogress" after ToLower(). */
  private toApiStatus(status: TicketFilters['status']): string {
    if (status === 'in_progress') return 'inprogress';
    return status ?? 'open';
  }

  private mapTicketStats(raw: unknown): TicketStats {
    const obj = (raw && typeof raw === 'object' ? raw : {}) as Record<string, unknown>;
    return {
      critical: Number(pickField(obj, 'critical', 'Critical') ?? 0),
      open: Number(pickField(obj, 'open', 'Open') ?? 0),
      in_progress: Number(pickField(obj, 'in_progress', 'inProgress', 'InProgress') ?? 0),
      resolution_rate: Number(pickField(obj, 'resolution_rate', 'resolutionRate', 'ResolutionRate') ?? 0),
    };
  }

  private mapPagedTickets(raw: unknown): { total: number; page: number; limit: number; data: SupportTicket[] } {
    const body = (raw && typeof raw === 'object' ? raw : {}) as Record<string, unknown>;
    const items = pickField<unknown[]>(body, 'data', 'Data') ?? [];
    return {
      total: Number(pickField<number>(body, 'total', 'Total') ?? items.length),
      page: Number(pickField<number>(body, 'page', 'Page') ?? 1),
      limit: Number(pickField<number>(body, 'limit', 'Limit') ?? items.length),
      data: items.map((item) => mapSupportTicket(item)),
    };
  }

  private mapAssignResponse(raw: unknown): {
    status: SupportTicket['status'];
    assigned_to: SupportTicket['assigned_to'];
  } {
    const obj = (raw && typeof raw === 'object' ? raw : {}) as Record<string, unknown>;
    const assignedRaw = pickField<Record<string, unknown>>(obj, 'assignedTo', 'AssignedTo');
    const assigned_to = assignedRaw
      ? {
          agent_id: String(pickField(assignedRaw, 'agentId', 'AgentId') ?? ''),
          name: String(pickField(assignedRaw, 'name', 'Name') ?? ''),
        }
      : null;

    return {
      status: normalizeTicketStatus(pickField(obj, 'status', 'Status')),
      assigned_to,
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
