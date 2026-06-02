import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Voucher {
  code: string;
  discountPercent: number;
  expiresAt?: string;
  isUsed: boolean;
  description?: string;
}

export interface VoucherValidationResult {
  isValid: boolean;
  discountPercent?: number;
  message?: string;
  errorMessage?: string;
}

@Injectable({ providedIn: 'root' })
export class VoucherService {
  private readonly apiUrl = `${environment.apiUrl}/vouchers`;

  constructor(private http: HttpClient) {}

  /** GET /api/vouchers/referral-link — returns a plain string */
  getReferralLink(): Observable<string> {
    return this.http.get(`${this.apiUrl}/referral-link`, { responseType: 'text' });
  }

  /** GET /api/vouchers/my */
  getMyVouchers(): Observable<Voucher[]> {
    return this.http.get<Voucher[]>(`${this.apiUrl}/my`);
  }

  /** GET /api/vouchers/validate?code=XXX */
  validateVoucher(code: string): Observable<VoucherValidationResult> {
    return this.http.get<VoucherValidationResult>(`${this.apiUrl}/validate`, {
      params: { code }
    });
  }
}
