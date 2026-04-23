import { Routes } from '@angular/router';

export const PUBLIC_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./home/home.component').then(m => m.HomeComponent)
    },
    {
        path: 'explore',
        loadComponent: () => import('./explore/explore.component').then(m => m.ExploreComponent)
    },
    {
        path: 'explore-services',
        loadComponent: () => import('./explore-services/explore-services.component').then(m => m.ExploreServicesComponent)
    },
    {
        path: 'packages',
        loadComponent: () => import('./packages/packages.component').then(m => m.PackagesComponent)
    },
    {
        path: 'corporate',
        loadComponent: () => import('./corporate/corporate.component').then(m => m.CorporateComponent)
    },
    {
        path: 'contact',
        loadComponent: () => import('./contact/contact.component').then(m => m.ContactComponent)
    },
    {
        path: 'vendor-join',
        loadComponent: () => import('./vendor-join/vendor-join.component').then(m => m.VendorJoinComponent)
    },
    {
        path: 'favorites',
        loadComponent: () => import('./favorites/favorites.component').then(m => m.FavoritesComponent)
    },
    {
        path: 'compare',
        loadComponent: () => import('./compare/compare.component').then(m => m.CompareComponent)
    },
    {
        path: 'vendor/:id',
        loadComponent: () => import('./vendor-profile/vendor-profile.component').then(m => m.VendorProfileComponent)
    },
    {
        path: 'login',
        redirectTo: ''
    },
    {
        path: 'forgot-password',
        loadComponent: () => import('../auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
    },
    {
        path: 'reset-password',
        loadComponent: () => import('../auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
    },
    {
        path: 'Authentication/reset-password',
        redirectTo: 'reset-password'
    }
];
