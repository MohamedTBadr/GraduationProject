import { Routes } from '@angular/router';

export const VENDOR_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./vendor-dashboard/vendor-dashboard.component').then(m => m.VendorDashboardComponent)
    },
    {
        path: 'bookings',
        loadComponent: () => import('./bookings/bookings.component').then(m => m.BookingsComponent)
    },
    {
        path: 'messages',
        loadComponent: () => import('./messages/messages.component').then(m => m.MessagesComponent)
    },
    {
        path: 'services',
        loadComponent: () => import('./services/services.component').then(m => m.ServicesComponent)
    },
    {
        path: 'packages',
        redirectTo: 'services',
        pathMatch: 'full'
    },
    {
        path: 'portfolio',
        loadComponent: () => import('./portfolio/portfolio.component').then(m => m.PortfolioComponent)
    },
    {
        path: 'analytics',
        loadComponent: () => import('./analytics/analytics.component').then(m => m.AnalyticsComponent)
    },
    {
        path: 'settings',
        loadComponent: () => import('./settings/settings.component').then(m => m.SettingsComponent)
    }
];
