import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
    const authService = inject(AuthService);
    const token = authService.getToken();

    // Attach Bearer token to every outgoing request
    const authedReq = token
        ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
        : req;

    return next(authedReq).pipe(
        catchError((error: HttpErrorResponse) => {
            // Attempt token refresh on 401 Unauthorized
            if (
                error.status === 401 && 
                !req.url.includes('RefreshToken') &&
                !req.url.includes('Login') &&
                !req.url.includes('Register')
            ) {
                return authService.refreshToken().pipe(
                    switchMap((res) => {
                        const retryReq = req.clone({
                            setHeaders: { Authorization: `Bearer ${res.value.accessToken}` }
                        });
                        return next(retryReq);
                    }),
                    catchError((refreshError) => {
                        // Refresh also failed – log the user out
                        authService.logout();
                        return throwError(() => refreshError);
                    })
                );
            }
            return throwError(() => error);
        })
    );
};
