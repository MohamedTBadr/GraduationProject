import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateReviewDto } from '../../shared/types/api.interfaces';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private readonly apiUrl = `${environment.apiUrl}/Review`;

  constructor(private http: HttpClient) {}

  submitReview(payload: CreateReviewDto): Observable<any> {
    return this.http.post<any>(this.apiUrl, payload);
  }
}
