import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaymobPaymentRequest, PaymobPaymentResponse } from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * POST /payments/paymob
   * Initiates a Paymob payment flow and returns a redirect URL or payment key.
   */
  initiatePaymob(payload: PaymobPaymentRequest): Observable<PaymobPaymentResponse> {
    return this.http.post<PaymobPaymentResponse>(`${this.apiUrl}/payments/paymob`, payload);
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
