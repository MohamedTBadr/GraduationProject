import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ApiUser,
  CreateUserRequest,
  UpdateUserRequest,
  PaginatedRequest,
  PagedResult
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  /** GET /User */
  getAll(pagination?: PaginatedRequest): Observable<PagedResult<ApiUser>> {
    let params = new HttpParams();
    if (pagination) {
      if (pagination.pageIndex) params = params.set('pageIndex', pagination.pageIndex.toString());
      if (pagination.pageSize) params = params.set('pageSize', pagination.pageSize.toString());
      if (pagination.searchTerm) params = params.set('searchTerm', pagination.searchTerm);
      if (pagination.sortBy) params = params.set('sortBy', pagination.sortBy);
      if (pagination.isDescending !== undefined) params = params.set('isDescending', pagination.isDescending.toString());
      if (pagination.city) params = params.set('city', pagination.city);
      if (pagination.region) params = params.set('region', pagination.region);
      if (pagination.latitude) params = params.set('latitude', pagination.latitude.toString());
      if (pagination.longitude) params = params.set('longitude', pagination.longitude.toString());
      if (pagination.radiusKm) params = params.set('radiusKm', pagination.radiusKm.toString());
    }
    return this.http.get<any>(`${this.apiUrl}/User`, { params }).pipe(
      map(res => {
        const data = res.value ?? res;
        const totalCount = data.totalCount ?? 0;
        const pageSize = data.pageSize ?? 10;
        const items = data.items ?? [];

        return {
          items,
          totalCount,
          pageNumber: data.pageNumber ?? 1,
          pageSize,
          totalPages: data.totalPages ?? (Math.ceil(totalCount / pageSize) || 1)
        } as PagedResult<ApiUser>;
      })
    );
  }

  /** GET /User/{userId} */
  getById(userId: string): Observable<ApiUser> {
    return this.http.get<ApiUser>(`${this.apiUrl}/User/${userId}`);
  }

  /** POST /User – admin: create user */
  create(payload: CreateUserRequest): Observable<ApiUser> {
    return this.http.post<ApiUser>(`${this.apiUrl}/User`, payload);
  }

  /** PATCH /User/{userId} */
  update(userId: string, payload: UpdateUserRequest): Observable<ApiUser> {
    return this.http.patch<ApiUser>(`${this.apiUrl}/User/${userId}`, payload);
  }

  /** PATCH /User/suspend/{userId} */
  suspend(userId: string, reason: string = 'Admin suspension'): Observable<void> {
    const headers = { 'Content-Type': 'application/json' };
    return this.http.patch<void>(`${this.apiUrl}/User/suspend/${userId}`, JSON.stringify(reason), { headers });
  }

  /** PATCH /User/unsuspend/{userId} */
  unsuspend(userId: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/User/unsuspend/${userId}`, {});
  }

  /** DELETE /User/{userId} */
  delete(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/User/${userId}`);
  }
}
