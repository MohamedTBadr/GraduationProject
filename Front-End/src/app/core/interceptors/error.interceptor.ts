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
            } else if (error.status === 401) {
                errorMessage = 'Unauthorized Access. Please log in.';
            } else if (error.status === 403) {
                errorMessage = 'You do not have permission to perform this action.';
            } else if (error.error) {
                // Parse .NET specific errors
                if (typeof error.error === 'string') {
                    errorMessage = error.error;
                } else if (error.error.detail) {
                    errorMessage = error.error.detail;
                } else if (error.error.message) {
                    errorMessage = error.error.message;
                } else if (error.error.errors) {
                    const errorMessages = [];
                    for (const key in error.error.errors) {
                        if (Object.prototype.hasOwnProperty.call(error.error.errors, key)) {
                            errorMessages.push(...error.error.errors[key]);
                        }
                    }
                    if (errorMessages.length > 0) {
                        errorMessage = errorMessages.join(' | ');
                    }
                }
            } else {
                errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
            }

            toastService.show(errorMessage, 'error');
            return throwError(() => error);
        })
    );
};
