import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface CreateOrderPayload {
  userId: string;
  eventId: string;
  currency: string;
  voucherCode?: string; // Optional discount voucher code
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

  // POST /Order — backend returns Result<OrderResponse> wrapper which the interceptor
  // unwraps; the inner value arrives as PascalCase, so we normalise it.
  createOrder(payload: CreateOrderPayload): Observable<OrderResponse> {
    return this.http.post<any>(`${this.apiUrl}/Order`, payload).pipe(
      map(o => this.normalizeOrder(o))
    );
  }

  getOrdersByUser(userId: string): Observable<OrderResponse[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Order/user/${userId}`).pipe(
      map(orders => orders.map(o => this.normalizeOrder(o)))
    );
  }

  getOrderById(id: string): Observable<OrderResponse> {
    return this.http.get<any>(`${this.apiUrl}/Order/${id}`).pipe(
      map(o => this.normalizeOrder(o))
    );
  }

  // Handles both PascalCase (API) and camelCase (already-normalized) responses
  private normalizeOrder(o: any): OrderResponse {
    return {
      id:             o.id             ?? o.Id             ?? '',
      userId:         o.userId         ?? o.UserId         ?? '',
      amount:         o.amount         ?? o.Amount         ?? 0,
      currency:       o.currency       ?? o.Currency       ?? 'EGP',
      paymentIntentId:o.paymentIntentId?? o.PaymentIntentId?? null,
      paymentStatus:  o.paymentStatus  ?? o.PaymentStatus  ?? '',
      createdAt:      o.createdAt      ?? o.CreatedAt      ?? '',
      appointment:    o.appointment    ?? o.Appointment    ?? '',
      eventId:        o.eventId        ?? o.EventId        ?? '',
    };
  }
}
