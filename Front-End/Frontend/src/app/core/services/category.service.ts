import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Category, CreateCategoryRequest } from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** GET /Category – returns all categories */
  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.apiUrl}/Category`);
  }

  /** GET /Category/{categoryId} */
  getById(categoryId: string): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/Category/${categoryId}`);
  }

  /** POST /Category */
  create(payload: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(`${this.apiUrl}/Category`, payload);
  }

  /** DELETE /Category/{categoryId} */
  delete(categoryId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Category/${categoryId}`);
  }
}
