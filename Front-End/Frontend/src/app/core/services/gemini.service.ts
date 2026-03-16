import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GeminiResponse } from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class GeminiService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * GET /Gemini/generate-text?prompt=...
   * Generates text using the Gemini AI integrated in the backend.
   */
  generateText(prompt: string): Observable<GeminiResponse> {
    const params = new HttpParams().set('prompt', prompt);
    return this.http.get<GeminiResponse>(`${this.apiUrl}/Gemini/generate-text`, { params });
  }
}
