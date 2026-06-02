import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService, SKIP_AUTH } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
    const authService = inject(AuthService);
    const token = authService.getToken();

    // Attach Bearer token and/or IdempotencyKey
    const headersConfig: any = {};
    if (token && !req.context.get(SKIP_AUTH)) {
        headersConfig['Authorization'] = `Bearer ${token}`;
    }

    // Auto-inject IdempotencyKey for mutating requests if not explicitly provided
    if (['POST', 'PUT', 'PATCH', 'DELETE'].includes(req.method) && !req.headers.has('IdempotencyKey')) {
        let idempotencyKey = '';
        if (typeof crypto !== 'undefined' && crypto.randomUUID) {
            idempotencyKey = crypto.randomUUID();
        } else {
            idempotencyKey = 'idkey_' + new Date().getTime() + '_' + Math.random().toString(36).substring(2);
        }
        headersConfig['IdempotencyKey'] = idempotencyKey;
    }

    const authedReq = Object.keys(headersConfig).length > 0
        ? req.clone({ setHeaders: headersConfig })
        : req;

    return next(authedReq).pipe(
        catchError((error: HttpErrorResponse) => {
            // Attempt token refresh on 401 Unauthorized if not on skip-auth routes
            if (error.status === 401 && !req.context.get(SKIP_AUTH)) {
                return authService.refreshTokenOnce().pipe(
                    switchMap((accessToken) => {
                        const retryReq = req.clone({
                            setHeaders: { Authorization: `Bearer ${accessToken}` }
                        });
                        return next(retryReq);
                    }),
                    catchError((refreshError) => {
                        // Refresh also failed – log the user out
                        authService.logout();
                        // Tag the error as handled so errorInterceptor skips it
                        const handledError = refreshError;
                        (handledError as any).handled = true;
                        return throwError(() => handledError);
                    })
                );
            }
            return throwError(() => error);
        })
    );
};
