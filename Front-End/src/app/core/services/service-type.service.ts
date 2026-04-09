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
      map(res => res.value || res),
      tap(data => this.cachedServiceTypes = data)
    );
  }

  /** GET /ServiceType/{serviceTypeId} */
  getById(serviceTypeId: string): Observable<ServiceType> {
    return this.http.get<ServiceType>(`${this.apiUrl}/ServiceType/${serviceTypeId}`);
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
