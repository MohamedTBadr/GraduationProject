import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, catchError, throwError, map } from 'rxjs';
import {
  UserRole,
  UserSession,
  LoginCredentials,
  AuthResponse
} from '../../shared/types/auth.interface';
import {
  LoginRequest,
  RegisterRequest,
  AuthApiResponse,
  ResetPasswordRequest
} from '../../shared/types/api.interfaces';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl;
  private currentUser = signal<UserSession | null>(null);

  // Readonly signal for components
  user = this.currentUser.asReadonly();

  // Computed values (Angular 18 best practice)
  isLoggedIn = computed(() => !!this.currentUser());
  role = computed(() => this.currentUser()?.role ?? null);

  constructor(private http: HttpClient) {
    const savedSession = localStorage.getItem('eventora_session');
    if (savedSession) {
      this.currentUser.set(JSON.parse(savedSession));
    }
  }

  hasRole(role: UserRole) {
    return this.currentUser()?.role === role;
  }

  /** POST /Authentication/Login */
  //mt
  login(credentials: LoginCredentials, headers?: HttpHeaders): Observable<AuthResponse> {
    const body: LoginRequest = {
      email: credentials.email,
      password: credentials.password || ''
    };

    return this.http
      .post<AuthApiResponse>(`${this.apiUrl}/Authentication/Login`, body, { headers })
      .pipe(
        tap((res) => {
          // Persist the raw RefreshToken before mapping
          localStorage.setItem('eventora_refresh_token', res.value.refreshToken);
          localStorage.setItem('eventora_token', res.value.accessToken);
        }),
        map((res) => this.mapToAuthResponse(res)),
        tap((response) => {
          this.setSession(response);
        }),
        catchError((error) => {
          console.error('Login error', error);
          return throwError(
            () => new Error(error?.error?.message || 'Login failed. Please check your credentials.')
          );
        })
      );
  }

  /** POST /Authentication/Register */
  register(data: any): Observable<AuthResponse> {
    const body: RegisterRequest = {
      name: `${data.firstName ?? ''} ${data.lastName ?? ''}`.trim() || data.name || '',
      email: data.email,
      password: data.password
    };

    // The [Idempotent] attribute on the backend Register endpoint requires a unique key
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });

    return this.http
      .post<AuthApiResponse>(`${this.apiUrl}/Authentication/Register`, body, { headers })
      .pipe(
        tap((res) => {
          // Persist the raw RefreshToken before mapping
          localStorage.setItem('eventora_refresh_token', res.value.refreshToken);
          localStorage.setItem('eventora_token', res.value.accessToken);
        }),
        map((res) => this.mapToAuthResponse(res)),
        tap((response) => {
          this.setSession(response);
        }),
        catchError((error) => {
          console.error('Registration error', error);
          const msg = error?.error?.message
            || (Array.isArray(error?.error) ? error.error.join(', ') : null)
            || 'Registration failed.';
          return throwError(() => new Error(msg));
        })
      );
  }

  /** POST /Authentication/RefreshToken */
  refreshToken(): Observable<AuthApiResponse> {
    const refreshToken = localStorage.getItem('eventora_refresh_token');
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    // C# record field is 'RefreshToken' (capital R) – must match exactly
    const body = { RefreshToken: refreshToken };
    return this.http
      .post<AuthApiResponse>(`${this.apiUrl}/Authentication/RefreshToken`, body)
      .pipe(
        tap((res) => {
          localStorage.setItem('eventora_token', res.value.accessToken);
          localStorage.setItem('eventora_refresh_token', res.value.refreshToken);
        })
      );
  }

  /** POST /Authentication/CheckIfEmailExists?email=... – email is [FromQuery] */
  checkEmailExists(email: string): Observable<boolean> {
    return this.http.post<boolean>(
      `${this.apiUrl}/Authentication/CheckIfEmailExists?email=${encodeURIComponent(email)}`,
      {}
    );
  }

  /** POST /Authentication/ForgetPassword */
  forgetPassword(email: string): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/Authentication/ForgetPassword?email=${encodeURIComponent(email)}`,
      {}
    );
  }

  /** POST /Authentication/ResetPassword */
  resetPassword(payload: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/Authentication/ResetPassword`, payload);
  }

  logout() {
    this.currentUser.set(null);
    localStorage.removeItem('eventora_session');
    localStorage.removeItem('eventora_token');
    localStorage.removeItem('eventora_refresh_token');
    window.location.reload();
  }

  getToken(): string | null {
    return localStorage.getItem('eventora_token');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('eventora_refresh_token');
  }

  // ─────────────────────────────────────────────
  // Private helpers
  // ─────────────────────────────────────────────

  private mapToAuthResponse(res: AuthApiResponse): AuthResponse {
    // Derive role from JWT claims or default to 'user'
    const role = this.extractRoleFromToken(res.value.accessToken);
    const user: UserSession = {
      id: '',          // will be overridden if returned by server
      name: res.value.name,
      email: res.value.email,
      role: role as UserRole
    };
    return { value: { user, token: res.value.accessToken, refreshToken: res.value.refreshToken, role: (res.value.role) as UserRole } };
  }

  private setSession(response: AuthResponse) {
    const refreshToken = localStorage.getItem('eventora_refresh_token'); // preserve if already set
    this.currentUser.set(response.value.user);
    localStorage.setItem('eventora_session', JSON.stringify(response.value.user));
    localStorage.setItem('eventora_token', response.value.token);
    // The interceptor will capture the refresh token from the raw API response separately
  }

  private extractRoleFromToken(token: string): string {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // Common JWT claim names for roles
      return (
        payload['role'] ||
        payload['Role'] ||
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        'user'
      );
    } catch {
      return 'user';
    }
  }
}