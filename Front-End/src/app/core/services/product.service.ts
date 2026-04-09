import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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

  /** GET /Service – returns all products */
  getAll(): Observable<ApiProduct[]> {
    return this.http.get<any>(`${this.apiUrl}/Service`).pipe(
      map(res => res.value?.items || res.items || res.value || res)
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
      map(res => res.value?.items || res.items || res.value || res)
    );
  }

  /** GET /Service/by-vendor/{vendorId} */
  getByVendor(vendorId: string): Observable<ApiProduct[]> {
    return this.http.get<any>(`${this.apiUrl}/Service/by-vendor/${vendorId}`).pipe(
      map(res => res.value?.items || res.items || res.value || res)
    );
  }

  /** GET /Service/by-service-type/{serviceTypeId} */
  getByServiceType(serviceTypeId: string): Observable<ApiProduct[]> {
    return this.http.get<any>(`${this.apiUrl}/Service/by-service-type/${serviceTypeId}`).pipe(
      map(res => res.value?.items || res.items || res.value || res)
    );
  }

  /** POST /Service */
  create(payload: CreateProductRequest): Observable<ApiProduct> {
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
