import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ServiceType,
  CreateServiceTypeRequest,
  UpdateServiceTypeRequest
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class ServiceTypeService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** GET /ServiceType */
  getAll(): Observable<ServiceType[]> {
    return this.http.get<ServiceType[]>(`${this.apiUrl}/ServiceType`);
  }

  /** GET /ServiceType/{serviceTypeId} */
  getById(serviceTypeId: string): Observable<ServiceType> {
    return this.http.get<ServiceType>(`${this.apiUrl}/ServiceType/${serviceTypeId}`);
  }

  /** POST /ServiceType */
  create(payload: CreateServiceTypeRequest): Observable<ServiceType> {
    return this.http.post<ServiceType>(`${this.apiUrl}/ServiceType`, payload);
  }

  /** PATCH /ServiceType/{serviceTypeId} */
  update(serviceTypeId: string, payload: UpdateServiceTypeRequest): Observable<ServiceType> {
    return this.http.patch<ServiceType>(`${this.apiUrl}/ServiceType/${serviceTypeId}`, payload);
  }

  /** DELETE /ServiceType/{serviceTypeId} */
  delete(serviceTypeId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/ServiceType/${serviceTypeId}`);
  }
}
