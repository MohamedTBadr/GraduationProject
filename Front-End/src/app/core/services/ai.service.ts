import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  BudgetAllocationResponse,
  EventTimelineResponse,
  RecommendationResponse,
  ApiResult
} from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly apiUrl = `${environment.apiUrl}/AI`;

  constructor(private http: HttpClient) {}

  /**
   * POST /api/AI/budget-allocation
   * Returns a budget breakdown by categories based on total budget and event type.
   */
  getBudgetAllocation(totalBudget: number, eventTypeName: string): Observable<BudgetAllocationResponse> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    return this.http.post<ApiResult<BudgetAllocationResponse>>(`${this.apiUrl}/budget-allocation`, {
      totalBudget,
      eventTypeName
    }, { headers }).pipe(
      map(res => res?.value ?? res)
    );
  }

  /**
   * POST /api/AI/event-timeline/{eventId}
   * Generates a minute-by-minute timeline for the event.
   */
  getEventTimeline(eventId: string): Observable<EventTimelineResponse> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    return this.http.post<ApiResult<EventTimelineResponse>>(`${this.apiUrl}/event-timeline/${eventId}`, {}, { headers }).pipe(
      map(res => res?.value ?? res)
    );
  }

  /**
   * GET /api/AI/clients-like-you/{eventId}
   * Gets recommended services from past events matching the user's event profile.
   */
  getClientsLikeYouRecommendations(eventId: string): Observable<RecommendationResponse> {
    return this.http.get<ApiResult<RecommendationResponse>>(`${this.apiUrl}/clients-like-you/${eventId}`).pipe(
      map(res => res?.value ?? res)
    );
  }
}
