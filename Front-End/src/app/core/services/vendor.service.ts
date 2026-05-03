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
        const data = res?.value || res?.Value || res;
        const items = Array.isArray(data) ? data : (data?.items || data?.Items || []);
        if (!Array.isArray(items)) return [];
        return items.map((v: any) => this.normalizeVendor(v));
      })
    );
  }

  getById(vendorId: string): Observable<ApiVendor> {
    if (!vendorId) throw new Error('Vendor ID is required');
    return this.http.get<any>(`${this.apiUrl}/Vendor/${vendorId}`).pipe(
      map((res: any) => {
        const v = res?.value || res?.Value || res;
        return this.normalizeVendor(v);
      })
    );
  }

  private normalizeVendor(v: any): ApiVendor {
    if (!v) return {} as ApiVendor;
    return {
      ...v,
      id: v.userId || v.UserId || v.id || v.Id || v.vendorId || v.VendorId,
      name: v.businessName || v.BusinessName || v.name || v.Name || 'Unknown Vendor',
      vendorTypeName: v.vendorTypeName || v.VendorTypeName || v.serviceType || v.ServiceType || v.categoryName || v.CategoryName || 'Vendor',
      about: v.description || v.Description || v.about || v.About || '',
      status: v.status || v.Status || 'active',
      isApproved: v.isApproved !== undefined ? v.isApproved : (v.IsApproved !== undefined ? v.IsApproved : true),
      createdAt: v.createdAt || v.CreatedAt || new Date(),
      rating: v.rating || v.Rating || 0,
      location: v.location || v.Location || v.address || v.Address || ''
    } as ApiVendor;
  }

  /** POST /Vendor – register a new vendor */
  create(payload: CreateVendorRequest): Observable<ApiVendor> {
    return this.http.post<ApiVendor>(`${this.apiUrl}/Vendor`, payload);
  }

  /** PATCH /Vendor/{vendorId}/approve – admin approves vendor */
  approve(vendorId: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/Vendor/${vendorId}/approve`, {});
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
