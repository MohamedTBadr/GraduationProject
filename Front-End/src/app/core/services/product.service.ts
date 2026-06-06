import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ApiProduct,
  CreateProductRequest,
  UpdateProductRequest,
  PaginatedRequest,
  PagedResult
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

  private mapServiceStatus(raw: any): 'active' | 'paused' {
    if (this.isHiddenFlag(raw?.isHidden ?? raw?.IsHidden)) return 'paused';

    const status = this.pickField(raw, 'status', 'Status');
    if (status === 'paused' || status === 'Paused') return 'paused';
    return 'active';
  }

  private isHiddenFlag(value: unknown): boolean {
    if (value === true || value === 1) return true;
    if (typeof value === 'string') {
      const normalized = value.trim().toLowerCase();
      return normalized === 'true' || normalized === '1';
    }
    return false;
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
    const ratings = raw.serviceRatings ?? raw.ServiceRatings ?? [];
    const explicitRating = this.pickField(raw, 'rating', 'Rating');
    let rating: number | undefined;
    if (explicitRating != null && explicitRating !== '') {
      const n = Number(explicitRating);
      rating = Number.isFinite(n) ? n : undefined;
    } else if (Array.isArray(ratings) && ratings.length > 0) {
      const sum = ratings.reduce(
        (acc: number, r: any) => acc + Number(r?.rating ?? r?.Rating ?? 0),
        0
      );
      rating = sum / ratings.length;
    }
    const isHidden = this.isHiddenFlag(raw?.isHidden ?? raw?.IsHidden);

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
      status: isHidden ? 'paused' : this.mapServiceStatus(raw),
      duration: this.pickField(raw, 'duration', 'Duration')
        ?? (raw.setupDuration != null ? String(raw.setupDuration) : raw.SetupDuration != null ? String(raw.SetupDuration) : undefined),
      leadTime: this.pickField(raw, 'leadTime', 'LeadTime')
        ?? (raw.leadTimeRequired != null ? String(raw.leadTimeRequired) : raw.LeadTimeRequired != null ? String(raw.LeadTimeRequired) : undefined),
      classification: this.pickField(raw, 'classification', 'Classification'),
      allowedEventTypes: raw.allowedEventTypes ?? raw.AllowedEventTypes,
      createdAt: this.pickField(raw, 'createdAt', 'CreatedAt'),
      serviceAreas: (Array.isArray(areas) ? areas : []).map((sa: any) => ({
        id: sa.id ?? sa.Id,
        city: String(sa.city ?? sa.City ?? ''),
        region: String(sa.region ?? sa.Region ?? ''),
        latitude: Number(sa.latitude ?? sa.Latitude ?? 0),
        longitude: Number(sa.longitude ?? sa.Longitude ?? 0)
      })),
      rating,
      reviewCount: Array.isArray(ratings) ? ratings.length : undefined,
      isHidden
    };
  }

  private mapProductList(res: any): ApiProduct[] {
    return this.extractArrayData(res).map(item => this.normalizeProduct(item));
  }

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
    if (filters.classification && filters.classification !== 'all') params = params.set('classification', filters.classification);
    if (filters.vendorTypeId) params = params.set('vendorTypeId', filters.vendorTypeId);
    if (filters.serviceTypeId) params = params.set('serviceTypeId', filters.serviceTypeId);
    if (filters.minPrice != null) params = params.set('minPrice', filters.minPrice.toString());
    if (filters.maxPrice != null) params = params.set('maxPrice', filters.maxPrice.toString());
    if (filters.includeHidden === true) params = params.set('includeHidden', 'true');

    return params;
  }

  private mapPagedProducts(res: any): PagedResult<ApiProduct> {
    const data = res?.value ?? res?.Value ?? res;
    const items = Array.isArray(data)
      ? data
      : (data?.items ?? data?.Items ?? []);
    const totalCount = data?.totalCount ?? data?.TotalCount ?? (Array.isArray(items) ? items.length : 0);
    const pageSize = data?.pageSize ?? data?.PageSize ?? 10;
    const pageNumber = data?.pageNumber ?? data?.PageNumber ?? 1;

    return {
      items: (Array.isArray(items) ? items : []).map(item => this.normalizeProduct(item)),
      totalCount,
      pageNumber,
      pageSize,
      totalPages: data?.totalPages ?? data?.TotalPages ?? (Math.ceil(totalCount / pageSize) || 1)
    };
  }

  /** GET /Service – returns filtered/paginated products */
  getAll(filters?: PaginatedRequest): Observable<ApiProduct[]> {
    return this.getAllPaged(filters).pipe(map(r => r.items));
  }

  /** GET /Service – paginated with total count */
  getAllPaged(filters?: PaginatedRequest): Observable<PagedResult<ApiProduct>> {
    return this.http.get<any>(`${this.apiUrl}/Service`, { params: this.buildParams(filters) }).pipe(
      map(res => this.mapPagedProducts(res))
    );
  }

  /** GET /Service/by-event-type/{eventTypeId} – paginated */
  getByEventTypePaged(eventTypeId: string, filters?: PaginatedRequest): Observable<PagedResult<ApiProduct>> {
    return this.http.get<any>(`${this.apiUrl}/Service/by-event-type/${eventTypeId}`, {
      params: this.buildParams(filters)
    }).pipe(map(res => this.mapPagedProducts(res)));
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
  getByVendor(vendorId: string, filters?: PaginatedRequest): Observable<ApiProduct[]> {
    return this.http.get<any>(`${this.apiUrl}/Service/by-vendor/${vendorId}`, {
      params: this.buildParams({
        pageIndex: 1,
        pageSize: 200,
        ...filters
      })
    }).pipe(
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
