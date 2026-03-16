import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../../shared/types/auth.interface';

export const roleGuard = (expectedRole: UserRole): CanActivateFn => {
    return () => {
        const authService = inject(AuthService);
        const router = inject(Router);

        if (authService.hasRole(expectedRole)) {
            return true;
        }

        router.navigate(['/']);
        return false;
    };
};
