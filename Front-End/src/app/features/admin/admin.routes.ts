import { Routes } from '@angular/router';
import { TicketDetailComponent } from './support/ticket-detail/ticket-detail.component';

export const ADMIN_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
    },
    {
        path: 'vendors',
        loadComponent: () => import('./vendors/vendors.component').then(m => m.VendorsComponent)
    },
    {
        path: 'vendors/create',
        loadComponent: () => import('./vendors/vendor-create/vendor-create.component').then(m => m.VendorCreateComponent)
    },
    {
        path: 'vendors/:id',
        loadComponent: () => import('./vendors/vendor-detail/vendor-detail.component').then(m => m.VendorDetailComponent)
    },
    {
        path: 'users',
        loadComponent: () => import('./users/users.component').then(m => m.UsersComponent)
    },
    {
        path: 'bookings',
        loadComponent: () => import('./bookings/bookings.component').then(m => m.BookingsComponent)
    },
    {
        path: 'financials',
        loadComponent: () => import('./financials/financials.component').then(m => m.FinancialsComponent)
    },
    {
        path: 'content',
        loadComponent: () => import('./content/content.component').then(m => m.ContentComponent)
    },
    {
        path: 'support',
        loadComponent: () => import('./support/support.component').then(m => m.SupportComponent)
    },
    {
        path: 'support/:id',
        component: TicketDetailComponent
    },
    {
        path: 'settings',
        loadComponent: () => import('./settings/settings.component').then(m => m.SettingsComponent)
    },
    {
        path: 'activity',
        loadComponent: () => import('./activity/activity.component').then(m => m.ActivityComponent)
    },
    {
        path: 'packages',
        loadComponent: () => import('./packages/packages.component').then(m => m.PackagesComponent)
    },
    {
        path: 'payouts',
        loadComponent: () => import('./payouts/payouts.component').then(m => m.PayoutsComponent)
    },
    {
        path: 'reports',
        loadComponent: () => import('./reports/reports.component').then(m => m.ReportsComponent)
    },
    {
        path: 'categories',
        loadComponent: () => import('./categories/categories.component').then(m => m.CategoriesComponent)
    },
    {
        path: 'company-inquiries',
        loadComponent: () => import('./company-inquiries/company-inquiries.component').then(m => m.CompanyInquiriesComponent)
    },
    {
        path: 'notifications',
        loadComponent: () => import('./notifications/notifications.component').then(m => m.NotificationsComponent)
    }
];
