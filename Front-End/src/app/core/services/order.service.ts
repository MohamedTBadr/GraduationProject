import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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

/** Matches the backend Result<T> JSON shape returned by Ok(order) */
export interface ResultWrapper<T> {
  isSuccess: boolean;
  isFailure: boolean;
  value: T;
  error: { code: number; type: string; description: string } | null;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  createOrder(payload: CreateOrderPayload): Observable<ResultWrapper<OrderResponse>> {
    return this.http.post<ResultWrapper<OrderResponse>>(`${this.apiUrl}/Order`, payload);
  }

  getOrdersByUser(userId: string): Observable<OrderResponse[]> {
    return this.http.get<OrderResponse[]>(`${this.apiUrl}/Order/user/${userId}`);
  }
}
