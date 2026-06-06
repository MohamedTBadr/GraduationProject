import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface ApiPackage {
  id: string;
  name: string;
  description?: string;
  price: number;
  discount: number;
  services: { id: string; name: string; price: number }[];
  vendorId: string;
}

@Injectable({ providedIn: 'root' })
export class PackageService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** GET /Package?vendorId={id}
   * Response: { isSuccess, value: { items: [...], totalCount, pageNumber, pageSize }, error }
   */
  getByVendor(vendorId: string): Observable<ApiPackage[]> {
    const params = new HttpParams().set('vendorId', vendorId);
    return this.http.get<any>(`${this.apiUrl}/Package`, { params }).pipe(
      map(res => {
        const data = res?.value ?? res;
        const items = Array.isArray(data) ? data : (data?.items ?? []);
        return Array.isArray(items) ? (items as ApiPackage[]) : [];
      })
    );
  }

  getAll(): Observable<ApiPackage[]> {
    return this.http.get<any>(`${this.apiUrl}/Package`).pipe(
      map(res => {
        const data = res?.value ?? res;
        const items = Array.isArray(data) ? data : (data?.items ?? []);
        return Array.isArray(items) ? (items as ApiPackage[]) : [];
      })
    );
  }

  create(dto: { name: string; description: string; price: number; discount: number; serviceIds: string[] }): Observable<any> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    return this.http.post<any>(`${this.apiUrl}/Package`, {
      name: dto.name,
      description: dto.description,
      price: dto.price,
      discount: dto.discount,
      serviceIds: dto.serviceIds
    }, { headers });
  }

  update(id: string, dto: { name: string; description: string; price: number; discount: number; serviceIds: string[]; vendorId: string }): Observable<any> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    return this.http.put<any>(`${this.apiUrl}/Package/${id}`, {
      id,
      name: dto.name,
      description: dto.description,
      price: dto.price,
      discount: dto.discount,
      serviceIds: dto.serviceIds,
      vendorId: dto.vendorId
    }, { headers });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Package/${id}`);
  }

  /** PATCH /Package/{id}/status — toggles active/paused (awaiting backend confirmation) */
  toggleStatus(packageId: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/Package/${packageId}/status`, {});
  }
}
