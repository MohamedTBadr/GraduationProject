import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ApiProduct,
  CreateProductRequest,
  UpdateProductRequest,
  PaginatedRequest
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Unwraps Result<T>, PaginatedResponse, or plain arrays from GET endpoints.
   */
  private extractArrayData(res: any): any[] {
    if (!res) return [];
    if (Array.isArray(res)) return res;

    const inner = res.value ?? res.Value;
    if (inner != null) {
      const items = inner.items ?? inner.Items;
      if (Array.isArray(items)) return items;
      if (Array.isArray(inner)) return inner;
    }

    const top = res.items ?? res.Items;
    if (Array.isArray(top)) return top;

    return [];
  }

  private pickField(obj: any, ...keys: string[]): any {
    if (!obj) return undefined;
    for (const key of keys) {
      const val = obj[key];
      if (val !== undefined && val !== null && val !== '') return val;
    }
    return undefined;
  }

  /** Maps API ServiceDTO fields to ApiProduct. */
  private normalizeProduct(raw: any): ApiProduct {
    if (raw == null || typeof raw !== 'object') {
      return { id: '', name: 'Unknown Service', description: '', price: 0 };
    }

    const images = raw.serviceImages ?? raw.ServiceImages;
    const firstImage = this.pickField(raw, 'imageUrl', 'ImageUrl')
      ?? (Array.isArray(images) && images.length > 0 ? images[0] : undefined);

    const priceRaw = this.pickField(raw, 'price', 'Price') ?? 0;
    const price = typeof priceRaw === 'number' ? priceRaw : parseFloat(String(priceRaw));
    const areas = raw.serviceAreas ?? raw.ServiceAreas ?? [];

    return {
      id: String(this.pickField(raw, 'id', 'Id') ?? ''),
      name: String(this.pickField(raw, 'name', 'Name') ?? 'Unknown Service'),
      description: String(this.pickField(raw, 'description', 'Description') ?? ''),
      price: Number.isFinite(price) ? price : 0,
      vendorTypeId: this.pickField(raw, 'vendorTypeId', 'VendorTypeId', 'categoryId', 'CategoryId'),
      vendorTypeName: this.pickField(raw, 'vendorTypeName', 'VendorTypeName', 'categoryName', 'CategoryName'),
      vendorId: this.pickField(raw, 'vendorId', 'VendorId'),
      vendorName: this.pickField(raw, 'vendorName', 'VendorName'),
      serviceTypeId: this.pickField(raw, 'serviceTypeId', 'ServiceTypeId'),
      serviceTypeName: this.pickField(raw, 'serviceTypeName', 'ServiceTypeName'),
      imageUrl: firstImage,
      imageUrls: Array.isArray(images) ? images : (firstImage ? [firstImage] : []),
      status: this.pickField(raw, 'status', 'Status') ?? 'active',
      duration: this.pickField(raw, 'duration', 'Duration')
        ?? (raw.setupDuration != null ? String(raw.setupDuration) : raw.SetupDuration != null ? String(raw.SetupDuration) : undefined),
      leadTime: this.pickField(raw, 'leadTime', 'LeadTime')
        ?? (raw.leadTimeRequired != null ? String(raw.leadTimeRequired) : raw.LeadTimeRequired != null ? String(raw.LeadTimeRequired) : undefined),
      classification: this.pickField(raw, 'classification', 'Classification'),
      allowedEventTypes: raw.allowedEventTypes ?? raw.AllowedEventTypes,
      createdAt: this.pickField(raw, 'createdAt', 'CreatedAt'),
      serviceAreas: Array.isArray(areas) ? areas : []
    };
  }

  private mapProductList(res: any): ApiProduct[] {
    return this.extractArrayData(res).map(item => this.normalizeProduct(item));
  }

  /** GET /Service – returns filtered/paginated products */
  getAll(filters?: PaginatedRequest): Observable<ApiProduct[]> {
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
      if (filters.classification && filters.classification !== 'all') params = params.set('classification', filters.classification);
      if (filters.eventTypeId) params = params.set('eventTypeId', filters.eventTypeId);
      if (filters.vendorTypeId) params = params.set('vendorTypeId', filters.vendorTypeId);
      if (filters.serviceTypeId) params = params.set('serviceTypeId', filters.serviceTypeId);
    }

    return this.http.get<any>(`${this.apiUrl}/Service`, { params }).pipe(
      map(res => this.mapProductList(res))
    );
  }

  /** GET /Service/{productId} */
  getById(productId: string): Observable<ApiProduct> {
    return this.http.get<any>(`${this.apiUrl}/Service/${productId}`).pipe(
      map(res => {
        const raw = res?.value ?? res?.Value ?? res;
        return this.normalizeProduct(raw);
      })
    );
  }


  /** GET /Service/by-vendor/{vendorId} */
  getByVendor(vendorId: string): Observable<ApiProduct[]> {
    return this.http.get<any>(`${this.apiUrl}/Service/by-vendor/${vendorId}`).pipe(
      map(res => this.mapProductList(res))
    );
  }

  /** GET /Service/by-service-type/{serviceTypeId} */
  getByServiceType(serviceTypeId: string): Observable<ApiProduct[]> {
    return this.http.get<any>(`${this.apiUrl}/Service/by-service-type/${serviceTypeId}`).pipe(
      map(res => this.mapProductList(res))
    );
  }

  /** POST /Service */
  create(payload: FormData): Observable<ApiProduct> {
    return this.http.post<any>(`${this.apiUrl}/Service`, payload).pipe(
      map(res => this.normalizeProduct(res?.value ?? res?.Value ?? res))
    );
  }

  /** PUT /Service/{productId} */
  update(productId: string, payload: UpdateProductRequest | FormData | any): Observable<ApiProduct> {
    return this.http.put<any>(`${this.apiUrl}/Service/${productId}`, payload).pipe(
      map(res => this.normalizeProduct(res?.value ?? res?.Value ?? res))
    );
  }

  /** PATCH /Service/{productId}/status – toggles active/paused */
  toggleStatus(productId: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/Service/${productId}/status`, {});
  }

  /** DELETE /Service/{productId} */
  delete(productId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Service/${productId}`);
  }
}
