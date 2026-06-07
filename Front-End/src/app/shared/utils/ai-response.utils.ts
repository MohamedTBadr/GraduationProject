import {
  BudgetAllocationResponse,
  BudgetCategory,
  EventTimelineResponse,
  TimelineItem
} from '../types/api.interfaces';

/** Unwrap `{ value }`, `{ Value }`, or pass-through (response interceptor may already unwrap). */
export function unwrapApiValue<T>(res: unknown): T | null {
  if (res == null) return null;
  if (typeof res !== 'object') return res as T;

  const r = res as Record<string, unknown>;
  if (r['isSuccess'] === false || r['IsSuccess'] === false) return null;

  if (r['value'] !== undefined) return r['value'] as T;
  if (r['Value'] !== undefined) return r['Value'] as T;
  return res as T;
}

function pickField(obj: Record<string, unknown>, ...keys: string[]): unknown {
  for (const k of keys) {
    const v = obj[k];
    if (v !== undefined && v !== null) return v;
  }
  return undefined;
}

/** AI may return 0.25 meaning 25% or 25 meaning 25%. */
export function normalizePercentage(value: unknown): number {
  const n = Number(value ?? 0);
  if (!Number.isFinite(n) || n <= 0) return 0;
  return n <= 1 ? n * 100 : n;
}

function normalizeBudgetCategory(cat: unknown): BudgetCategory {
  const c = (cat ?? {}) as Record<string, unknown>;
  return {
    name: String(pickField(c, 'name', 'Name') ?? ''),
    amount: Number(pickField(c, 'amount', 'Amount') ?? 0),
    percentage: normalizePercentage(pickField(c, 'percentage', 'Percentage')),
    description: String(pickField(c, 'description', 'Description') ?? '')
  };
}

export function normalizeBudgetAllocation(
  raw: unknown,
  fallbackBudget = 0,
  fallbackType = ''
): BudgetAllocationResponse | null {
  const data = unwrapApiValue<unknown>(raw) ?? raw;
  if (!data || typeof data !== 'object') return null;

  const o = data as Record<string, unknown>;
  const categoriesRaw = pickField(o, 'categories', 'Categories');
  const categories = Array.isArray(categoriesRaw)
    ? categoriesRaw.map(normalizeBudgetCategory).filter(c => c.name)
    : [];

  const totalBudget = Number(pickField(o, 'totalBudget', 'TotalBudget') ?? fallbackBudget) || fallbackBudget;
  const eventType = String(pickField(o, 'eventType', 'EventType') ?? fallbackType);
  const advice = String(pickField(o, 'advice', 'Advice') ?? '');

  if (categories.length === 0) return null;

  return { totalBudget, eventType, categories, advice };
}

function normalizeTimelineItem(item: unknown): TimelineItem {
  const i = (item ?? {}) as Record<string, unknown>;
  return {
    time: String(pickField(i, 'time', 'Time') ?? ''),
    activity: String(pickField(i, 'activity', 'Activity') ?? ''),
    duration: String(pickField(i, 'duration', 'Duration') ?? ''),
    importance: String(pickField(i, 'importance', 'Importance') ?? 'Low')
  };
}

export function normalizeEventTimeline(raw: unknown): EventTimelineResponse | null {
  const data = unwrapApiValue<unknown>(raw) ?? raw;
  if (!data || typeof data !== 'object') return null;

  const o = data as Record<string, unknown>;
  const timelineRaw = pickField(o, 'timeline', 'Timeline');
  const timeline = Array.isArray(timelineRaw)
    ? timelineRaw.map(normalizeTimelineItem).filter(i => i.time || i.activity)
    : [];

  if (timeline.length === 0) return null;

  return {
    eventId: String(pickField(o, 'eventId', 'EventId') ?? ''),
    eventTitle: String(pickField(o, 'eventTitle', 'EventTitle') ?? ''),
    planningNotes: String(pickField(o, 'planningNotes', 'PlanningNotes') ?? ''),
    timeline
  };
}

/** Extract a human-readable message from API error payloads. */
export function extractApiErrorMessage(err: unknown, fallback: string): string {
  const e = err as { error?: Record<string, unknown>; message?: string };
  const body = e?.error;
  if (body && typeof body === 'object') {
    const desc = body['errorDescription'] ?? body['ErrorDescription'];
    if (typeof desc === 'string' && desc.trim()) return desc;
  }
  if (typeof e?.message === 'string' && e.message.trim()) return e.message;
  return fallback;
}
