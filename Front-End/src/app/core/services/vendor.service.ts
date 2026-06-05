import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ApiVendor,
  CreateVendorRequest,
  UpdateVendorRequest,
  PaginatedRequest
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class VendorService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** GET /Vendor – returns all vendors (supports pagination & filters) */
  getAll(filters?: PaginatedRequest): Observable<ApiVendor[]> {
    let params = new HttpParams();
    if (filters) {
      if (filters.pageIndex) params = params.set('pageIndex', filters.pageIndex.toString());
      if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
      if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
      if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
      if (filters.isDescending !== undefined) params = params.set('isDescending', filters.isDescending.toString());
      
      // Location
      if (filters.city) params = params.set('city', filters.city);
      if (filters.region) params = params.set('region', filters.region);
      if (filters.latitude) params = params.set('latitude', filters.latitude.toString());
      if (filters.longitude) params = params.set('longitude', filters.longitude.toString());
      if (filters.radiusKm) params = params.set('radiusKm', filters.radiusKm.toString());

      // Taxonomy
      if (filters.vendorTypeId) params = params.set('vendorTypeId', filters.vendorTypeId);
    }

    return this.http.get<any>(`${this.apiUrl}/Vendor`, { params }).pipe(
      map(res => {
        const data = res?.value ?? res;
        const items = Array.isArray(data) ? data : (data?.items ?? []);
        if (!Array.isArray(items)) return [];
        return items.map((v: any) => this.normalizeVendor(v));
      })
    );
  }

  getById(vendorId: string): Observable<ApiVendor> {
    if (!vendorId) throw new Error('Vendor ID is required');
    return this.http.get<any>(`${this.apiUrl}/Vendor/${vendorId}`).pipe(
      map((res: any) => this.normalizeVendor(res?.value ?? res))
    );
  }

  private normalizeVendor(v: any): ApiVendor {
    if (!v) return {} as ApiVendor;
    return {
      ...v,
      id: v.userId || v.id || v.vendorId,
      name: v.businessName || v.name || 'Unknown Vendor',
      vendorTypeId: v.vendorTypeId || v.vendorType?.id || '',
      vendorTypeName: v.vendorTypeName || v.serviceType || v.categoryName || 'Vendor',
      about: v.description || v.about || '',
      status: v.status || 'active',
      isApproved: v.isApproved ?? true,
      createdAt: v.createdAt || new Date(),
      rating: v.rating || 0,
      location: v.location || v.address || '',
      documentUrl: v.documentUrl || v.document,
      profilePictureUrl: v.profilePictureUrl || v.profilePicture,
      serviceAreas: (v.serviceAreas ?? []).map((sa: any) => ({
        ...sa,
        id: sa.id,
        city: sa.city,
        region: sa.region,
        latitude: sa.latitude ?? sa.lattitude ?? 0,
        longitude: sa.longitude ?? 0
      }))
    } as ApiVendor;
  }

  /** POST /Vendor – register a new vendor */
  create(payload: CreateVendorRequest | FormData): Observable<ApiVendor> {
    // If it's not FormData, we should probably convert it if it contains files
    // But usually the component will pass FormData if files are involved
    return this.http.post<any>(`${this.apiUrl}/Vendor`, payload).pipe(
      map(res => this.normalizeVendor(res?.value ?? res))
    );
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
