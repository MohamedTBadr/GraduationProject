import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { CreateServiceTypeRequest, UpdateServiceTypeRequest } from '../../shared/types/api.interfaces';
import { ServiceType } from '../models/taxonomy.models';

@Injectable({ providedIn: 'root' })
export class ServiceTypeService {
  private readonly apiUrl = environment.apiUrl;
  private cachedServiceTypes: ServiceType[] | null = null;

  constructor(private http: HttpClient) {}

  /** GET /api/ServiceType */
  getAll(): Observable<ServiceType[]> {
    if (this.cachedServiceTypes) {
      return of(this.cachedServiceTypes);
    }
    
    return this.http.get<any>(`${this.apiUrl}/ServiceType`).pipe(
      map(res => {
        const data = res.value || res.Value || res;
        const arr = Array.isArray(data) ? data : (data?.items || data?.Items || []);
        return Array.isArray(arr) ? arr : [];
      }),
      tap(data => {
        const normalizedData = data.map((item: any) => ({
           ...item,
           id: item.id || item.Id,
           name: item.name || item.Name
        }));
        this.cachedServiceTypes = normalizedData;
      }),
      map(() => this.cachedServiceTypes as ServiceType[])
    );
  }

  /** GET /api/ServiceType/{serviceTypeId} */
  getById(serviceTypeId: string): Observable<ServiceType> {
    return this.http.get<any>(`${this.apiUrl}/ServiceType/${serviceTypeId}`).pipe(
      map(res => {
        const item = res.value || res.Value || res;
        return {
          ...item,
          id: item.id || item.Id,
          name: item.name || item.Name
        };
      })
    );
  }

  /** POST /api/ServiceType */
  create(payload: CreateServiceTypeRequest): Observable<ServiceType> {
    return this.http.post<ServiceType>(`${this.apiUrl}/ServiceType`, payload, { responseType: 'text' as 'json' }).pipe(
      tap(() => this.cachedServiceTypes = null)
    );
  }

  /** PATCH /api/ServiceType/{serviceTypeId} */
  update(serviceTypeId: string, payload: UpdateServiceTypeRequest): Observable<ServiceType> {
    return this.http.patch<ServiceType>(`${this.apiUrl}/ServiceType/${serviceTypeId}`, payload, { responseType: 'text' as 'json' }).pipe(
      tap(() => this.cachedServiceTypes = null)
    );
  }

  /** DELETE /api/ServiceType/{serviceTypeId} */
  delete(serviceTypeId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/ServiceType/${serviceTypeId}`).pipe(
      tap(() => this.cachedServiceTypes = null)
    );
  }
}
