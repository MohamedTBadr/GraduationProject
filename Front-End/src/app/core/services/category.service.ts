import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Category, CreateCategoryRequest } from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** GET /Category – returns all categories */
  getAll(): Observable<Category[]> {
    return this.http.get<any>(`${this.apiUrl}/Category`).pipe(
      map(res => {
        const data = res.value || res.Value || res;
        const arr = Array.isArray(data) ? data : (data.items || data.Items || []);
        return arr.map((item: any) => ({
           ...item,
           id: item.id || item.Id,
           name: item.name || item.Name
        }));
      })
    );
  }

  /** GET /Category/{categoryId} */
  getById(categoryId: string): Observable<Category> {
    return this.http.get<any>(`${this.apiUrl}/Category/${categoryId}`).pipe(
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

  /** POST /Category */
  create(payload: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(`${this.apiUrl}/Category`, payload, { responseType: 'text' as 'json' });
  }

  /** DELETE /Category/{categoryId} */
  delete(categoryId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Category/${categoryId}`);
  }

  /** PATCH /Category/{categoryId} */
  update(categoryId: string, payload: CreateCategoryRequest): Observable<Category> {
    return this.http.patch<Category>(`${this.apiUrl}/Category/${categoryId}`, payload, { responseType: 'text' as 'json' });
  }

  
}
