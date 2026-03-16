import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
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
    if (pagination?.pageNumber) {
      params = params.set('pageNumber', pagination.pageNumber.toString());
    }
    if (pagination?.pageSize) {
      params = params.set('pageSize', pagination.pageSize.toString());
    }
    return this.http.get<PagedResult<ApiUser>>(`${this.apiUrl}/User`, { params });
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
