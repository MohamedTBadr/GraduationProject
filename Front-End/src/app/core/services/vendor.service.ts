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
        const data = res?.value ?? res?.Value ?? res;
        const items = Array.isArray(data) ? data : (data?.items ?? data?.Items ?? []);
        // #region agent log
        fetch('http://127.0.0.1:7491/ingest/eb6f68d1-7ed9-481a-83a5-e12a4599d43f',{method:'POST',headers:{'Content-Type':'application/json','X-Debug-Session-Id':'af8321'},body:JSON.stringify({sessionId:'af8321',location:'vendor.service.ts:getAll',message:'Vendor API parse',data:{resKeys:res?Object.keys(res):[],dataKeys:data&&typeof data==='object'&&!Array.isArray(data)?Object.keys(data):[],isResArray:Array.isArray(res),isDataArray:Array.isArray(data),itemsCount:Array.isArray(items)?items.length:-1,firstItemKeys:Array.isArray(items)&&items[0]?Object.keys(items[0]):[]},timestamp:Date.now(),hypothesisId:'A'})}).catch(()=>{});
        // #endregion
        if (!Array.isArray(items)) return [];
        return items.map((v: any) => this.normalizeVendor(v));
      })
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

    // #region agent log
    fetch('http://127.0.0.1:7491/ingest/eb6f68d1-7ed9-481a-83a5-e12a4599d43f',{method:'POST',headers:{'Content-Type':'application/json','X-Debug-Session-Id':'af8321'},body:JSON.stringify({sessionId:'af8321',location:'vendor.service.ts:normalizeVendor',message:'Vendor normalized',data:{rawKeys:Object.keys(v),id:normalized.id,name:normalized.name,vendorTypeName:normalized.vendorTypeName,fallbackId},timestamp:Date.now(),hypothesisId:'H',runId:'post-fix-3'})}).catch(()=>{});
    // #endregion

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
