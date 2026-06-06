import { CreateTicketRequest } from '../types/api.interfaces';

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

export function normalizeTicketId(raw: Record<string, unknown> | null | undefined): string {
  if (!raw) return '';
  const id = raw['ticket_id'] ?? raw['ticketId'] ?? raw['TicketId'];
  return typeof id === 'string' ? id : '';
}

export function normalizeTicketResponse(raw: unknown): Record<string, unknown> {
  if (!raw || typeof raw !== 'object') return {};
  const obj = raw as Record<string, unknown>;
  return {
    ticket_id: normalizeTicketId(obj),
    title: obj['title'] ?? obj['Title'] ?? '',
    from: obj['from'] ?? obj['From'] ?? '',
    type: obj['type'] ?? obj['Type'] ?? '',
    priority: obj['priority'] ?? obj['Priority'] ?? '',
    status: obj['status'] ?? obj['Status'] ?? 'open',
    opened_at: obj['opened_at'] ?? obj['openedAt'] ?? obj['OpenedAt'] ?? new Date().toISOString(),
    description: obj['description'] ?? obj['Description'] ?? '',
    booking_ref: obj['booking_ref'] ?? obj['bookingRef'] ?? obj['BookingRef'] ?? null,
  };
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
