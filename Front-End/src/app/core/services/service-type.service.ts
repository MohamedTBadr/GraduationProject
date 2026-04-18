import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ServiceType,
  CreateServiceTypeRequest,
  UpdateServiceTypeRequest
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class ServiceTypeService {
  private readonly apiUrl = environment.apiUrl;
  private cachedServiceTypes: ServiceType[] | null = null;

  constructor(private http: HttpClient) {}

  /** GET /ServiceType */
  getAll(): Observable<ServiceType[]> {
    if (this.cachedServiceTypes) {
      return of(this.cachedServiceTypes);
    }
    
    return this.http.get<any>(`${this.apiUrl}/ServiceType`).pipe(
      map(res => {
        const data = res.value || res.Value || res;
        return Array.isArray(data) ? data : (data.items || data.Items || []);
      }),
      tap(data => {
        // Automatically map Id to id and Name to name if needed, but Angular templates usually expect lowercase.
        // The API might be returning PascalCase (Id, Name). Let's let the component handle it or map it here:
        const normalizedData = data.map((item: any) => ({
           ...item,
           id: item.id || item.Id,
           name: item.name || item.Name
        }));
        this.cachedServiceTypes = normalizedData;
      }),
      map(data => this.cachedServiceTypes as ServiceType[])
    );
  }

  /** GET /ServiceType/{serviceTypeId} */
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

  /** POST /ServiceType */
  create(payload: CreateServiceTypeRequest): Observable<ServiceType> {
    return this.http.post<ServiceType>(`${this.apiUrl}/ServiceType`, payload, { responseType: 'text' as 'json' }).pipe(
      tap(() => this.cachedServiceTypes = null)
    );
  }

  /** PATCH /ServiceType/{serviceTypeId} */
  update(serviceTypeId: string, payload: UpdateServiceTypeRequest): Observable<ServiceType> {
    return this.http.patch<ServiceType>(`${this.apiUrl}/ServiceType/${serviceTypeId}`, payload, { responseType: 'text' as 'json' }).pipe(
      tap(() => this.cachedServiceTypes = null)
    );
  }

  /** DELETE /ServiceType/{serviceTypeId} */
  delete(serviceTypeId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/ServiceType/${serviceTypeId}`).pipe(
      tap(() => this.cachedServiceTypes = null)
    );
  }
}
