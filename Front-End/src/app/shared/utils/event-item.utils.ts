import { EventItemResponseDto } from '../types/api.interfaces';

export function itemUnitPrice(item: EventItemResponseDto): number {
  return Number(item.price ?? 0);
}

export function itemLineTotal(item: EventItemResponseDto): number {
  return itemUnitPrice(item) * (item.quantity ?? 1);
}

export function approvedItems(items: EventItemResponseDto[] | undefined): EventItemResponseDto[] {
  return (items ?? []).filter(i => i.itemStatus === 'Approved');
}

export function pendingApprovalItems(items: EventItemResponseDto[] | undefined): EventItemResponseDto[] {
  return (items ?? []).filter(i => i.itemStatus === 'Pending');
}

export function paidItems(items: EventItemResponseDto[] | undefined): EventItemResponseDto[] {
  return (items ?? []).filter(
    i => i.itemStatus === 'Paid' || i.itemStatus === 'Done' || i.itemStatus === 'Completed'
  );
}

export function approvedAmount(items: EventItemResponseDto[] | undefined): number {
  return approvedItems(items).reduce((sum, i) => sum + itemLineTotal(i), 0);
}

/** Budget committed by non-rejected services (includes pending approval). */
export function budgetCommittedAmount(items: EventItemResponseDto[] | undefined): number {
  return (items ?? [])
    .filter(i => i.itemStatus !== 'Rejected')
    .reduce((sum, i) => sum + itemLineTotal(i), 0);
}

export function itemStatusLabel(item: EventItemResponseDto): string {
  const s = item.itemStatus || '';
  if (s === 'Approved') return 'Approved — Ready to Pay';
  if (s === 'Paid') return 'Paid';
  if (s === 'Done') return 'Done';
  if (s === 'Completed') return 'Completed';
  if (s === 'Rejected') return 'Rejected';
  return 'Awaiting Confirmation';
}

export function itemBadgeClass(item: EventItemResponseDto): string {
  const s = (item.itemStatus || '').toLowerCase();
  if (s === 'approved') return 'badge-confirmed';
  if (s === 'paid' || s === 'done' || s === 'completed') return 'badge-paid';
  if (s === 'rejected') return 'badge-rejected';
  return 'badge-pending';
}
