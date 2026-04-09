import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ApiVendor,
  CreateVendorRequest,
  UpdateVendorRequest
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class VendorService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** GET /Vendor – returns all vendors */
  getAll(): Observable<ApiVendor[]> {
    return this.http.get<any>(`${this.apiUrl}/Vendor`).pipe(
      map(res => {
        const items = res.value || res;
        if (!Array.isArray(items)) return [];
        return items.map((v: any) => ({
          ...v,
          id: v.userId || v.id,
          name: v.businessName || v.name || 'Unknown Vendor',
          categoryName: v.serviceType || v.categoryName || 'Vendor',
          about: v.description || v.about,
          status: v.status || 'active',
          isApproved: v.isApproved !== undefined ? v.isApproved : true
        } as ApiVendor));
      })
    );
  }

  /** GET /Vendor/{vendorId} */
  getById(vendorId: string): Observable<ApiVendor> {
    return this.http.get<ApiVendor>(`${this.apiUrl}/Vendor/${vendorId}`);
  }

  /** POST /Vendor – register a new vendor */
  create(payload: CreateVendorRequest): Observable<ApiVendor> {
    return this.http.post<ApiVendor>(`${this.apiUrl}/Vendor`, payload);
  }

  /** PATCH /vendor/{vendorId}/approve – admin approves vendor */
  approve(vendorId: string): Observable<ApiVendor> {
    return this.http.patch<ApiVendor>(`${this.apiUrl}/vendor/${vendorId}/approve`, {});
  }

  /** PATCH /Vendor/{vendorId} – update vendor info */
  update(vendorId: string, payload: UpdateVendorRequest): Observable<ApiVendor> {
    return this.http.patch<ApiVendor>(`${this.apiUrl}/Vendor/${vendorId}`, payload);
  }

  /** DELETE /Vendor/{vendorId} */
  delete(vendorId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Vendor/${vendorId}`);
  }
}
