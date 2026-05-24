import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../../shared/components/toast/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const toastService = inject(ToastService);

    return next(req).pipe(
        catchError((error) => {
            if (error && (error as any).handled) {
                return throwError(() => error);
            }
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
                if (error.error.errorDescription || error.error.ErrorDescription) { // New Result<T> error format
                    errorMessage = error.error.errorDescription || error.error.ErrorDescription;
                } else if (typeof error.error === 'string') {
                    errorMessage = error.error;
                } else if (error.error.detail || error.error.Detail) {
                    errorMessage = error.error.detail || error.error.Detail;
                } else if (error.error.message || error.error.Message) {
                    errorMessage = error.error.message || error.error.Message;
                } else if (error.error.errors || error.error.Errors) {
                    const errorsObj = error.error.errors || error.error.Errors;
                    const errorMessages = [];
                    for (const key in errorsObj) {
                        if (Object.prototype.hasOwnProperty.call(errorsObj, key)) {
                            errorMessages.push(...errorsObj[key]);
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
