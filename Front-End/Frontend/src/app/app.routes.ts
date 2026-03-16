import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./layouts/public-layout/public-layout.component').then(m => m.PublicLayoutComponent),
        loadChildren: () => import('./features/public/public.routes').then(m => m.PUBLIC_ROUTES)
    },
    {
        path: 'add-event',
        canActivate: [authGuard],
        loadComponent: () => import('./features/user/add-event/add-event.component').then(m => m.AddEventComponent)
    },


    // Admin Portal (Protected)
    {
        path: 'admin',
        canActivate: [authGuard, roleGuard('admin')],
        loadComponent: () => import('./layouts/admin-layout/admin-layout.component').then(m => m.AdminLayoutComponent),
        loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES)
    },

    // Vendor Portal (Protected)
    {
        path: 'vendor-dashboard',
        canActivate: [authGuard, roleGuard('vendor')],
        loadComponent: () => import('./layouts/vendor-layout/vendor-layout.component').then(m => m.VendorLayoutComponent),
        children: [
            {
                path: '',
                loadComponent: () => import('./features/vendor/vendor-dashboard/vendor-dashboard.component').then(m => m.VendorDashboardComponent)
            },
            {
                path: 'services',
                loadComponent: () => import('./features/vendor/services/services.component').then(m => m.ServicesComponent)
            },
            {
                path: 'packages',
                loadComponent: () => import('./features/vendor/packages/packages.component').then(m => m.PackagesComponent)
            },
            {
                path: 'messages',
                loadComponent: () => import('./features/vendor/messages/messages.component').then(m => m.MessagesComponent)
            },
            {
                path: 'bookings',
                loadComponent: () => import('./features/vendor/bookings/bookings.component').then(m => m.BookingsComponent)
            },
            {
                path: 'notifications',
                loadComponent: () => import('./features/vendor/notifications/notifications.component').then(m => m.NotificationsComponent)
            },
            {
                path: 'portfolio',
                loadComponent: () => import('./features/vendor/portfolio/portfolio.component').then(m => m.PortfolioComponent)
            },
            {
                path: 'analytics',
                loadComponent: () => import('./features/vendor/analytics/analytics.component').then(m => m.AnalyticsComponent)
            },
            {
                path: 'earnings',
                loadComponent: () => import('./features/vendor/earnings/earnings.component').then(m => m.EarningsComponent)
            },
            {
                path: 'reviews',
                loadComponent: () => import('./features/vendor/reviews/reviews.component').then(m => m.ReviewsComponent)
            },
            {
                path: 'profile',
                loadComponent: () => import('./features/vendor/profile/profile.component').then(m => m.ProfileComponent)
            }
        ]
    },

    // User Dashboard (Protected)
    {
        path: 'user',
        canActivate: [authGuard, roleGuard('user')],
        loadComponent: () => import('./layouts/user-layout/user-layout.component').then(m => m.UserLayoutComponent),
        children: [
            {
                path: 'dashboard',
                loadComponent: () => import('./features/user/dashboard/dashboard.component').then(m => m.DashboardComponent)
            },
            {
                path: 'my-events',
                loadComponent: () => import('./features/user/my-events/my-events.component').then(m => m.MyEventsComponent)
            },
            {
                path: 'bookings',
                loadComponent: () => import('./features/user/my-bookings/my-bookings.component').then(m => m.MyBookingsComponent)
            },
            {
                path: 'messages',
                loadComponent: () => import('./features/user/messages/messages.component').then(m => m.MessagesComponent)
            },
            {
                path: 'favorites',
                loadComponent: () => import('./features/user/favorites.component').then(m => m.FavoritesComponent)
            },
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
        ]
    },

    { path: '**', redirectTo: '' }
];
