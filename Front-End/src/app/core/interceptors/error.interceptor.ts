import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../../shared/components/toast/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const toastService = inject(ToastService);

    return next(req).pipe(
        catchError((error) => {
            let errorMessage = 'An unexpected error occurred';

            if (error.error instanceof ErrorEvent) {
                // Client-side error
                errorMessage = error.error.message;
            } else {
                // Server-side error
                errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
            }

            toastService.show(errorMessage, 'error');
            return throwError(() => error);
        })
    );
};
