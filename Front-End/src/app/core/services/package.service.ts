import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
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
        const items = res?.value?.items ?? res?.value?.Items ?? res?.Value?.items ?? res?.Value?.Items ?? [];
        return Array.isArray(items) ? (items as ApiPackage[]) : [];
      })
    );
  }

  getAll(): Observable<ApiPackage[]> {
    return this.http.get<any>(`${this.apiUrl}/Package`).pipe(
      map(res => {
        const items = res?.value?.items ?? res?.value?.Items ?? res?.Value?.items ?? res?.Value?.Items ?? [];
        return Array.isArray(items) ? (items as ApiPackage[]) : [];
      })
    );
  }

  create(dto: { name: string; description: string; price: number; discount: number; serviceIds: string[] }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Package`, {
      Name: dto.name,
      Description: dto.description,
      Price: dto.price,
      Discount: dto.discount,
      ServiceIds: dto.serviceIds
    });
  }

  update(id: string, dto: { name: string; description: string; price: number; discount: number; serviceIds: string[]; vendorId: string }): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/Package/${id}`, {
      Id: id,
      Name: dto.name,
      Description: dto.description,
      Price: dto.price,
      Discount: dto.discount,
      ServiceIds: dto.serviceIds,
      VendorId: dto.vendorId
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Package/${id}`);
  }
}
