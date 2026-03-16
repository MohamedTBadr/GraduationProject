import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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

  /** GET /Products – returns all products */
  getAll(): Observable<ApiProduct[]> {
    return this.http.get<ApiProduct[]>(`${this.apiUrl}/Products`);
  }

  /** GET /Product/{productId} */
  getById(productId: string): Observable<ApiProduct> {
    return this.http.get<ApiProduct>(`${this.apiUrl}/Product/${productId}`);
  }

  /** GET /Product/by-category/{categoryId} */
  getByCategory(categoryId: string): Observable<ApiProduct[]> {
    return this.http.get<ApiProduct[]>(`${this.apiUrl}/Product/by-category/${categoryId}`);
  }

  /** GET /Product/by-vendor/{vendorId} */
  getByVendor(vendorId: string): Observable<ApiProduct[]> {
    return this.http.get<ApiProduct[]>(`${this.apiUrl}/Product/by-vendor/${vendorId}`);
  }

  /** GET /Product/by-service-type/{serviceTypeId} */
  getByServiceType(serviceTypeId: string): Observable<ApiProduct[]> {
    return this.http.get<ApiProduct[]>(`${this.apiUrl}/Product/by-service-type/${serviceTypeId}`);
  }

  /** POST /Product */
  create(payload: CreateProductRequest): Observable<ApiProduct> {
    return this.http.post<ApiProduct>(`${this.apiUrl}/Product`, payload);
  }

  /** PUT /Product/{productId} */
  update(productId: string, payload: UpdateProductRequest): Observable<ApiProduct> {
    return this.http.put<ApiProduct>(`${this.apiUrl}/Product/${productId}`, payload);
  }

  /** DELETE /Product/{productId} */
  delete(productId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Product/${productId}`);
  }
}
