import { CreateTicketRequest, SupportTicket, TicketReply } from '../types/api.interfaces';

export type TicketSubmitterType = 'Client' | 'Vendor';
export type TicketCategory = 'Booking' | 'Payment' | 'Technical' | 'General';

export const TICKET_CATEGORIES: { value: TicketCategory; label: string }[] = [
  { value: 'Booking', label: 'Booking Issue' },
  { value: 'Payment', label: 'Payment / Refund' },
  { value: 'Technical', label: 'Technical Glitch' },
  { value: 'General', label: 'General Inquiry' },
];

export interface BuildTicketPayloadInput {
  submitterType: TicketSubmitterType;
  category: TicketCategory;
  title: string;
  description: string;
  priority: string;
  bookingRef?: string | null;
  contactName?: string;
  contactEmail?: string;
}

/** Backend `Type` is submitter role (Client/Vendor), not issue category — embed category in description. */
export function buildCreateTicketPayload(input: BuildTicketPayloadInput): CreateTicketRequest {
  const lines = [`Category: ${input.category}`];

  if (input.contactName?.trim()) {
    lines.push(`Contact name: ${input.contactName.trim()}`);
  }
  if (input.contactEmail?.trim()) {
    lines.push(`Contact email: ${input.contactEmail.trim()}`);
  }
  if (input.bookingRef?.trim()) {
    lines.push(`Booking reference: ${input.bookingRef.trim()}`);
  }

  lines.push('', input.description.trim());

  return {
    title: input.title.trim(),
    description: lines.join('\n'),
    type: input.submitterType,
    priority: input.priority,
    bookingRef: input.bookingRef?.trim() || null,
  };
}

export function pickField<T>(obj: Record<string, unknown>, ...keys: string[]): T | undefined {
  for (const key of keys) {
    const value = obj[key];
    if (value !== undefined && value !== null) return value as T;
  }
  return undefined;
}

export function normalizeTicketId(raw: Record<string, unknown> | null | undefined): string {
  if (!raw) return '';
  const id = raw['ticket_id'] ?? raw['ticketId'] ?? raw['TicketId'];
  return typeof id === 'string' ? id : '';
}

export function normalizeTicketStatus(status: unknown): SupportTicket['status'] {
  const raw = String(status ?? 'open').toLowerCase().replace(/[\s_-]+/g, '');
  if (raw === 'inprogress') return 'in_progress';
  if (raw === 'resolved') return 'resolved';
  return 'open';
}

export function mapTicketReply(raw: unknown, ticketId = ''): TicketReply {
  const obj = (raw && typeof raw === 'object' ? raw : {}) as Record<string, unknown>;
  const notified = pickField<string[]>(obj, 'notifiedVia', 'NotifiedVia') ?? [];
  return {
    reply_id: String(pickField(obj, 'replyId', 'ReplyId', 'reply_id') ?? ''),
    ticket_id: String(pickField(obj, 'ticketId', 'TicketId', 'ticket_id') ?? ticketId),
    message: String(pickField(obj, 'message', 'Message') ?? ''),
    replied_by: String(pickField(obj, 'repliedBy', 'RepliedBy', 'replied_by') ?? ''),
    replied_at: String(pickField(obj, 'repliedAt', 'RepliedAt', 'replied_at') ?? new Date().toISOString()),
    notified_via: Array.isArray(notified) ? notified : [],
  };
}

function mapAssignedTo(raw: Record<string, unknown>): SupportTicket['assigned_to'] {
  const nested = pickField<Record<string, unknown>>(raw, 'assignedTo', 'AssignedTo');
  if (nested && typeof nested === 'object') {
    const name = pickField<string>(nested, 'name', 'Name');
    const agentId = pickField<string>(nested, 'agentId', 'AgentId', 'agent_id');
    if (name || agentId) {
      return { name: String(name ?? ''), agent_id: String(agentId ?? '') };
    }
  }

  const assignedName = pickField<string>(raw, 'assignedTo', 'AssignedTo');
  if (typeof assignedName === 'string' && assignedName.trim()) {
    return { name: assignedName, agent_id: '' };
  }

  return null;
}

function mapTicketReplies(raw: Record<string, unknown>, ticketId: string): TicketReply[] {
  const replies = pickField<unknown[]>(raw, 'replies', 'Replies') ?? [];
  return replies.map((item) => mapTicketReply(item, ticketId));
}

export function mapSupportTicket(raw: unknown): SupportTicket {
  if (!raw || typeof raw !== 'object') {
    return {
      ticket_id: '',
      title: '',
      from: '',
      type: 'Client',
      priority: 'medium',
      status: 'open',
      opened_at: new Date().toISOString(),
      description: '',
    };
  }

  const obj = raw as Record<string, unknown>;
  const ticketId = normalizeTicketId(obj);
  const type = String(pickField(obj, 'type', 'Type') ?? 'Client');
  const priority = String(pickField(obj, 'priority', 'Priority') ?? 'medium').toLowerCase();

  return {
    ticket_id: ticketId,
    title: String(pickField(obj, 'title', 'Title') ?? ''),
    from: String(pickField(obj, 'from', 'From') ?? ''),
    type: (type === 'Vendor' ? 'Vendor' : 'Client'),
    priority: (['critical', 'high', 'medium', 'low'].includes(priority)
      ? priority
      : 'medium') as SupportTicket['priority'],
    status: normalizeTicketStatus(pickField(obj, 'status', 'Status')),
    opened_at: String(pickField(obj, 'opened_at', 'openedAt', 'OpenedAt') ?? new Date().toISOString()),
    description: String(pickField(obj, 'description', 'Description') ?? ''),
    booking_ref: (pickField<string | null>(obj, 'booking_ref', 'bookingRef', 'BookingRef') ?? null),
    assigned_to: mapAssignedTo(obj),
    resolved_at: pickField<string | null>(obj, 'resolved_at', 'resolvedAt', 'ResolvedAt') ?? null,
    replies: mapTicketReplies(obj, ticketId),
  };
}

/** @deprecated use mapSupportTicket */
export function normalizeTicketResponse(raw: unknown): Record<string, unknown> {
  const ticket = mapSupportTicket(raw);
  return { ...ticket };
}

export function mapSubjectToCategory(subject: string): TicketCategory {
  const normalized = subject.toLowerCase();
  if (normalized.includes('booking')) return 'Booking';
  if (normalized.includes('payment') || normalized.includes('refund') || normalized.includes('payout')) {
    return 'Payment';
  }
  if (normalized.includes('technical') || normalized.includes('glitch')) return 'Technical';
  return 'General';
}

export function extractCategoryFromDescription(description: string): string {
  const match = description?.match(/Category:\s*(.+)/i);
  return match?.[1]?.trim() || 'General';
}
