import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface CreateOrderPayload {
  userId: string;
  eventId: string;
  currency: string;
  voucherCode?: string;
  shippingAddress: {
    street: string;
    city: string;
    state: string;
    postalCode: string;
  };
  appointment: string;
}

export interface OrderResponse {
  id: string;
  userId: string;
  amount: number;
  currency: string;
  paymentIntentId: string | null;
  paymentStatus: string;
  createdAt: string;
  appointment: string;
  eventId: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  createOrder(payload: CreateOrderPayload): Observable<OrderResponse> {
    return this.http.post<any>(`${this.apiUrl}/Order`, payload).pipe(
      map(o => this.normalizeOrder(o))
    );
  }

  getAllOrders(): Observable<OrderResponse[]> {
    return this.http.get<any>(`${this.apiUrl}/Order`).pipe(
      map(res => this.mapOrderList(res))
    );
  }

  getOrdersByUser(userId: string): Observable<OrderResponse[]> {
    return this.http.get<any>(`${this.apiUrl}/Order/user/${userId}`).pipe(
      map(res => this.mapOrderList(res))
    );
  }

  /** Resolve order id when create response body is empty but order exists server-side */
  findLatestOrderForEvent(userId: string, eventId: string): Observable<OrderResponse | null> {
    return this.getOrdersByUser(userId).pipe(
      map(orders => {
        const matches = orders
          .filter(o => o.eventId === eventId)
          .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        return matches[0] ?? null;
      })
    );
  }

  getOrderById(id: string): Observable<OrderResponse> {
    return this.http.get<any>(`${this.apiUrl}/Order/${id}`).pipe(
      map(o => this.normalizeOrder(o))
    );
  }

  private mapOrderList(res: any): OrderResponse[] {
    const data = res?.value ?? res?.Value ?? res;
    const items = Array.isArray(data) ? data : (data?.items ?? data?.Items ?? []);
    return (Array.isArray(items) ? items : []).map((o: any) => this.normalizeOrder(o));
  }

  private unwrapOrderBody(o: any): any {
    if (!o || typeof o !== 'object') return o;
    const nested = o.value ?? o.Value;
    if (nested && typeof nested === 'object' && (nested.id ?? nested.Id)) {
      return nested;
    }
    return o;
  }

  private normalizeOrder(o: any): OrderResponse {
    const raw = this.unwrapOrderBody(o);
    if (!raw || typeof raw !== 'object') {
      return {
        id: '', userId: '', amount: 0, currency: 'EGP',
        paymentIntentId: null, paymentStatus: '', createdAt: '', appointment: '', eventId: ''
      };
    }

    return {
      id:              String(raw.id ?? raw.Id ?? ''),
      userId:          String(raw.userId ?? raw.UserId ?? ''),
      amount:          Number(raw.amount ?? raw.Amount ?? 0),
      currency:        String(raw.currency ?? raw.Currency ?? 'EGP'),
      paymentIntentId: raw.paymentIntentId ?? raw.PaymentIntentId ?? null,
      paymentStatus:   String(raw.paymentStatus ?? raw.PaymentStatus ?? ''),
      createdAt:       String(raw.createdAt ?? raw.CreatedAt ?? ''),
      appointment:     String(raw.appointment ?? raw.Appointment ?? ''),
      eventId:         String(raw.eventId ?? raw.EventId ?? '')
    };
  }
}
