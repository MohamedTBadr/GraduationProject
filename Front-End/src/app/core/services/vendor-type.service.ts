import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { VendorType } from '../models/taxonomy.models';

@Injectable({ providedIn: 'root' })
export class VendorTypeService {
  private readonly apiUrl = environment.apiUrl;
  private cachedVendorTypes: VendorType[] | null = null;

  constructor(private http: HttpClient) {}

  /** GET /api/VendorType */
  getAll(): Observable<VendorType[]> {
    if (this.cachedVendorTypes) {
      return of(this.cachedVendorTypes);
    }
    
    return this.http.get<any>(`${this.apiUrl}/VendorType`).pipe(
      map(res => {
        if (!res) return [];
        const data = res.value || res.Value || res;
        const arr = Array.isArray(data) ? data : (data.items || data.Items || []);
        return arr.map((item: any) => ({
           id: item?.id || item?.Id,
           name: item?.name || item?.Name
        }));
      }),
      tap(data => this.cachedVendorTypes = data)
    );
  }

  /** GET /api/VendorType/{id} */
  getById(id: string): Observable<VendorType> {
    return this.http.get<any>(`${this.apiUrl}/VendorType/${id}`).pipe(
      map(res => {
        if (!res) return { id: '', name: '' };
        const item = res.value || res.Value || res;
        return {
          id: item?.id || item?.Id,
          name: item?.name || item?.Name
        };
      })
    );
  }

  /** POST /api/VendorType */
  create(payload: { name: string }): Observable<VendorType> {
    return this.http.post<any>(`${this.apiUrl}/VendorType`, payload).pipe(
      map(res => {
        if (!res) return { id: '', name: '' };
        const item = res.value || res.Value || res;
        return {
          id: item?.id || item?.Id,
          name: item?.name || item?.Name
        };
      }),
      tap(() => this.cachedVendorTypes = null)
    );
  }

  /** PUT /api/VendorType/{id} */
  update(id: string, payload: { name: string }): Observable<VendorType> {
    return this.http.put<any>(`${this.apiUrl}/VendorType/${id}`, payload).pipe(
      map(res => {
        if (!res) return { id: '', name: '' };
        const item = res.value || res.Value || res;
        return {
          id: item?.id || item?.Id,
          name: item?.name || item?.Name
        };
      }),
      tap(() => this.cachedVendorTypes = null)
    );
  }

  /** DELETE /api/VendorType/{id} */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/VendorType/${id}`).pipe(
      tap(() => this.cachedVendorTypes = null)
    );
  }
}
