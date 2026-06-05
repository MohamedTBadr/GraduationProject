import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaymobFreeResponse, PaymobPaymentRequest } from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // POST /payments/paymob — sends only orderId; backend fetches amount + billing.
  // Returns a string (iframe URL) for paid orders, or PaymobFreeResponse for free orders.
  initiatePaymob(payload: PaymobPaymentRequest): Observable<string | PaymobFreeResponse> {
    return this.http.post<string | PaymobFreeResponse>(`${this.apiUrl}/payments/paymob`, payload);
  }

  /**
   * POST /payments/paymob/webhook
   * Called by Paymob to notify about payment status updates.
   * Normally handled server-side, but exposed here for completeness.
   */
  paymobWebhook(payload: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/payments/paymob/webhook`, payload);
  }
}
