import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders, HttpContext, HttpContextToken } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, map, shareReplay, finalize } from 'rxjs';
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
  ResetPasswordRequest,
  ChangePasswordRequest
} from '../../shared/types/api.interfaces';
import { environment } from '../../../environments/environment';

export const SKIP_AUTH = new HttpContextToken<boolean>(() => false);

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

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    try {
      const savedSession = localStorage.getItem('eventora_session');
      if (savedSession) {
        this.currentUser.set(JSON.parse(savedSession));
      }
    } catch {
      localStorage.removeItem('eventora_session');
      localStorage.removeItem('eventora_token');
      localStorage.removeItem('eventora_refresh_token');
    }
  }

  hasRole(role: UserRole) {
    return this.currentUser()?.role === role;
  }

  /** POST /Authentication/Login */
  //mt
  login(credentials: LoginCredentials): Observable<AuthResponse> {
    const body: LoginRequest = {
      email: credentials.email,
      password: credentials.password || ''
    };

    const headers = new HttpHeaders({
      'IdempotencyKey': crypto.randomUUID()
    });

    return this.http
      .post<AuthApiResponse>(`${this.apiUrl}/Authentication/Login`, body, {
        headers,
        context: new HttpContext().set(SKIP_AUTH, true)
      })
      .pipe(
        tap((res) => {
          // Persist the raw RefreshToken before mapping
          localStorage.setItem('eventora_refresh_token', res.refreshToken);
          localStorage.setItem('eventora_token', res.accessToken);
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

  register(data: RegisterRequest): Observable<AuthResponse> {
    const body: RegisterRequest = {
      name: (data.name || `${data.firstName || ''} ${data.lastName || ''}`).trim() || '',
      firstName: data.firstName || '',
      lastName: data.lastName || '',
      phoneNumber: data.phoneNumber || '',
      email: data.email,
      password: data.password
    };

    // The [Idempotent] attribute on the backend Register endpoint requires a unique key
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });

    return this.http
      .post<AuthApiResponse>(`${this.apiUrl}/Authentication/Register`, body, {
        headers,
        context: new HttpContext().set(SKIP_AUTH, true)
      })
      .pipe(
        tap((res) => {
          // Persist the raw RefreshToken before mapping
          localStorage.setItem('eventora_refresh_token', res.refreshToken);
          localStorage.setItem('eventora_token', res.accessToken);
        }),
        map((res) => this.mapToAuthResponse(res)),
        tap((response) => {
          this.setSession(response);
        }),
        catchError((error) => {
          console.error('Registration error', error);
          
          let msg = 'Registration failed.';
          
          if (error.status === 409) {
            if (error?.error?.detail) {
              msg = error.error.detail;
            } else if (error?.error?.message) {
              msg = error.error.message;
            } else {
              msg = 'An account with this email already exists.';
            }
          } else if (typeof error.error === 'string') {
            msg = error.error;
          } else if (error?.error?.detail) {
            msg = error.error.detail;
          } else if (error?.error?.ErrorMessage) {
            msg = error.error.ErrorMessage;
          } else if (error?.error?.message) {
            msg = error.error.message;
          } else if (Array.isArray(error?.error)) {
            msg = error.error.join(', ');
          } else if (error?.error?.errors) {
            // Standard .NET ValidationProblemDetails
            const errorMessages = [];
            for (const key in error.error.errors) {
              if (Object.prototype.hasOwnProperty.call(error.error.errors, key)) {
                errorMessages.push(...error.error.errors[key]);
              }
            }
            if (errorMessages.length > 0) {
              msg = errorMessages.join(' | ');
            }
          }
          
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

    // Match the JSON key casing tested in Apidog
    const body = { refreshToken: refreshToken };
    return this.http
      .post<AuthApiResponse>(`${this.apiUrl}/Authentication/RefreshToken`, body, {
        context: new HttpContext().set(SKIP_AUTH, true)
      })
      .pipe(
        tap((res) => {
          localStorage.setItem('eventora_token', res.accessToken);
          localStorage.setItem('eventora_refresh_token', res.refreshToken);
        })
      );
  }

  private refreshResult$: Observable<string> | null = null;

  refreshTokenOnce(): Observable<string> {
    if (this.refreshResult$) {
      return this.refreshResult$;
    }

    this.refreshResult$ = this.refreshToken().pipe(
      map((res) => res.accessToken),
      shareReplay(1),
      finalize(() => {
        this.refreshResult$ = null;
      })
    );

    return this.refreshResult$;
  }

  /** GET /Authentication/CheckIfEmailExists?email=... – email is [FromQuery] */
  checkEmailExists(email: string): Observable<boolean> {
    return this.http.get<boolean>(
      `${this.apiUrl}/Authentication/CheckIfEmailExists?email=${encodeURIComponent(email)}`
    );
  }

  /** POST /Authentication/ForgetPassword */
  forgetPassword(email: string): Observable<void> {
    const headers = new HttpHeaders({
      'IdempotencyKey': crypto.randomUUID()
    });
    return this.http.post<void>(
      `${this.apiUrl}/Authentication/ForgetPassword?email=${encodeURIComponent(email)}`,
      {},
      { headers }
    ).pipe(
      catchError((error) => {
        let msg = 'Failed to request password reset.';
        if (error.status === 404) {
          msg = 'No user found with this email address.';
        } else if (typeof error.error === 'string') {
          msg = error.error;
        } else if (error?.error?.detail) {
          msg = error.error.detail;
        } else if (error?.error?.message) {
          msg = error.error.message;
        }
        return throwError(() => new Error(msg));
      })
    );
  }

  /** POST /Authentication/ForgetPassword (alias for resend) */
  resendReset(email: string): Observable<void> {
    return this.forgetPassword(email);
  }

  /** POST /Authentication/ResetPassword */
  resetPassword(payload: ResetPasswordRequest): Observable<void> {
    const headers = new HttpHeaders({
      'IdempotencyKey': crypto.randomUUID()
    });
    return this.http.post<void>(`${this.apiUrl}/Authentication/ResetPassword`, payload, { headers }).pipe(
      catchError((error) => {
        let msg = 'Failed to reset password.';
        if (typeof error.error === 'string') {
          msg = error.error;
        } else if (error?.error?.ErrorMessage) {
          msg = error.error.ErrorMessage;
        } else if (error?.error?.message) {
          msg = error.error.message;
        } else if (Array.isArray(error?.error)) {
          msg = error.error.join(', ');
        } else if (error?.error?.errors) {
          const errorMessages = [];
          for (const key in error.error.errors) {
            if (Object.prototype.hasOwnProperty.call(error.error.errors, key)) {
              errorMessages.push(...error.error.errors[key]);
            }
          }
          if (errorMessages.length > 0) {
            msg = errorMessages.join(' | ');
          }
        }
        return throwError(() => new Error(msg));
      })
    );
  }

  /** POST /Authentication/ChangePassword */
  changePassword(payload: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/Authentication/ChangePassword`, payload);
  }

  logout() {
    this.currentUser.set(null);
    localStorage.removeItem('eventora_session');
    localStorage.removeItem('eventora_token');
    localStorage.removeItem('eventora_refresh_token');
    this.router.navigate(['/']);
  }

  getToken(): string | null {
    return localStorage.getItem('eventora_token');
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      if (!payload.exp) return true;
      const exp = payload.exp * 1000;
      // Use a 10 second skew window
      return Date.now() >= (exp - 10000);
    } catch {
      return true;
    }
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('eventora_refresh_token');
  }


  // ─────────────────────────────────────────────
  // Private helpers
  // ─────────────────────────────────────────────

  private mapToAuthResponse(res: AuthApiResponse): AuthResponse {
    // Derive claims from JWT
    const claims = this.extractClaimsFromToken(res.accessToken);
    const tokenRole = claims.role;
    
    // Attempt to extract user id from standard JWT claims (sub, nameidentifier, or id)
    const userId = claims.id || '';

    const user: UserSession = {
      id: userId,
      name: res.name,
      email: res.email,
      role: tokenRole as UserRole
    };
    
    // Use the role from the token if the api response role is empty or missing
    let apiRole = res.role;
    if (!apiRole) {
      apiRole = tokenRole;
    } else {
      apiRole = apiRole.charAt(0).toUpperCase() + apiRole.slice(1).toLowerCase();
    }
    if (apiRole === 'Customer') apiRole = 'User';

    return { value: { user, token: res.accessToken, refreshToken: res.refreshToken, role: apiRole as UserRole } };
  }

  private setSession(response: AuthResponse) {
    const refreshToken = localStorage.getItem('eventora_refresh_token'); // preserve if already set
    this.currentUser.set(response.value.user);
    localStorage.setItem('eventora_session', JSON.stringify(response.value.user));
    localStorage.setItem('eventora_token', response.value.token);
    // The interceptor will capture the refresh token from the raw API response separately
  }

  private extractClaimsFromToken(token: string): {role: string, id: string} {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      
      let rawRole = (
        payload['role'] ||
        payload['Role'] ||
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        'User'
      );
      if (typeof rawRole !== 'string') rawRole = String(rawRole);
      let role = rawRole.charAt(0).toUpperCase() + rawRole.slice(1).toLowerCase();
      if (role === 'Customer') role = 'User';

      let id = (
        payload['sub'] || 
        payload['nameid'] || 
        payload['nameidentifier'] ||
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
        payload['id'] || ''
      );

      return { role, id };
    } catch {
      return { role: 'User', id: '' };
    }
  }
}