import { ErrorHandler, Injectable, Injector } from '@angular/core';
import { ToastService } from '../../shared/components/toast/toast.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
    constructor(private injector: Injector) { }

    handleError(error: any): void {
        const toastService = this.injector.get(ToastService);

        const message = error.message ? error.message : error.toString();

        // Log to console for development
        console.error('Global Error:', error);

        // Show toast notification
        toastService.show(message, 'error');
    }
}
