import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ModalService } from '../../shared/services/modal.service';

export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const modalService = inject(ModalService);

    if (authService.isLoggedIn()) {
        return true;
    }

    // Navigate to home then open the login modal
    router.navigate(['/']).then(() => {
        modalService.open('login');
    });
    return false;
};
