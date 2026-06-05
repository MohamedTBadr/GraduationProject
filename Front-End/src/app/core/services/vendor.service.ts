import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ApiVendor,
  CreateVendorRequest,
  UpdateVendorRequest,
  PaginatedRequest,
  PagedResult
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class VendorService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  private buildParams(filters?: PaginatedRequest): HttpParams {
    let params = new HttpParams();
    if (!filters) return params;

    if (filters.pageIndex) params = params.set('pageIndex', filters.pageIndex.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.isDescending !== undefined) params = params.set('isDescending', filters.isDescending.toString());
    if (filters.city) params = params.set('city', filters.city);
    if (filters.region) params = params.set('region', filters.region);
    if (filters.latitude) params = params.set('latitude', filters.latitude.toString());
    if (filters.longitude) params = params.set('longitude', filters.longitude.toString());
    if (filters.radiusKm) params = params.set('radiusKm', filters.radiusKm.toString());
    if (filters.vendorTypeId) params = params.set('vendorTypeId', filters.vendorTypeId);

    return params;
  }

  private mapPagedVendors(res: any): PagedResult<ApiVendor> {
    const data = res?.value ?? res?.Value ?? res;
    const items = Array.isArray(data)
      ? data
      : (data?.items ?? data?.Items ?? []);
    const totalCount = data?.totalCount ?? data?.TotalCount ?? (Array.isArray(items) ? items.length : 0);
    const pageSize = data?.pageSize ?? data?.PageSize ?? 10;
    const pageNumber = data?.pageNumber ?? data?.PageNumber ?? 1;

    return {
      items: (Array.isArray(items) ? items : []).map((v: any) => this.normalizeVendor(v)),
      totalCount,
      pageNumber,
      pageSize,
      totalPages: data?.totalPages ?? data?.TotalPages ?? (Math.ceil(totalCount / pageSize) || 1)
    };
  }

  /** GET /Vendor – returns all vendors (supports pagination & filters) */
  getAll(filters?: PaginatedRequest): Observable<ApiVendor[]> {
    return this.getAllPaged(filters).pipe(map(r => r.items));
  }

  /** GET /Vendor – paginated with total count */
  getAllPaged(filters?: PaginatedRequest): Observable<PagedResult<ApiVendor>> {
    return this.http.get<any>(`${this.apiUrl}/Vendor`, { params: this.buildParams(filters) }).pipe(
      map(res => this.mapPagedVendors(res))
    );
  }

  getById(vendorId: string): Observable<ApiVendor> {
    if (!vendorId) throw new Error('Vendor ID is required');
    return this.http.get<any>(`${this.apiUrl}/Vendor/${vendorId}`).pipe(
      map((res: any) => this.normalizeVendor(res?.value ?? res?.Value ?? res, vendorId))
    );
  }

  /** Reads camelCase or PascalCase API fields (backend responses are inconsistent). */
  private pickField(obj: any, ...keys: string[]): any {
    if (!obj) return undefined;
    for (const key of keys) {
      const val = obj[key];
      if (val !== undefined && val !== null && val !== '') return val;
    }
    return undefined;
  }

  private normalizeVendor(v: any, fallbackId?: string): ApiVendor {
    if (!v || typeof v !== 'object') {
      return { id: fallbackId ?? '', name: 'Unknown Vendor' } as ApiVendor;
    }

    const id = String(
      this.pickField(v, 'userId', 'UserId', 'id', 'Id', 'vendorId', 'VendorId') ?? fallbackId ?? ''
    );
    const name = this.pickField(v, 'businessName', 'BusinessName', 'name', 'Name') ?? 'Unknown Vendor';
    const vendorTypeName =
      this.pickField(v, 'vendorTypeName', 'VendorTypeName', 'vendorType', 'VendorType', 'serviceType', 'ServiceType', 'categoryName', 'CategoryName')
      ?? 'Vendor';
    const areas = v.serviceAreas ?? v.ServiceAreas ?? [];

    const normalized = {
      id,
      name,
      vendorTypeId: String(this.pickField(v, 'vendorTypeId', 'VendorTypeId') ?? v.vendorType?.id ?? v.VendorType?.Id ?? ''),
      vendorTypeName,
      about: this.pickField(v, 'description', 'Description', 'about', 'About') ?? '',
      status: (this.pickField(v, 'status', 'Status') ?? 'active') as ApiVendor['status'],
      isApproved: !!(this.pickField(v, 'isVerified', 'IsVerified', 'isApproved', 'IsApproved') ?? false),
      createdAt: this.pickField(v, 'createdAt', 'CreatedAt') ?? new Date().toISOString(),
      rating: Number(this.pickField(v, 'rating', 'Rating') ?? 0),
      location: this.pickField(v, 'location', 'Location', 'address', 'Address') ?? '',
      documentUrl: this.pickField(v, 'documentUrl', 'DocumentUrl', 'document', 'Document'),
      profilePictureUrl: this.pickField(v, 'profilePictureUrl', 'ProfilePictureUrl', 'profilePicture', 'ProfilePicture'),
      serviceAreas: (Array.isArray(areas) ? areas : []).map((sa: any) => ({
        ...sa,
        id: sa.id ?? sa.Id,
        city: sa.city ?? sa.City ?? '',
        region: sa.region ?? sa.Region ?? '',
        latitude: sa.latitude ?? sa.Latitude ?? sa.lattitude ?? 0,
        longitude: sa.longitude ?? sa.Longitude ?? 0
      }))
    } as ApiVendor;

    return normalized;
  }

  /** POST /Vendor – register a new vendor */
  create(payload: CreateVendorRequest | FormData): Observable<ApiVendor> {
    // If it's not FormData, we should probably convert it if it contains files
    // But usually the component will pass FormData if files are involved
    return this.http.post<any>(`${this.apiUrl}/Vendor`, payload).pipe(
      map(res => this.normalizeVendor(res?.value ?? res?.Value ?? res))
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
