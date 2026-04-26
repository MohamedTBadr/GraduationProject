import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ApiProduct,
  CreateProductRequest,
  UpdateProductRequest
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  private extractArrayData(res: any): any[] {
    if (!res) return [];
    if (Array.isArray(res)) return res;
    if (res.value && Array.isArray(res.value.items)) return res.value.items;
    if (res.value && Array.isArray(res.value)) return res.value;
    if (Array.isArray(res.items)) return res.items;
    return [];
  }

  /** GET /Service – returns all products */
  getAll(filters?: { classification?: string; eventTypeId?: string }): Observable<ApiProduct[]> {
    let params = new HttpParams();
    if (filters?.classification && filters.classification !== 'all') {
      params = params.set('classification', filters.classification);
    }
    if (filters?.eventTypeId) {
      params = params.set('eventTypeId', filters.eventTypeId);
    }
    return this.http.get<any>(`${this.apiUrl}/Service`, { params }).pipe(
      map(res => this.extractArrayData(res))
    );
  }

  /** GET /Service/{productId} */
  getById(productId: string): Observable<ApiProduct> {
    return this.http.get<any>(`${this.apiUrl}/Service/${productId}`).pipe(
      map(res => res.value || res)
    );
  }

  /** GET /Service/by-category/{categoryId} */
  getByCategory(categoryId: string): Observable<ApiProduct[]> {
    return this.http.get<any>(`${this.apiUrl}/Service/by-category/${categoryId}`).pipe(
      map(res => this.extractArrayData(res))
    );
  }

  /** GET /Service/by-vendor/{vendorId} */
  getByVendor(vendorId: string): Observable<ApiProduct[]> {
    return this.http.get<any>(`${this.apiUrl}/Service/by-vendor/${vendorId}`).pipe(
      map(res => this.extractArrayData(res))
    );
  }

  /** GET /Service/by-service-type/{serviceTypeId} */
  getByServiceType(serviceTypeId: string): Observable<ApiProduct[]> {
    return this.http.get<any>(`${this.apiUrl}/Service/by-service-type/${serviceTypeId}`).pipe(
      map(res => this.extractArrayData(res))
    );
  }

  /** POST /Service */
  create(payload: FormData): Observable<ApiProduct> {
    return this.http.post<any>(`${this.apiUrl}/Service`, payload).pipe(
      map(res => res.value || res)
    );
  }

  /** PUT /Service/{productId} */
  update(productId: string, payload: UpdateProductRequest): Observable<ApiProduct> {
    return this.http.put<any>(`${this.apiUrl}/Service/${productId}`, payload).pipe(
      map(res => res.value || res)
    );
  }

  /** DELETE /Service/{productId} */
  delete(productId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Service/${productId}`);
  }
}
