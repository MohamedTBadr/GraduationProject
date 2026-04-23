import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../../shared/types/auth.interface';
import { ModalService } from '../../shared/services/modal.service';

export const roleGuard = (expectedRole: UserRole): CanActivateFn => {
    return () => {
        const authService = inject(AuthService);
        const router = inject(Router);
        const modalService = inject(ModalService);

        if (authService.hasRole(expectedRole)) {
            return true;
        }

        // If not logged in at all, open login modal from home
        if (!authService.isLoggedIn()) {
            router.navigate(['/']).then(() => {
                modalService.open('login');
            });
        } else {
            // Logged in but wrong role — just go home
            router.navigate(['/']);
        }
        return false;
    };
};
