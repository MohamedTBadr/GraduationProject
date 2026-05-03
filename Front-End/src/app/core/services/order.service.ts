import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CreateOrderPayload {
  userId: string;
  eventId: string;
  currency: string;
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
    return this.http.post<OrderResponse>(`${this.apiUrl}/Order`, payload);
  }

  getOrdersByUser(userId: string): Observable<OrderResponse[]> {
    return this.http.get<OrderResponse[]>(`${this.apiUrl}/Order/user/${userId}`);
  }
}
