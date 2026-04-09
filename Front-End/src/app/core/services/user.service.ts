import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ApiUser,
  CreateUserRequest,
  UpdateUserRequest,
  PaginationParams,
  PagedResult
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** GET /User?pageNumber=1&pageSize=10 */
  getAll(pagination?: PaginationParams): Observable<PagedResult<ApiUser>> {
    let params = new HttpParams();
    if (pagination) {
      if (pagination.pageNumber) {
        params = params.set('PageIndex', pagination.pageNumber.toString()); // Note: backend uses PageIndex
      }
      if (pagination.pageSize) {
        params = params.set('PageSize', pagination.pageSize.toString());
      }
      if (pagination.searchTerm) {
        params = params.set('SearchTerm', pagination.searchTerm);
      }
    }
    return this.http.get<any>(`${this.apiUrl}/User`, { params }).pipe(
      map(res => res.value || res)
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

  /** DELETE /User/{userId} */
  delete(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/User/${userId}`);
  }
}
