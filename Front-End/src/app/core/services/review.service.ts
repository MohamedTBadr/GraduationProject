import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateReviewDto } from '../../shared/types/api.interfaces';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** POST /Service/{serviceId}/ratings — backend sets userId from token */
  submitReview(payload: CreateReviewDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/Service/${payload.serviceId}/ratings`, {
      rating: payload.rating,
      review: payload.review
    });
  }
}
